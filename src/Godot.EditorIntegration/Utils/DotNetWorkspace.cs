using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Godot.EditorIntegration.Utils;

internal sealed class DotNetWorkspace : IDisposable
{
    private readonly MSBuildWorkspace _workspace;

    private readonly Compilation _compilation;

    private DotNetWorkspace(MSBuildWorkspace workspace, Compilation compilation)
    {
        _workspace = workspace;
        _compilation = compilation;
    }

    public static async Task<DotNetWorkspace> OpenAsync(string path, CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create();

        var project = await workspace.OpenProjectAsync(path, cancellationToken: cancellationToken).ConfigureAwait(false);

        var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        if (compilation is null)
        {
            throw new InvalidOperationException($"Could not get compilation for project at '{path}'.");
        }

        return new DotNetWorkspace(workspace, compilation);
    }

    public Document? GetDocumentForSyntax(SyntaxNode syntaxNode)
    {
        return _workspace.CurrentSolution.GetDocument(syntaxNode.SyntaxTree);
    }

    public IEnumerable<ITypeSymbol> FindTypeSymbols(string typeName)
    {
        return FindTypeSymbolsCore(typeName, _compilation.GlobalNamespace);
    }

    private static IEnumerable<ITypeSymbol> FindTypeSymbolsCore(string typeName, INamespaceOrTypeSymbol namespaceOrTypeSymbol)
    {
        foreach (var member in namespaceOrTypeSymbol.GetMembers())
        {
            if (member is ITypeSymbol typeSymbol && typeSymbol.Name == typeName)
            {
                if (typeSymbol.HasAttribute(KnownTypeNames.GodotClassAttribute))
                {
                    yield return typeSymbol;
                }
            }

            if (member is INamespaceOrTypeSymbol nestedNamespace)
            {
                var nestedTypes = FindTypeSymbolsCore(typeName, nestedNamespace);
                foreach (var nestedType in nestedTypes)
                {
                    yield return nestedType;
                }
            }
        }
    }

    public bool TryApplyChanges(Solution newSolution)
    {
        return _workspace.TryApplyChanges(newSolution);
    }

    public void Dispose()
    {
        _workspace.Dispose();
    }
}

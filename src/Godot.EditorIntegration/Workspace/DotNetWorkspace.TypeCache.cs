using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.EditorIntegration.Workspace;

partial class DotNetWorkspace
{
    private readonly ConcurrentDictionary<string, ImmutableArray<INamedTypeSymbol>> _typeCache = [];

    private readonly ConcurrentDictionary<DocumentId, ImmutableArray<INamedTypeSymbol>> _typeCacheByDocument = [];

    public bool TryGetCachedTypeSymbolsForDocument(DocumentId documentId, [NotNullWhen(true)] out ImmutableArray<INamedTypeSymbol> symbols)
    {
        return _typeCacheByDocument.TryGetValue(documentId, out symbols);
    }

    private async Task CacheTypeSymbolsAsync(CancellationToken cancellationToken = default)
    {
        var compilation = _compilation;
        if (compilation is null)
        {
            return;
        }

        try
        {
            await CacheTypeSymbolsAsyncCore(compilation.Assembly.GlobalNamespace, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Caching is a best-effort optimization, so we can just stop if it's taking too long.
        }

        async Task CacheTypeSymbolsAsyncCore(INamespaceOrTypeSymbol namespaceOrTypeSymbol, CancellationToken cancellationToken = default)
        {
            foreach (var member in namespaceOrTypeSymbol.GetTypeMembers())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (member is INamedTypeSymbol typeSymbol)
                {
                    if (typeSymbol.HasAttribute(KnownTypeNames.GodotClassAttribute))
                    {
                        _typeCache.AddOrUpdate(typeSymbol.Name,
                            _ => [typeSymbol],
                            (_, symbols) => symbols.Add(typeSymbol));

                        foreach (var location in typeSymbol.Locations)
                        {
                            if (!location.IsInSource)
                            {
                                continue;
                            }

                            string filePath = location.SourceTree.FilePath;
                            var document = _solution?
                                .GetProject(_projectId)?
                                .GetDocument(location.SourceTree);
                            if (document is null)
                            {
                                continue;
                            }

                            _typeCacheByDocument.AddOrUpdate(
                                document.Id,
                                _ => [typeSymbol],
                                (_, symbols) => symbols.Add(typeSymbol));
                        }
                    }
                }

                if (member is INamespaceOrTypeSymbol nestedNamespace)
                {
                    await CacheTypeSymbolsAsyncCore(nestedNamespace, cancellationToken);
                }
            }
        }
    }

    /// <summary>
    /// Incrementally updates <see cref="_typeCache"/> for a single document without
    /// re-traversing the entire compilation. To be called after <see cref="_solution"/>
    /// is updated with new document text, and after a new document is added or removed.
    /// </summary>
    private async Task CacheTypeSymbolsForDocumentAsync(DocumentId documentId, CancellationToken cancellationToken = default)
    {
        if (_solution is null)
        {
            return;
        }

        if (documentId.ProjectId != _projectId)
        {
            // The changed document doesn't belong to our project, so we can ignore it.
            return;
        }

        var project = _solution.GetProject(_projectId);
        if (project is null)
        {
            // This should be unreachable.
            return;
        }

        // Incrementally recompile only the affected document.
        var newCompilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        if (newCompilation is null)
        {
            // Something went wrong, so we won't be able to cache the types.
            return;
        }
        _compilation = newCompilation;

        // Evict all previously cached symbols that came from this document.
        if (_typeCacheByDocument.TryRemove(documentId, out var oldSymbols))
        {
            // Partial types span multiple files: the same symbol instance appears in
            // _typeCacheByDocument for every file that contributes a partial declaration.
            // Incremental per-document eviction can't safely handle that case, so fall
            // back to a full reload instead.
            if (oldSymbols.Any(s => s.Locations.Length > 1))
            {
                ScheduleReload();
                return;
            }

            foreach (var oldSymbol in oldSymbols)
            {
                _typeCache.AddOrUpdate(oldSymbol.Name,
                    _ => [],
                    (_, symbols) => symbols.Remove(oldSymbol));
            }
        }

        // The document may have been deleted. In that case there's nothing to add back.
        var document = _solution.GetDocument(documentId);
        if (document is null)
        {
            return;
        }

        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxRoot is null)
        {
            return;
        }

        // Obtain a semantic model scoped to this document's syntax tree.
        var semanticModel = newCompilation.GetSemanticModel(syntaxRoot.SyntaxTree);

        // We only care about class declarations. The [GodotClass] attribute can't be applied to other kinds of types.
        var classDeclarationSyntaxes = syntaxRoot
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>();

        var newSymbols = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var symbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken);
            if (symbol is null)
            {
                continue;
            }

            if (!symbol.HasAttribute(KnownTypeNames.GodotClassAttribute))
            {
                continue;
            }

            _typeCache.AddOrUpdate(symbol.Name,
                _ => [symbol],
                (_, symbols) => symbols.Add(symbol));
            newSymbols.Add(symbol);
        }

        _typeCacheByDocument[documentId] = newSymbols.DrainToImmutable();
    }

    private void ClearTypeCache()
    {
        _typeCache.Clear();
        _typeCacheByDocument.Clear();
    }
}

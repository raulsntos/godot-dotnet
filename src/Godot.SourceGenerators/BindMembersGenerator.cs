using System.Text;
using System.Threading;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators;

[Generator]
internal sealed class BindMembersGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<GodotClassSpec?> specs = context.SyntaxProvider
            .ForAttributeWithMetadataName(KnownTypeNames.GodotClassAttribute,
                predicate: IsSyntaxTargetForGeneration,
                transform: GetSemanticTargetForGeneration)
            .Where(spec => spec is not null);

        context.RegisterSourceOutput(specs, Execute);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken cancellationToken = default)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static GodotClassSpec? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken = default)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        return ClassSpecCollector.Collect(context.SemanticModel.Compilation, typeSymbol, cancellationToken);
    }

    private static void Execute(SourceProductionContext context, GodotClassSpec? spec)
    {
        if (spec is null)
        {
            return;
        }

        string hintName = $"{spec.Value.GetHintName()}.generated.cs";

        var sb = new IndentedStringBuilder();
        BindMembersWriter.Write(sb, spec.Value);

        var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
        context.AddSource(hintName, sourceText);
    }
}

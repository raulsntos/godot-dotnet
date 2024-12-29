using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators;

[Generator]
internal sealed class BindMethodsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<GodotClassSpec> specs = context.SyntaxProvider
            .ForAttributeWithMetadataName(KnownTypeNames.GodotClassAttribute,
                predicate: IsSyntaxTargetForGeneration,
                transform: GetSemanticTargetForGeneration)
            .Where(spec => spec.HasValue)
            .Select((spec, token) => spec!.Value);

        IncrementalValuesProvider<Diagnostic> diagnostics = specs
            .Where(spec => spec.State != GodotClassSpec.StateType.Valid)
            .Select((spec, token) => spec.GetDiagnostic()!);

        context.RegisterSourceOutput(specs, Execute);
        context.RegisterSourceOutput(diagnostics, OutputDiagnostics);
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

    private static void Execute(SourceProductionContext context, GodotClassSpec spec)
    {
        string hintName = $"{spec.GetHintName()}.generated.cs";

        var sb = new IndentedStringBuilder();
        BindMethodsWriter.Write(sb, spec);

        var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
        context.AddSource(hintName, sourceText);
    }

    private static void OutputDiagnostics(SourceProductionContext context, Diagnostic diagnostic)
    {
        context.ReportDiagnostic(diagnostic);
    }
}

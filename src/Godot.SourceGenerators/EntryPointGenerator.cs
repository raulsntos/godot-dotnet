using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators;

[Generator]
internal sealed class EntryPointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<string> assemblyName = context.CompilationProvider.Select((compilation, ct) =>
        {
            return compilation.AssemblyName ?? "GDExtension";
        });

        IncrementalValueProvider<bool> disableGodotEntryPointGeneration = context.SyntaxProvider
            .ForAttributeWithMetadataName(KnownTypeNames.DisableGodotEntryPointGenerationAttribute,
                // DisableGodotEntryPointGenerationAttribute is only available at the top level.
                predicate: (node, ct) => true,
                transform: (context, ct) => true)
            .Collect()
            .Select((topLevelAttrs, ct) => !topLevelAttrs.IsEmpty);

        IncrementalValueProvider<ImmutableArray<GodotRegistrationSpec?>> registrationSpecs = context.SyntaxProvider
            .ForAttributeWithMetadataName(KnownTypeNames.GodotClassAttribute,
                predicate: IsSyntaxTargetForGeneration,
                transform: GetSemanticTargetForGeneration)
            .Where(spec => spec is not null)
            .Collect();

        var assemblySpec = assemblyName
            .Combine(disableGodotEntryPointGeneration)
            .Combine(registrationSpecs)
            .Select((provider, ct) =>
            {
                return new AssemblySpec()
                {
                    Name = provider.Left.Left,
                    Types = [.. provider.Right.OfType<GodotRegistrationSpec>()],
                    DisableGodotEntryPointGeneration = provider.Left.Right,
                };
            });

        context.RegisterSourceOutput(assemblySpec, Execute);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken cancellationToken = default)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static GodotRegistrationSpec? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken = default)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        return RegistrationSpecCollector.Collect(context.SemanticModel.Compilation, typeSymbol, cancellationToken);
    }

    private static void Execute(SourceProductionContext context, AssemblySpec spec)
    {
        const string HintName = "Main.generated.cs";

        var sb = new IndentedStringBuilder();
        EntryPointWriter.Write(sb, spec);

        var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
        context.AddSource(HintName, sourceText);
    }
}

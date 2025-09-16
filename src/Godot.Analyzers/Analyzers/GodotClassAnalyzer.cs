using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class GodotClassAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0101_GodotClassMustDeriveFromGodotObject,
        Descriptors.GODOT0102_GodotClassMustNotBeGeneric,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (!context.Symbol.HasAttribute(KnownTypeNames.GodotClassAttribute))
        {
            return;
        }

        INamedTypeSymbol symbol = (INamedTypeSymbol)context.Symbol;

        if (!symbol.DerivesFrom(KnownTypeNames.GodotObject))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0101_GodotClassMustDeriveFromGodotObject,
                symbol.Locations[0],
                // Message Format parameters.
                symbol.ToDisplayString()
            ));
        }

        if (symbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0102_GodotClassMustNotBeGeneric,
                symbol.Locations[0],
                // Message Format parameters.
                symbol.ToDisplayString()
            ));
        }
    }
}

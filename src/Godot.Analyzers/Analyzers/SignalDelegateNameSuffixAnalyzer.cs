using System;
using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SignalDelegateNameSuffixAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0704_SignalDelegateMissingSuffix,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (!context.Symbol.HasAttribute(KnownTypeNames.SignalAttribute))
        {
            return;
        }

        INamedTypeSymbol symbol = (INamedTypeSymbol)context.Symbol;

        if (!symbol.Name.EndsWith("EventHandler", StringComparison.Ordinal))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0704_SignalDelegateMissingSuffix,
                symbol.Locations[0],
                // Message Format parameters.
                symbol.ToDisplayString()
            ));
        }
    }
}

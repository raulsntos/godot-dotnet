using System.Collections.Immutable;
using System.Diagnostics;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SignalDelegateParameterTypesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0701_SignalParameterTypeIsNotSupported,
        Descriptors.GODOT0702_SignalShouldBeVoidReturnType,
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

        if (symbol.TypeKind != TypeKind.Delegate)
        {
            // The [Signal] attribute can only be applied to delegate types, so this should be unreachable.
            return;
        }

        IMethodSymbol? delegateSymbol = symbol.DelegateInvokeMethod;
        if (delegateSymbol is null)
        {
            Debug.Fail($"DelegateInvokeMethod is null for delegate type '{symbol}'.");
            return;
        }

        if (!delegateSymbol.ReturnsVoid)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0702_SignalShouldBeVoidReturnType,
                symbol.Locations[0],
                // Message Format parameters.
                symbol.ToDisplayString()
            ));
        }

        foreach (var parameterSymbol in delegateSymbol.Parameters)
        {
            if (!IsSupportedParameterType(context.Compilation, parameterSymbol.Type))
            {
                var location = parameterSymbol.GetTypeSyntaxLocation();
                if (location is null)
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.GODOT0701_SignalParameterTypeIsNotSupported,
                    location,
                    // Message Format parameters.
                    parameterSymbol.Type.ToDisplayString()
                ));
            }
        }
    }

    private static bool IsSupportedParameterType(Compilation compilation, ITypeSymbol type)
    {
        // Every signal parameter must be Variant compatible.
        return Marshalling.TryGetMarshallingInformation(compilation, type, out _);
    }
}

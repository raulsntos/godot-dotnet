using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BindMethodAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0601_MethodParameterTypeIsNotSupported,
        Descriptors.GODOT0601_MethodReturnTypeIsNotSupported,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (!context.Symbol.HasAttribute(KnownTypeNames.BindMethodAttribute))
        {
            return;
        }

        IMethodSymbol methodSymbol = (IMethodSymbol)context.Symbol;

        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            if (!IsSupportedParameterType(context.Compilation, parameterSymbol.Type))
            {
                var location = parameterSymbol.GetTypeSyntaxLocation();
                if (location is null)
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.GODOT0601_MethodParameterTypeIsNotSupported,
                    location,
                    // Message Format parameters.
                    parameterSymbol.Type.ToDisplayString()
                ));
            }
        }

        if (!methodSymbol.ReturnsVoid)
        {
            if (!IsSupportedParameterType(context.Compilation, methodSymbol.ReturnType))
            {
                var location = methodSymbol.GetTypeSyntaxLocation();
                if (location is null)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.GODOT0601_MethodReturnTypeIsNotSupported,
                    location,
                    // Message Format parameters.
                    methodSymbol.ReturnType.ToDisplayString()
                ));
            }
        }
    }

    private static bool IsSupportedParameterType(Compilation compilation, ITypeSymbol type)
    {
        // Every method parameter must be Variant compatible.
        return Marshalling.TryGetMarshallingInformation(compilation, type, out _);
    }
}

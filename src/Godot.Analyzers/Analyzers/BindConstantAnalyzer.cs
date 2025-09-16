using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BindConstantAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0301_ConstantTypeIsNotSupported,
        Descriptors.GODOT0302_ConstantMustBeConst,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (!context.Symbol.HasAttribute(KnownTypeNames.BindConstantAttribute))
        {
            return;
        }

        IFieldSymbol fieldSymbol = (IFieldSymbol)context.Symbol;

        if (!IsSupportedConstantType(fieldSymbol.Type))
        {
            var location = fieldSymbol.GetTypeSyntaxLocation();
            if (location is null)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0301_ConstantTypeIsNotSupported,
                location,
                // Message Format parameters.
                fieldSymbol.Type.ToDisplayString()
            ));
        }

        if (!fieldSymbol.IsConst)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0302_ConstantMustBeConst,
                fieldSymbol.Locations[0],
                // Message Format parameters.
                fieldSymbol.Name
            ));
        }
    }

    private static bool IsSupportedConstantType(ITypeSymbol type)
    {
        // These are the only types supported by Godot for constants,
        // although it always uses Int64 as the underlying type.
        return type.SpecialType switch
        {
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            _ => false,
        };
    }
}

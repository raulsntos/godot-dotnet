using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.UpgradeAssistant.Providers;

[RequiresGodotDotNet]
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AddGodotClassAttributeAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1006_AddGodotClassAttribute;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Rule];

    public override void Initialize(DiagnosticAnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)context.Symbol;

        if (!symbol.DerivesFromGodotSharpObject())
        {
            return;
        }

        if (symbol.ContainingType is not null)
        {
            // If the type is nested, it could not have been used as a script with GodotSharp.
            return;
        }

        if (symbol.IsGenericType)
        {
            // If the type is generic, it could not have been used as a script with GodotSharp.
            return;
        }

        if (symbol.HasUnknownAttribute(KnownTypeNames.GodotClassAttribute))
        {
            // The class is already marked with [GodotClass], so there's no issue.
            return;
        }

        var classDeclarationSyntax = symbol.GetInSourceDeclarationSyntax<ClassDeclarationSyntax>();
        if (classDeclarationSyntax is null)
        {
            // Could not find the class' declaration in source code, it must be a generated type.
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Rule,
            location: classDeclarationSyntax.Identifier.GetLocation(),
            // Message Format parameters.
            symbol.ToDisplayString()));
    }
}

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.UpgradeAssistant.Providers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AddPartialKeywordToGodotClassesAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1001_AddPartialKeywordToGodotClasses;

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

        var classDeclarationSyntaxes = symbol.DeclaringSyntaxReferences
            .Select(s => s.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .Where(s => s.GetLocation().IsInSource)
            .ToArray();

        if (classDeclarationSyntaxes.Length == 0)
        {
            // Could not find the class' declaration, the type must not be a class.
            return;
        }

        // Check if the class is already declared as partial.
        foreach (ClassDeclarationSyntax? classDeclarationSyntax in classDeclarationSyntaxes)
        {
            if (classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
            {
                return;
            }
        }

        // If the class is not declared partial there should only be one declaration.
        Debug.Assert(classDeclarationSyntaxes.Length == 1);

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Rule,
            location: classDeclarationSyntaxes.Single().Identifier.GetLocation(),
            // Message Format parameters.
            symbol.ToDisplayString()));
    }
}

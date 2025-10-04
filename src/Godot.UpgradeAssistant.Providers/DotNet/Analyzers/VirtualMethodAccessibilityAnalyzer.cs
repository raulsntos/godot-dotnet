using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.UpgradeAssistant.Providers;

[RequiresGodotDotNet]
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class VirtualMethodAccessibilityAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1009_VirtualMethodAccessibility;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Rule];

    public override void Initialize(DiagnosticAnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetDeclaredSymbol(methodDeclarationSyntax, context.CancellationToken) is not IMethodSymbol methodSymbol)
        {
            // Unable to get the method symbol.
            return;
        }

        if (!methodSymbol.OverridesFromGodotSharp())
        {
            // We only care about methods that override virtual methods from the GodotSharp assembly.
            return;
        }

        if (methodSymbol.DeclaredAccessibility == Accessibility.Protected)
        {
            // Virtual method override is already protected.
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Rule,
            location: methodDeclarationSyntax.Identifier.GetLocation(),
            // Message Format parameters.
            methodSymbol.ToDisplayString()));
    }
}

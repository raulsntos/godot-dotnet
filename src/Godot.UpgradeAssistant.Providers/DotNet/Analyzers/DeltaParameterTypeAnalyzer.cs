using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.UpgradeAssistant.Providers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class DeltaParameterTypeAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1010_DeltaParameterType;

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
        if (semanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is not IMethodSymbol methodSymbol)
        {
            // Unable to get the method symbol.
            return;
        }

        if (!methodSymbol.IsOverride)
        {
            // We only care about virtual method overrides.
            return;
        }

        var originalMethod = methodSymbol.GetOverrideOriginalSymbol();

        if (!originalMethod.ContainingType.DerivesFrom("Godot.Node", "GodotSharp"))
        {
            // The methods we are looking for are declared in the Node type.
            return;
        }

        if (methodSymbol.Name.Equals("_Process", StringComparison.Ordinal)
         || methodSymbol.Name.Equals("_PhysicsProcess", StringComparison.Ordinal))
        {
            if (methodSymbol.Parameters.Length != 1)
            {
                // The methods we are looking for only have one parameter.
                return;
            }

            var deltaParameter = originalMethod.Parameters[0];
            if (deltaParameter.Name != "delta")
            {
                // The parameter name doesn't match the expected name.
                return;
            }

            SpecialType expectedDeltaType = context.Options.IsGodotDotNetEnabled()
                // TODO: Change to System_Single when we change delta to float in godot-dotnet.
                ? SpecialType.System_Double
                : SpecialType.System_Double;

            if (deltaParameter.Type.SpecialType == expectedDeltaType)
            {
                // The 'delta' parameter already matches the expected type.
                return;
            }

            var properties = new Dictionary<string, string?>();
            if (expectedDeltaType == SpecialType.System_Single)
            {
                properties[PropertyNames.DeltaIsFloat32] = "true";
            }

            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: Rule,
                location: methodDeclarationSyntax.Identifier.GetLocation(),
                properties: properties.ToImmutableDictionary(),
                // Message Format parameters.
                methodSymbol.ToDisplayString()));
        }
    }
}

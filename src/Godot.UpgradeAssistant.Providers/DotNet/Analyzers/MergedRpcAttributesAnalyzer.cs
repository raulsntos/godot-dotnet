using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.UpgradeAssistant.Providers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class MergedRpcAttributesAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1005_MergedRpcAttributes;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Rule];

    public override void Initialize(DiagnosticAnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol methodSymbol)
        {
            // Unable to get the attribute constructor symbol.
            return;
        }

        var symbol = methodSymbol.ContainingType;
        if (symbol is null)
        {
            // Unable to get the attribute symbol.
            return;
        }

        // Godot.MasterAttribute, Godot.MasterSyncAttribute are unsupported and won't be converted,
        // they are handled by ApiMapAnalyzer.
        if (symbol.EqualsType("Godot.RemoteAttribute", "GodotSharp")
         || symbol.EqualsType("Godot.RemoteSyncAttribute", "GodotSharp")
         || symbol.EqualsType("Godot.SyncAttribute", "GodotSharp")
         || symbol.EqualsType("Godot.SlaveAttribute", "GodotSharp")
         || symbol.EqualsType("Godot.PuppetAttribute", "GodotSharp")
         || symbol.EqualsType("Godot.PuppetSyncAttribute", "GodotSharp"))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                descriptor: Rule,
                location: attributeSyntax.GetLocation(),
                // Message Format parameters.
                symbol.ToDisplayString()));
        }
    }
}

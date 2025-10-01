using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.UpgradeAssistant.Providers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal sealed partial class ApiMapCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor NotImplementedRule =>
        Descriptors.GUA1002_NotImplementedSymbol;

    private static DiagnosticDescriptor RemovedRule =>
        Descriptors.GUA1003_RemovedSymbol;

    private static DiagnosticDescriptor ReplacedRule =>
        Descriptors.GUA1004_ReplacedSymbol;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [
            NotImplementedRule.Id,
            RemovedRule.Id,
            ReplacedRule.Id,
        ];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the syntax node identified by the diagnostic.
        var syntaxNode = root?
            .FindNode(diagnosticSpan)
            .DescendantNodesAndSelf()
            .FirstOrDefault(s =>
                s is IdentifierNameSyntax
                  or GenericNameSyntax
                  or MemberAccessExpressionSyntax
                  or AttributeSyntax
                  or MethodDeclarationSyntax);

        if (syntaxNode is null)
        {
            // Can't apply the code fix without syntax.
            return;
        }

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        var symbol = syntaxNode switch
        {
            ExpressionSyntax => semanticModel?.GetSymbolInfo(syntaxNode, context.CancellationToken).Symbol,
            MethodDeclarationSyntax => semanticModel?.GetDeclaredSymbol(syntaxNode, context.CancellationToken),
            _ => null,
        };
        if (symbol is null)
        {
            // Can't find mapping without the symbol.
            return;
        }

        if (symbol is IMethodSymbol { IsOverride: true } methodSymbol)
        {
            // For method overrides we want the original virtual method that is being overridden.
            symbol = methodSymbol.GetOverrideOriginalSymbol();
        }

        var targetGodotVersion = Constants.LatestGodotVersion;
        if (diagnostic.Properties.TryGetValue(PropertyNames.TargetGodotVersion, out string? targetGodotVersionValue))
        {
            targetGodotVersion = SemVer.Parse(targetGodotVersionValue!);
        }

        bool isGodotDotNetEnabled = false;
        if (diagnostic.Properties.TryGetValue(PropertyNames.IsGodotDotNetEnabled, out string? isGodotDotNetEnabledValue)
         && (isGodotDotNetEnabledValue?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            isGodotDotNetEnabled = true;
        }

        var mapping = await ApiMapUtils.GetApiEntryForSymbolAsync(symbol, targetGodotVersion, isGodotDotNetEnabled, context.CancellationToken).ConfigureAwait(false);
        if (mapping is null)
        {
            // Mapping not found, return the document unchanged.
            return;
        }

        string codeFixTitle = mapping.State switch
        {
            ApiMapState.NotImplemented => SR.GUA1002_NotImplementedSymbol_CodeFix,
            ApiMapState.Removed => SR.GUA1003_RemovedSymbol_CodeFix,
            ApiMapState.Replaced => SR.FormatGUA1004_ReplacedSymbol_CodeFix(mapping.Value),
            _ => throw new InvalidOperationException(SR.FormatInvalidOperation_ApiMapEntryStateIsInvalid(mapping.State)),
        };

        bool fixesDiagnostic = mapping.State switch
        {
            // Code actions for removed or not implemented APIs never fix the diagnostic.
            ApiMapState.NotImplemented or ApiMapState.Removed => false,

            // Code actions for replaced APIs should fix the diagnostic unless it requires manual work.
            ApiMapState.Replaced => !mapping.NeedsManualUpgrade,

            _ =>
                throw new InvalidOperationException(SR.FormatInvalidOperation_ApiMapEntryStateIsInvalid(mapping.State)),
        };

        var codeAction = CodeAction.Create(
            title: codeFixTitle,
            createChangedDocument: cancellationToken =>
                ApplyFix(context.Document, syntaxNode, mapping, cancellationToken),
            equivalenceKey: nameof(ApiMapCodeFix))
            .WithUpgradeMetadata(new()
            {
                FixesDiagnostic = fixesDiagnostic,
            });

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, SyntaxNode syntaxNode, ApiMapEntry mapping, CancellationToken cancellationToken = default)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        HashSet<string> newNamespaces = [];

        SyntaxNode? newRoot = mapping.Kind switch
        {
            ApiMapKind.Field or
            ApiMapKind.Property or
            ApiMapKind.Method or
            ApiMapKind.Event =>
                ApplyFixToMember(root, syntaxNode, mapping, newNamespaces),

            ApiMapKind.Namespace =>
                ApplyFixToNamespace(root, syntaxNode, mapping, newNamespaces),

            ApiMapKind.Type =>
                ApplyFixToType(root, syntaxNode, mapping, newNamespaces),

            _ => throw new InvalidOperationException(SR.FormatInvalidOperation_ApiMapEntryKindIsInvalid(mapping.Kind)),
        };

        if (newRoot is null || newRoot == root)
        {
            // If we have no new root or it's the same as the original, then there was no changes.
            // Return the document unchanged.
            return document;
        }

        // Add using directives for the namespace of the new APIs in scope.
        foreach (string newNamespace in newNamespaces)
        {
            if (string.IsNullOrWhiteSpace(newNamespace))
            {
                continue;
            }

            newRoot = SyntaxUtils.AddUsingDirective(newRoot, newNamespace);
        }

        return document.WithSyntaxRoot(newRoot);
    }
}

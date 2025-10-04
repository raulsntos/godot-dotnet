using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.UpgradeAssistant.Providers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal sealed class VirtualMethodAccessibilityCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1009_VirtualMethodAccessibility;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [Rule.Id];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the method declaration identified by the diagnostic.
        var declaration = root?
            .FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .First();

        if (declaration is null)
        {
            // Can't apply the code fix without a declaration.
            return;
        }

        var codeAction = CodeAction.Create(
            title: SR.GUA1009_VirtualMethodAccessibility_CodeFix,
            equivalenceKey: nameof(VirtualMethodAccessibilityCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, declaration, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, MethodDeclarationSyntax methodDeclarationSyntax, CancellationToken cancellationToken = default)
    {
        List<SyntaxToken> newModifiers = [];
        foreach (var modifier in methodDeclarationSyntax.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PublicKeyword))
            {
                // When we find the 'public' keyword token, replace it with 'protected'.
                newModifiers.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
            }
            else
            {
                // Add other tokens unchanged.
                newModifiers.Add(modifier);
            }
        }

        var newSyntaxNode = methodDeclarationSyntax.WithModifiers(SyntaxFactory.TokenList(newModifiers)).WithTriviaFrom(methodDeclarationSyntax);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var newRoot = root.ReplaceNode(methodDeclarationSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);
    }
}

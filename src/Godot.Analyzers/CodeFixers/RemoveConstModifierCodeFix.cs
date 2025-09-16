using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveConstModifierCodeFix))]
internal sealed class RemoveConstModifierCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create([
        Descriptors.GODOT0503_PropertyMustNotBeConst.Id,
    ]);

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var syntaxNode = root
            .FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<MemberDeclarationSyntax>()
            .FirstOrDefault();

        if (syntaxNode is null)
        {
            return;
        }

        var codeAction = CodeAction.Create(
            title: SR.GODOT0503_RemoveConstModifier_CodeFix,
            equivalenceKey: nameof(RemoveConstModifierCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, syntaxNode, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        if (declarationSyntax is not FieldDeclarationSyntax fieldDeclarationSyntax)
        {
            return document;
        }

        var modifiers = fieldDeclarationSyntax.Modifiers;

        var constModifier = modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.ConstKeyword));
        if (constModifier == default)
        {
            return document;
        }

        var newSyntaxNode = fieldDeclarationSyntax.WithModifiers(modifiers.Remove(constModifier));

        var newRoot = root.ReplaceNode(declarationSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);
    }
}

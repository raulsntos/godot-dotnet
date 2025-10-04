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
internal sealed class AddBindPropertyAttributeCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1007_AddBindPropertyAttribute;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [Rule.Id];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the member declaration identified by the diagnostic.
        var declaration = root?
            .FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<MemberDeclarationSyntax>()
            .First();

        if (declaration is null)
        {
            // Can't apply the code fix without a declaration.
            return;
        }

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        // Check if the declaration has the [Export] attribute to display a better title in the registered code fix.
        bool hasExportAttribute = declaration.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute =>
            {
                var attributeSymbolInfo = semanticModel.GetSymbolInfo(attribute);
                return attributeSymbolInfo.Symbol is INamedTypeSymbol attributeSymbol
                    && attributeSymbol.EqualsType("Godot.ExportAttribute", "GodotSharp");
            });

        var codeAction = CodeAction.Create(
            title: hasExportAttribute
                ? SR.GUA1007_AddBindPropertyAttributeReplaceExport_CodeFix
                : SR.GUA1007_AddBindPropertyAttribute_CodeFix,
            equivalenceKey: nameof(AddBindPropertyAttributeCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, declaration, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, MemberDeclarationSyntax memberDeclarationSyntax, CancellationToken cancellationToken = default)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        List<AttributeSyntax> attributesToRemove = [];

        ISymbol? symbol;
        if (memberDeclarationSyntax is FieldDeclarationSyntax fieldDeclarationSyntax)
        {
            // If the member is a field, we need to get the symbol from the variable declarator.
            var variableDeclaratorSyntax = fieldDeclarationSyntax.Declaration.Variables.FirstOrDefault();
            if (variableDeclaratorSyntax is null)
            {
                // If we couldn't get the variable declarator, return the document unchanged.
                // This should be unreachable.
                return document;
            }

            symbol = semanticModel.GetDeclaredSymbol(variableDeclaratorSyntax, cancellationToken);
        }
        else
        {
            symbol = semanticModel.GetDeclaredSymbol(memberDeclarationSyntax, cancellationToken);
        }

        if (symbol is null)
        {
            // If we couldn't get the symbol from the member declaration syntax, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        bool isExported = false;

        var attributes = symbol.GetAttributes();
        foreach (AttributeData attributeData in attributes)
        {
            var attributeSymbol = attributeData.AttributeClass;

            if (attributeSymbol.EqualsType("Godot.ExportAttribute", "GodotSharp"))
            {
                isExported = true;

                var attributeSyntax = attributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax;
                if (attributeSyntax is not null)
                {
                    attributesToRemove.Add(attributeSyntax);
                }
            }
        }

        var bindPropertyAttribute = CreateBindPropertyAttribute(isExported);

        var newMemberDeclarationSyntax = SyntaxUtils.RemoveAttributes(memberDeclarationSyntax, attributesToRemove);
        newMemberDeclarationSyntax = SyntaxUtils.AddAttributeList(newMemberDeclarationSyntax, bindPropertyAttribute);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var newRoot = root.ReplaceNode(memberDeclarationSyntax, newMemberDeclarationSyntax);
        return document.WithSyntaxRoot(newRoot);
    }

    private static AttributeSyntax CreateBindPropertyAttribute(bool isExported)
    {
        return SyntaxUtils.CreateAttribute("BindProperty");
    }
}

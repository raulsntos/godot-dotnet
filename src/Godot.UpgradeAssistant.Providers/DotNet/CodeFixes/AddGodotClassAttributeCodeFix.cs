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
internal sealed class AddGodotClassAttributeCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1006_AddGodotClassAttribute;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [Rule.Id];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the class declaration identified by the diagnostic.
        var declaration = root?
            .FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .First();

        if (declaration is null)
        {
            // Can't apply the code fix without a declaration.
            return;
        }

        var codeAction = CodeAction.Create(
            title: SR.GUA1006_AddGodotClassAttribute_CodeFix,
            equivalenceKey: nameof(AddGodotClassAttributeCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, declaration, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken = default)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        bool isTool = false;
        string? icon = null;

        List<AttributeSyntax> attributesToRemove = [];

        var symbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken);
        if (symbol is null)
        {
            // If we couldn't get the symbol from the class declaration syntax, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var attributes = symbol.GetAttributes();
        foreach (AttributeData attributeData in attributes)
        {
            var attributeSymbol = attributeData.AttributeClass;

            if (attributeSymbol.EqualsType("Godot.ToolAttribute", "GodotSharp"))
            {
                isTool = true;
            }

            if (attributeSymbol.EqualsType("Godot.IconAttribute", "GodotSharp"))
            {
                icon = attributeData.ConstructorArguments[0].Value as string;
            }

            if (attributeSymbol.EqualsType("Godot.ToolAttribute", "GodotSharp")
             || attributeSymbol.EqualsType("Godot.GlobalClassAttribute", "GodotSharp")
             || attributeSymbol.EqualsType("Godot.IconAttribute", "GodotSharp"))
            {
                var attributeSyntax = attributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax;
                if (attributeSyntax is not null)
                {
                    attributesToRemove.Add(attributeSyntax);
                }
            }
        }

        var godotClassAttribute = CreateGodotClassAttribute(isTool, icon);

        var newClassDeclarationSyntax = SyntaxUtils.RemoveAttributes(classDeclarationSyntax, attributesToRemove);
        newClassDeclarationSyntax = SyntaxUtils.AddAttributeList(newClassDeclarationSyntax, godotClassAttribute);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var newRoot = root.ReplaceNode(classDeclarationSyntax, newClassDeclarationSyntax);
        return document.WithSyntaxRoot(newRoot);
    }

    private static AttributeSyntax CreateGodotClassAttribute(bool isTool, string? icon)
    {
        List<AttributeArgumentSyntax> arguments = [];

        if (isTool)
        {
            arguments.Add(SyntaxUtils.CreateBooleanAttributeArgument("Tool", true));
        }

        if (!string.IsNullOrEmpty(icon))
        {
            arguments.Add(SyntaxUtils.CreateStringAttributeArgument("Icon", icon));
        }

        return SyntaxUtils.CreateAttribute("GodotClass", arguments);
    }
}

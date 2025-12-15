using System;
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

namespace Godot.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddToolToGodotClassCodeFix))]
internal sealed class AddToolToGodotClassCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create([
        Descriptors.GODOT0004_GodotClassWithEditorCallbacksShouldBeTool.Id,
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
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (syntaxNode is null)
        {
            return;
        }

        // Find the class declaration that contains this method.
        var classDeclarationSyntax = syntaxNode
            .Ancestors()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclarationSyntax is null)
        {
            return;
        }

        var godotClassAttribute = FindGodotClassAttribute(classDeclarationSyntax);
        if (godotClassAttribute is null)
        {
            return;
        }

        var codeAction = CodeAction.Create(
            title: SR.GODOT0004_AddToolToGodotClass_CodeFix,
            equivalenceKey: nameof(AddToolToGodotClassCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, godotClassAttribute, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, AttributeSyntax attributeSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var argumentList = attributeSyntax.ArgumentList;
        var newArgumentList = argumentList;

        var toolArgument = SyntaxUtils.CreateBooleanAttributeArgument("Tool", true);

        if (argumentList is not null)
        {
            var existingToolArgument = FindToolArgumentSyntax(argumentList);
            if (existingToolArgument is not null)
            {
                // Attribute already has a 'Tool' named argument.
                newArgumentList = argumentList.ReplaceNode(existingToolArgument, toolArgument);
            }
            else
            {
                // Attribute has arguments, but not the 'Tool' named argument.
                newArgumentList = argumentList.WithArguments([toolArgument]);
            }
        }
        else
        {
            // Attribute has no arguments.
            newArgumentList = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(toolArgument));
        }

        var newSyntaxNode = attributeSyntax.WithArgumentList(newArgumentList);

        var newRoot = root.ReplaceNode(attributeSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);

        static AttributeArgumentSyntax? FindToolArgumentSyntax(AttributeArgumentListSyntax argumentList)
        {
            foreach (var argumentSyntax in argumentList.Arguments)
            {
                if (argumentSyntax.NameEquals?.Name.Identifier.Text == "Tool")
                {
                    return argumentSyntax;
                }
            }
            return null;
        }
    }

    private static AttributeSyntax? FindGodotClassAttribute(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                string name = attribute.Name.ToString();
                if (name == "GodotClass"
                 || name == "GodotClassAttribute"
                 || name.EndsWith(".GodotClass", StringComparison.Ordinal)
                 || name.EndsWith(".GodotClassAttribute", StringComparison.Ordinal))
                {
                    return attribute;
                }
            }
        }

        return null;
    }
}

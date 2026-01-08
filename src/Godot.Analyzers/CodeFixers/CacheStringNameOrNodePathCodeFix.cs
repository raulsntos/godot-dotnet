using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Godot.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CacheStringNameOrNodePathCodeFix))]
internal sealed class CacheStringNameOrNodePathCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create([
        Descriptors.GODOT0005_AvoidImplicitStringConversion.Id,
    ]);

    // This code fix doesn't support 'Fix All' because it gets confused
    // when multiple string literals in the same document have the same value.
    public override FixAllProvider? GetFixAllProvider() =>
        null;

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
            .OfType<LiteralExpressionSyntax>()
            .FirstOrDefault();

        if (syntaxNode is null)
        {
            return;
        }

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        var operation = semanticModel.GetOperation(syntaxNode, context.CancellationToken);

        if (operation is not ILiteralOperation literalOperation)
        {
            return;
        }

        var typeInfo = semanticModel.GetTypeInfo(syntaxNode, context.CancellationToken);
        var targetType = typeInfo.ConvertedType;

        if (targetType is null)
        {
            var parentOperation = literalOperation.Parent;
            if (parentOperation is IConversionOperation parentConversion)
            {
                targetType = parentConversion.Type;
            }
        }

        if (targetType is null)
        {
            return;
        }

        string? stringValue = literalOperation.ConstantValue.Value as string;
        if (stringValue is null)
        {
            // Ignore null strings, but not empty strings.
            return;
        }

        var annotation = new SyntaxAnnotation();
        var annotatedNode = syntaxNode.WithAdditionalAnnotations(annotation);
        var newRoot = root.ReplaceNode(syntaxNode, annotatedNode);
        var newDocument = context.Document.WithSyntaxRoot(newRoot);

        var codeAction = CodeAction.Create(
            title: SR.GODOT0005_CacheStringNameOrNodePath_CodeFix,
            equivalenceKey: nameof(CacheStringNameOrNodePathCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(newDocument, annotation, stringValue, targetType, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, SyntaxAnnotation annotation, string stringValue, ITypeSymbol targetType, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Since this code fix modifies the syntax tree, once root is modified
        // the previous syntax node we had won't match anymore. We need to look in the
        // modified root to find the equivalent syntax node.
        var syntaxNode = root.GetAnnotatedNodes(annotation).FirstOrDefault();
        if (syntaxNode is null)
        {
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document;
        }

        // Find the containing type.
        var typeDeclaration = syntaxNode.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDeclaration is null)
        {
            return document;
        }
        var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);
        if (typeSymbol is null)
        {
            return document;
        }

        // Check if a field with the same value already exists.
        bool shouldAddCachedField = false;
        if (!TryFindExistingCachedField(typeSymbol, stringValue, targetType, out string? fieldName))
        {
            shouldAddCachedField = true;
            fieldName = GenerateUniqueFieldName(typeSymbol, stringValue);
        }

        // Replace the string literal with a reference to the cached field.
        // If the field doesn't exist yet, it will be added.
        var newSyntaxNode = SyntaxFactory.IdentifierName(fieldName);
        var newRoot = root.ReplaceNode(syntaxNode, newSyntaxNode);

        if (shouldAddCachedField)
        {
            var fieldDeclaration = CreateCachedFieldDeclaration(targetType, fieldName, stringValue);

            // Find the containing type again because we modified the syntax tree.
            typeDeclaration = newRoot.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault(td => td.Identifier.IsEquivalentTo(typeDeclaration.Identifier));
            if (typeDeclaration is null)
            {
                throw new InvalidOperationException("Containing type declaration not found after syntax tree modification.");
            }

            var firstMember = typeDeclaration.Members.FirstOrDefault();
            var newTypeDeclaration = firstMember is not null
                ? typeDeclaration.InsertNodesBefore(firstMember, [fieldDeclaration])
                : typeDeclaration.AddMembers(fieldDeclaration);
            newRoot = newRoot.ReplaceNode(typeDeclaration, newTypeDeclaration);
        }

        return document.WithSyntaxRoot(newRoot);
    }

    private static FieldDeclarationSyntax CreateCachedFieldDeclaration(ITypeSymbol typeSymbol, string fieldName, string stringValue)
    {
        // private static readonly StringName/NodePath FieldName = new StringName/NodePath("value");

        NameSyntax typeNameSyntax = SyntaxFactory.IdentifierName(typeSymbol.Name);

        var argumentList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList([
                SyntaxFactory.Argument(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(stringValue)))
            ]));

        var creationExpression = SyntaxFactory.ObjectCreationExpression(typeNameSyntax)
            .WithArgumentList(argumentList);

        var variableDeclarator = SyntaxFactory
            .VariableDeclarator(fieldName)
            .WithInitializer(SyntaxFactory.EqualsValueClause(creationExpression));

        var variableDeclaration = SyntaxFactory
            .VariableDeclaration(typeNameSyntax)
            .AddVariables(variableDeclarator);

        return SyntaxFactory.FieldDeclaration(variableDeclaration)
            .AddModifiers([
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword),
            ])
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithTrailingTrivia(SyntaxFactory.LineFeed, SyntaxFactory.LineFeed);
    }

    private static string GenerateUniqueFieldName(INamedTypeSymbol typeSymbol, string stringValue)
    {
        if (string.IsNullOrEmpty(stringValue))
        {
            return "Empty";
        }

        // Generate a field name from the string value.
        // Convert to PascalCase and remove invalid characters.
        var sb = new StringBuilder();
        bool capitalizeNext = true;
        foreach (char c in stringValue)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (capitalizeNext)
                {
                    sb.Append(char.ToUpperInvariant(c));
                    capitalizeNext = false;
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                capitalizeNext = true;
            }
        }

        string fieldName = sb.ToString();
        if (fieldName.Length == 0)
        {
            return "Empty";
        }

        // Ensure it starts with a valid identifier character.
        if (!char.IsLetter(fieldName[0]))
        {
            fieldName = $"_{fieldName}";
        }

        // Avoid conflicts by appending a counter if needed.
        var existingMembers = new HashSet<string>(
            typeSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Select(f => f.Name));

        string baseName = fieldName;
        int counter = 1;
        while (existingMembers.Contains(fieldName))
        {
            fieldName = $"{baseName}{counter}";
            counter++;
        }

        return fieldName;
    }

    private static bool TryFindExistingCachedField(INamedTypeSymbol containingTypeSymbol, string stringValue, ITypeSymbol typeSymbol, [NotNullWhen(true)] out string? fieldName)
    {
        foreach (var memberSymbol in containingTypeSymbol.GetMembers())
        {
            if (IsCachedField(memberSymbol, typeSymbol, stringValue))
            {
                fieldName = memberSymbol.Name;
                return true;
            }
        }

        fieldName = null;
        return false;

        // The member must be a static readonly field with the same type and initializer value.
        static bool IsCachedField(ISymbol memberSymbol, ITypeSymbol typeSymbol, string stringValue)
        {
            if (memberSymbol is not IFieldSymbol fieldSymbol)
            {
                return false;
            }

            if (!fieldSymbol.IsStatic)
            {
                return false;
            }

            if (!fieldSymbol.IsReadOnly)
            {
                return false;
            }

            if (!fieldSymbol.Type.EqualsType(typeSymbol.FullQualifiedNameOmitGlobal()))
            {
                return false;
            }

            foreach (var syntaxReference in fieldSymbol.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax();
                if (syntax is VariableDeclaratorSyntax variableDeclarator)
                {
                    if (IsMatchingInitializer(variableDeclarator.Initializer, typeSymbol, stringValue))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool IsMatchingInitializer(EqualsValueClauseSyntax? initializer, ITypeSymbol typeSymbol, string stringValue)
        {
            if (initializer is null)
            {
                return false;
            }

            // The initializer expression must be the StringName/NodePath constructor.
            if (initializer.Value is not ObjectCreationExpressionSyntax creationExpression)
            {
                return false;
            }

            // The constructor type must match.
            if (creationExpression.Type is not IdentifierNameSyntax nameSyntax
             || nameSyntax.Identifier.Text != typeSymbol.Name)
            {
                return false;
            }

            // The StringName/NodePath constructor is expected to take exactly one string argument.
            if (creationExpression.ArgumentList is null || creationExpression.ArgumentList.Arguments.Count != 1)
            {
                return false;
            }

            // The argument is expected to be a string literal with the same value.
            var argument = creationExpression.ArgumentList.Arguments[0];
            if (argument.Expression is not LiteralExpressionSyntax literal
             || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }
            return literal.Token.ValueText == stringValue;
        }
    }
}

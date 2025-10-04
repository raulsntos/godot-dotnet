using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.Common.CodeAnalysis;

/// <summary>
/// Helper methods for working with or creating Roslyn syntax nodes.
/// </summary>
internal static class SyntaxUtils
{
#if NET9_0_OR_GREATER
    /// <summary>
    /// Create a <see cref="QualifiedNameSyntax"/> to reference a type
    /// specified by the provided fully-qualified name.
    /// If <paramref name="typeArguments"/> is not <see langword="null"/>,
    /// the name will reference a generic type with the given type arguments.
    /// </summary>
    /// <remarks>
    /// This method handles splitting the fully-qualified name in multiple identifiers
    /// and merges them in a <see cref="QualifiedNameSyntax"/>, so we can
    /// easily create syntax from strings that reference types.
    /// </remarks>
    /// <param name="typeFullName">The fully-qualified name of the type.</param>
    /// <param name="typeArguments">The generic type arguments for the type.</param>
    /// <returns>The <see cref="QualifiedNameSyntax"/> node.</returns>
    public static NameSyntax CreateQualifiedName(string typeFullName, IEnumerable<TypeSyntax>? typeArguments = null)
    {
        NameSyntax? newSyntaxNode = null;

        var splitByAccessOperator = typeFullName.AsSpan().Split('.');
        foreach (var identifierRange in splitByAccessOperator)
        {
            // If the type is generic and this is will be the last indentifier, the name syntax node
            // has to be a generic name with the provided type arguments.
            SimpleNameSyntax nameSyntax;
            if (typeArguments is not null && IsLastIdentifier(identifierRange))
            {
                var typeArgumentList = SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(typeArguments));

                nameSyntax = SyntaxFactory.GenericName(typeFullName[identifierRange])
                    .WithTypeArgumentList(typeArgumentList);
            }
            else
            {
                nameSyntax = SyntaxFactory.IdentifierName(typeFullName[identifierRange]);
            }

            if (newSyntaxNode is null)
            {
                newSyntaxNode = nameSyntax;
            }
            else
            {
                newSyntaxNode = SyntaxFactory.QualifiedName(newSyntaxNode, nameSyntax);
            }
        }

        Debug.Assert(newSyntaxNode is not null);
        return newSyntaxNode;

        bool IsLastIdentifier(System.Range identifierRange)
        {
            // If we are at the last identifier, adding its length to the number of consumed characters
            // should be equal to the total length of the fully-qualified type name before splitting.
            return typeFullName.Length == identifierRange.End.GetOffset(typeFullName.Length);
        }
    }

    /// <summary>
    /// Create a <see cref="MemberAccessExpressionSyntax"/> to access the member
    /// specified by the provided fully-qualified name.
    /// </summary>
    /// <remarks>
    /// This method handles splitting the fully-qualified name in multiple identifiers
    /// and merges them in a <see cref="MemberAccessExpressionSyntax"/>, so we can
    /// easily create syntax from strings that reference members.
    /// </remarks>
    /// <param name="memberFullName">The fully-qualified name of the member.</param>
    /// <returns>The <see cref="MemberAccessExpressionSyntax"/> node.</returns>
    public static ExpressionSyntax CreateMemberAccessExpression(string memberFullName)
    {
        ExpressionSyntax? newSyntaxNode = null;

        var splitByAccessOperator = memberFullName.AsSpan().Split('.');
        foreach (var identifierRange in splitByAccessOperator)
        {
            var identifierSyntax = SyntaxFactory.IdentifierName(memberFullName[identifierRange]);
            if (newSyntaxNode is null)
            {
                newSyntaxNode = identifierSyntax;
            }
            else
            {
                newSyntaxNode = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    newSyntaxNode,
                    identifierSyntax);
            }
        }

        Debug.Assert(newSyntaxNode is not null);
        return newSyntaxNode!;
    }
#endif

#if NET9_0_OR_GREATER
    /// <summary>
    /// Create an <see cref="AttributeSyntax"/> from the attribute type
    /// specified by the provided fully-qualified name.
    /// </summary>
    /// <remarks>
    /// This method handles parsing the fully-qualified name to propertly reference
    /// the attribute type, and adding the attribute argument list from the provided
    /// arguments, so we can easily create attribute syntax.
    /// </remarks>
    /// <param name="attributeTypeFullName">The fully-qualified name of the attribute type.</param>
    /// <param name="arguments">The argument syntax nodes to add to the attribute.</param>
    /// <returns>The <see cref="AttributeSyntax"/> node.</returns>
    public static AttributeSyntax CreateAttribute(string attributeTypeFullName, List<AttributeArgumentSyntax>? arguments = null)
    {
        var attributeSyntax = SyntaxFactory.Attribute(CreateQualifiedName(attributeTypeFullName));

        if (arguments is not null && arguments.Count > 0)
        {
            var argumentListSyntax = SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SeparatedList(arguments));

            attributeSyntax = attributeSyntax.WithArgumentList(argumentListSyntax);
        }

        return attributeSyntax;
    }
#endif

    /// <summary>
    /// Create an <see cref="AttributeArgumentSyntax"/> from the provided expression.
    /// Optionally, it can be a named argument if <paramref name="argumentName"/> is not null.
    /// </summary>
    /// <remarks>
    /// This method handles creating the attribute argument syntax from the expression,
    /// and optionally adding a name equals for the provided argument name, so we can easily
    /// create attribute argument syntax.
    /// </remarks>
    /// <param name="argumentName">Optional name to create a named argument.</param>
    /// <param name="expressionSyntax">The expression that will be used as the argument's value.</param>
    /// <returns>The <see cref="AttributeArgumentSyntax"/> node.</returns>
    public static AttributeArgumentSyntax CreateAttributeArgument(string? argumentName, ExpressionSyntax expressionSyntax)
    {
        var argumentSyntax = SyntaxFactory.AttributeArgument(expressionSyntax);

        if (!string.IsNullOrEmpty(argumentName))
        {
            var nameEquals = SyntaxFactory.NameEquals(
                SyntaxFactory.IdentifierName(argumentName!));

            argumentSyntax = argumentSyntax.WithNameEquals(nameEquals);
        }

        return argumentSyntax;
    }

    /// <summary>
    /// Create an <see cref="AttributeArgumentSyntax"/> from the literal boolean expression.
    /// Optionally, it can be a named argument if <paramref name="argumentName"/> is not null.
    /// </summary>
    /// <param name="argumentName">Optional name to create a named argument.</param>
    /// <param name="value">The value of the literal boolean expression.</param>
    /// <returns>The <see cref="AttributeArgumentSyntax"/> node.</returns>
    public static AttributeArgumentSyntax CreateBooleanAttributeArgument(string? argumentName, bool value)
    {
        var valueExpression = value
            ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
            : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);

        return CreateAttributeArgument(argumentName, valueExpression);
    }

    /// <summary>
    /// Create an <see cref="AttributeArgumentSyntax"/> from the literal string expression.
    /// Optionally, it can be a named argument if <paramref name="argumentName"/> is not null.
    /// </summary>
    /// <param name="argumentName">Optional name to create a named argument.</param>
    /// <param name="value">The value of the literal string expression.</param>
    /// <returns>The <see cref="AttributeArgumentSyntax"/> node.</returns>
    public static AttributeArgumentSyntax CreateStringAttributeArgument(string? argumentName, string value)
    {
        var valueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
        return CreateAttributeArgument(argumentName, valueExpression);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Create an <see cref="AttributeArgumentSyntax"/> from a member access expression.
    /// Optionally, it can be a named argument if <paramref name="argumentName"/> is not null.
    /// </summary>
    /// <remarks>
    /// The <paramref name="memberFullName"/> will be parsed to properly create the
    /// <see cref="MemberAccessExpressionSyntax"/> that will be used as the value of the attribute.
    /// </remarks>
    /// <param name="argumentName">Optional name to create a named argument.</param>
    /// <param name="memberFullName">The fully-qualified name of the member.</param>
    /// <returns>The <see cref="AttributeArgumentSyntax"/> node.</returns>
    public static AttributeArgumentSyntax CreateMemberAccessAttributeArgument(string? argumentName, string memberFullName)
    {
        var valueExpression = CreateMemberAccessExpression(memberFullName);
        return CreateAttributeArgument(argumentName, valueExpression);
    }
#endif

    /// <summary>
    /// Adds an using directive to the root syntax node of a document, if the document doesn't
    /// already have an using directive for the same namespace.
    /// </summary>
    /// <remarks>
    /// When renaming APIs, sometimes a new namespace needs to be included to the document so
    /// the new API is in scope. Call this method to ensure the required namespace is included
    /// in the document.
    /// </remarks>
    /// <param name="root">Root syntax node of the document to modify.</param>
    /// <param name="namespaceName">Name of the namespace to add the using directive for.</param>
    /// <returns>The changed root syntax node with the using directive added.</returns>
    public static SyntaxNode AddUsingDirective(SyntaxNode root, string namespaceName)
    {
        if (root is not CompilationUnitSyntax compilationUnitSyntax)
        {
            // Syntax must be a CompilationUnitSyntax to add using directives, return unchanged root.
            return root;
        }

        if (compilationUnitSyntax.Usings.Any(s => s.Name?.ToString() == namespaceName))
        {
            // Using directive already exists in the document, return unchanged root.
            return root;
        }

        return compilationUnitSyntax.AddUsings([
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(namespaceName)),
        ]);
    }

    /// <summary>
    /// Add an attribute syntax node in its own attribute list to the target syntax node.
    /// </summary>
    /// <param name="syntaxNode">The target syntax node to add the attribute to.</param>
    /// <param name="attributeSyntax">The attribute syntax that will be added.</param>
    /// <returns>The changed syntax node with the attribute added.</returns>
    public static MemberDeclarationSyntax AddAttributeList(MemberDeclarationSyntax syntaxNode, AttributeSyntax attributeSyntax)
    {
        var attributeListSyntax = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attributeSyntax));

        if (syntaxNode.AttributeLists.Count == 0)
        {
            // If the target syntax node has no attribute lists, then move the leading trivia
            // to the new attribute list to preserve it.
            attributeListSyntax = attributeListSyntax.WithLeadingTrivia(syntaxNode.GetLeadingTrivia());
            syntaxNode = syntaxNode.WithoutLeadingTrivia();
        }

        return syntaxNode.AddAttributeLists(attributeListSyntax);
    }

    /// <summary>
    /// Removes an attribute syntax node from a target syntax node,
    /// including the attribute list syntax if it was the last attribute
    /// in the list.
    /// </summary>
    /// <param name="syntaxNode">The target syntax node to remove the attribute from.</param>
    /// <param name="attributeSyntax">The attribute syntax that will be removed.</param>
    /// <returns>The changed syntax node with the attribute removed.</returns>
    public static MemberDeclarationSyntax RemoveAttribute(MemberDeclarationSyntax syntaxNode, AttributeSyntax attributeSyntax)
    {
        if (attributeSyntax.Parent is AttributeListSyntax { Attributes.Count: 1 })
        {
            // If this was the last attribute in the list, remove the entire list syntax node
            // but preserve the non-whitespace trivia.
            var leadingTrivia = attributeSyntax.Parent.GetLeadingTrivia()
                .Where(t => !t.IsKind(SyntaxKind.WhitespaceTrivia));
            SyntaxNode newAttributeSyntaxParent = attributeSyntax.Parent.WithLeadingTrivia(leadingTrivia);
            syntaxNode = syntaxNode.ReplaceNode(attributeSyntax.Parent, newAttributeSyntaxParent);

            // Since we just replaced syntaxNode, the attribute list syntax node won't match anymore
            // so we have to find the equivalent syntax node.
            newAttributeSyntaxParent = syntaxNode
                .DescendantNodes()
                .FirstOrDefault(s => s.IsEquivalentTo(newAttributeSyntaxParent))!;

            Debug.Assert(newAttributeSyntaxParent is not null);

            return syntaxNode.RemoveNode(newAttributeSyntaxParent!, SyntaxRemoveOptions.KeepLeadingTrivia)!;
        }

        // Otherwise, just remove the attribute syntax node.
        return syntaxNode.RemoveNode(attributeSyntax, SyntaxRemoveOptions.KeepNoTrivia)!;
    }

    /// <summary>
    /// Removes multiple attribute syntax nodes from a target syntax node,
    /// including the attribute list syntax if it becomes empty.
    /// </summary>
    /// <param name="syntaxNode">The target syntax node to remove the attributes from.</param>
    /// <param name="attributeSyntaxes">The attribute syntax nodes that will be removed.</param>
    /// <returns>The changed syntax node with the attribute removed.</returns>
    public static MemberDeclarationSyntax RemoveAttributes(MemberDeclarationSyntax syntaxNode, IList<AttributeSyntax> attributeSyntaxes)
    {
        switch (attributeSyntaxes.Count)
        {
            // If there's no attributes to remove, we can return early.
            case 0:
            {
                return syntaxNode;
            }

            // If we only want to remove one attribute, we can take the fast path.
            case 1:
            {
                return RemoveAttribute(syntaxNode, attributeSyntaxes[0]);
            }
        }

        foreach (var attributeSyntax in attributeSyntaxes)
        {
            // Since we are removing attributes in a loop, once syntaxNode is modified
            // the attribute syntax nodes we have won't match anymore. We need to look
            // in the modified syntaxNode to find the equivalent syntax nodes.
            var attributeSyntaxToRemove = syntaxNode
                .DescendantNodes()
                .FirstOrDefault(s => s.IsEquivalentTo(attributeSyntax));
            if (attributeSyntaxToRemove is AttributeSyntax attributeToRemove)
            {
                syntaxNode = RemoveAttribute(syntaxNode, attributeToRemove);
            }
        }

        return syntaxNode;
    }
}

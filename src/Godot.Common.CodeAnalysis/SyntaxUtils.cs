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
}

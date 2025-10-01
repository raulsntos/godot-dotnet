using System.Collections.Generic;
using System.Diagnostics;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.UpgradeAssistant.Providers;

partial class ApiMapCodeFix
{
    private static SyntaxNode? ApplyFixToMember(SyntaxNode root, SyntaxNode syntaxNode, ApiMapEntry mapping, HashSet<string> newNamespaces)
    {
        if (mapping.State is ApiMapState.Removed or ApiMapState.NotImplemented || mapping.NeedsManualUpgrade)
        {
            string? comment = mapping.GetComment();
            if (string.IsNullOrEmpty(comment))
            {
                return null;
            }

            return SyntaxUtils.AddComment(root, syntaxNode, comment);
        }

        Debug.Assert(mapping.ValueDescriptor is not null);

        if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
        {
            // For virtual method overrides we just need to change the name of the method.
            var newMethodDeclarationSyntax = methodDeclarationSyntax.WithIdentifier(SyntaxFactory.Identifier(mapping.ValueDescriptor.Identifier));
            return root.ReplaceNode(syntaxNode, newMethodDeclarationSyntax.WithTriviaFrom(syntaxNode));
        }

        string? typeFullName = GetMappedTypeFullName(syntaxNode, mapping, newNamespaces);
        if (string.IsNullOrEmpty(typeFullName) && syntaxNode is not IdentifierNameSyntax)
        {
            return null;
        }

        string identifier = mapping.ValueDescriptor.Identifier;
        if (mapping.Kind == ApiMapKind.Method && SyntaxUtils.IsGenericNameSyntax(syntaxNode, out var typeArguments))
        {
            SyntaxNode qualifiedName = SyntaxUtils.CreateQualifiedName(identifier, typeArguments);
            if (qualifiedName is not null)
            {
                identifier = qualifiedName.ToString();
            }
        }

        string fullName = SyntaxUtils.ConcatIdentifiers(typeFullName, identifier);

        SyntaxNode newSyntaxNode = mapping.Kind switch
        {
            ApiMapKind.Method => CreateInvocationExpression(fullName, syntaxNode.Parent),
            _ => SyntaxUtils.CreateMemberAccessExpression(fullName),
        };
        if (newSyntaxNode is null)
        {
            return null;
        }

        // For methods we want to replace the invocation expression, not the member access expression.
        if (mapping.Kind == ApiMapKind.Method)
        {
            Debug.Assert(syntaxNode.Parent is InvocationExpressionSyntax);
            syntaxNode = syntaxNode.Parent;
        }

        return root.ReplaceNode(syntaxNode, newSyntaxNode.WithTriviaFrom(syntaxNode));

        static SyntaxNode CreateInvocationExpression(string fullName, SyntaxNode? parentSyntaxNode)
        {
            IEnumerable<ArgumentSyntax>? arguments = null;

            if (parentSyntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                // Get the arguments from the parent invocation expression to preserve them.
                arguments = invocationExpressionSyntax.ArgumentList.Arguments;
            }

            return SyntaxUtils.CreateInvocationExpression(fullName, arguments);
        }

        // Get the fully-qualified name of the type that contains the member that replaces the member referenced
        // by the syntax node, trying to keep the same level of qualified name syntax as the original syntax node.
        static string? GetMappedTypeFullName(SyntaxNode syntaxNode, ApiMapEntry mapping, HashSet<string> newNamespaces)
        {
            Debug.Assert(mapping.ValueDescriptor is not null);

            string? typeFullName;

            if (mapping.Kind == ApiMapKind.Method && mapping.IsExtension)
            {
                // If the syntax node is a member access expression, the expression part
                // represents the name of the type or variable that contains the member.
                typeFullName = GetExpressionTextOrNull(syntaxNode);
                AddNamespaceIfNotNullOrEmpty(mapping.ValueDescriptor.Namespace);
            }
            else if (mapping.IsStatic)
            {
                // If the API is static, we'll include the fully-qualified type name to play it safe,
                // since we can't guarantee that the new member will be accessible from the current scope.
                if (SyntaxUtils.IsSimpleMemberAccessExpression(syntaxNode))
                {
                    typeFullName = mapping.ValueDescriptor.Type;
                    AddNamespaceIfNotNullOrEmpty(mapping.ValueDescriptor.Namespace);
                }
                else
                {
                    typeFullName = SyntaxUtils.ConcatIdentifiers(mapping.ValueDescriptor.Namespace, mapping.ValueDescriptor.Type);

                    // Preserve the global namespace.
                    if (SyntaxUtils.IsGlobalQualifiedSyntax(syntaxNode))
                    {
                        typeFullName = $"global::{typeFullName}";
                    }
                }
            }
            else
            {
                // If the syntax node is a member access expression, the expression part
                // represents the name of the type that contains the member.
                typeFullName = GetExpressionTextOrNull(syntaxNode);
            }

            return typeFullName;

            void AddNamespaceIfNotNullOrEmpty(string? namespaceName)
            {
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    newNamespaces.Add(namespaceName);
                }
            }

            static string? GetExpressionTextOrNull(SyntaxNode syntaxNode)
            {
                return syntaxNode is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                    ? memberAccessExpressionSyntax.Expression.ToString()
                    : null;
            }
        }
    }
}

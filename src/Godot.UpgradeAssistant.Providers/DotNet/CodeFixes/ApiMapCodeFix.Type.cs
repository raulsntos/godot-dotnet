using System.Collections.Generic;
using System.Diagnostics;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.UpgradeAssistant.Providers;

partial class ApiMapCodeFix
{
    private static SyntaxNode? ApplyFixToType(SyntaxNode root, SyntaxNode syntaxNode, ApiMapEntry mapping, HashSet<string> newNamespaces)
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

        string typeName = GetMappedTypeName(syntaxNode, mapping, newNamespaces, out var fullNameSyntax);

        SyntaxNode? newSyntaxNode = CreateNewSyntaxNode(syntaxNode, typeName);
        if (newSyntaxNode is null)
        {
            return null;
        }

        return root.ReplaceNode(fullNameSyntax, newSyntaxNode.WithTriviaFrom(fullNameSyntax));

        static SyntaxNode? CreateNewSyntaxNode(SyntaxNode? syntaxNode, string typeFullName)
        {
            while (syntaxNode is not null)
            {
                if (SyntaxUtils.IsGenericNameSyntax(syntaxNode, out var typeArguments))
                {
                    return SyntaxUtils.CreateQualifiedName(typeFullName, typeArguments);
                }
                if (syntaxNode is NameSyntax)
                {
                    return SyntaxUtils.CreateQualifiedName(typeFullName);
                }
                if (syntaxNode is MemberAccessExpressionSyntax)
                {
                    return SyntaxUtils.CreateMemberAccessExpression(typeFullName);
                }

                syntaxNode = syntaxNode.Parent;
            }

            return null;
        }

        // Get the name of the type that replaces the type referenced by the syntax node,
        // trying to keep the same level of qualified name syntax as the original syntax node.
        static string GetMappedTypeName(SyntaxNode syntaxNode, ApiMapEntry mapping, HashSet<string> newNamespaces, out SyntaxNode fullNameSyntax)
        {
            Debug.Assert(mapping.ValueDescriptor is not null);

            fullNameSyntax = SyntaxUtils.GetFullNameSyntax(syntaxNode);

            // If we got the same syntax node, we should reference the type using the simple name (not fully-qualified).
            bool useSimpleName = fullNameSyntax == syntaxNode;

            string name = useSimpleName
                ? mapping.ValueDescriptor.Identifier
                : mapping.ValueDescriptor.FullName;
            Debug.Assert(!string.IsNullOrEmpty(name));

            if (useSimpleName)
            {
                // We're using the simple name, so we need to ensure the namespace is imported.
                AddNamespaceIfNotNullOrEmpty(SyntaxUtils.ConcatIdentifiers(mapping.ValueDescriptor.Namespace, mapping.ValueDescriptor.Type));
            }
            else
            {
                // Preserve the global namespace.
                if (SyntaxUtils.IsGlobalQualifiedSyntax(fullNameSyntax))
                {
                    name = $"global::{name}";
                }
            }

            return name;

            void AddNamespaceIfNotNullOrEmpty(string? namespaceName)
            {
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    newNamespaces.Add(namespaceName);
                }
            }
        }
    }
}

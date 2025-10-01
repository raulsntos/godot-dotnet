using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.UpgradeAssistant.Providers;

partial class ApiMapCodeFix
{
    private static SyntaxNode? ApplyFixToNamespace(SyntaxNode root, SyntaxNode syntaxNode, ApiMapEntry mapping, HashSet<string> newNamespaces)
    {
        syntaxNode = SyntaxUtils.GetFullNameSyntax(syntaxNode);

        UsingDirectiveSyntax? usingDirectiveSyntax = syntaxNode
            .Ancestors()
            .OfType<UsingDirectiveSyntax>()
            .FirstOrDefault();

        if (syntaxNode.Parent is QualifiedNameSyntax)
        {
            // The syntax node is only a part inside a longer qualified name,
            // skip it since it will likely be handled by a different fix that
            // applies to the parent node.
            return null;
        }

        // The syntax node contains a fully-qualified name,
        // get only the namespace part of that name.
        syntaxNode = GetNamespaceSyntax(syntaxNode);

        if (mapping.NeedsManualUpgrade)
        {
            string? comment = mapping.GetComment();
            if (string.IsNullOrEmpty(comment))
            {
                return null;
            }

            return SyntaxUtils.AddComment(root, syntaxNode, comment);
        }

        if (mapping.State is ApiMapState.Removed or ApiMapState.NotImplemented)
        {
            return TryRemoveNodeAsync(root, syntaxNode);
        }

        Debug.Assert(mapping.Value is not null);

        // One namespace can be replaced by multiple, in which case the replace value
        // will actually contain the fully-qualified namespaces separated by ';'.
        string[] mappingValues = mapping.Value.Split([';'], StringSplitOptions.RemoveEmptyEntries);
        Debug.Assert(mappingValues.Length > 0);

        if (usingDirectiveSyntax is not null)
        {
            foreach (string namespaceName in mappingValues)
            {
                newNamespaces.Add(namespaceName);
            }

            // Since we added the namespaces to 'newNamespaces', an using directive
            // will be added to the document for each of them later and the current
            // using directive syntax node can just be removed.
            return TryRemoveNodeAsync(root, syntaxNode);
        }
        else
        {
            return TryTransformNamespaceAsync(root, syntaxNode, mappingValues);
        }

        static SyntaxNode GetNamespaceSyntax(SyntaxNode syntaxNode)
        {
            if (syntaxNode.Parent is not UsingDirectiveSyntax
             && syntaxNode is QualifiedNameSyntax qualifiedNameSyntax)
            {
                return qualifiedNameSyntax.Left;
            }

            return syntaxNode;
        }

        static SyntaxNode? TryRemoveNodeAsync(SyntaxNode root, SyntaxNode syntaxNode)
        {
            var usingDirectiveSyntax = syntaxNode
                .Ancestors()
                .OfType<UsingDirectiveSyntax>()
                .FirstOrDefault();
            if (usingDirectiveSyntax is null)
            {
                return null;
            }

            return root.RemoveNode(usingDirectiveSyntax, SyntaxRemoveOptions.KeepNoTrivia);
        }
    }

    private static SyntaxNode? TryTransformNamespaceAsync(SyntaxNode root, SyntaxNode syntaxNode, string[] mappingValues)
    {
        // Only mapping namespaces one-to-one is supported when replacing references,
        // if the namespace is mapped to multiple new namespaces, the API map will
        // have to include the type mappings for every type in the namespace.
        string? namespaceName = mappingValues.Length == 1
            ? mappingValues[0]
            : null;

        if (namespaceName is null)
        {
            return null;
        }

        // Preserve the global namespace.
        if (SyntaxUtils.IsGlobalQualifiedSyntax(syntaxNode))
        {
            namespaceName = $"global::{namespaceName}";
        }

        return TryReplaceNodeAsync(root, syntaxNode, namespaceName);

        static SyntaxNode? TryReplaceNodeAsync(SyntaxNode root, SyntaxNode syntaxNode, string newValue)
        {
            SyntaxNode newSyntaxNode = SyntaxUtils.CreateQualifiedName(newValue);
            if (newSyntaxNode is null)
            {
                return null;
            }

            return root.ReplaceNode(syntaxNode, newSyntaxNode.WithTriviaFrom(syntaxNode));
        }
    }
}

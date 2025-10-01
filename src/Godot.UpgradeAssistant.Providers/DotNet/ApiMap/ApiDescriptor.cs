using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Godot.UpgradeAssistant.Providers;

/// <summary>
/// Describes an API created from a fully-qualified name. It can be used to extract the
/// namespace, type, and member name from <see cref="ApiMapEntry"/>.
/// </summary>
/// <example>
/// For a member with the fully-qualified name 'NamespaceA.NamespaceB.Type.Member',
/// the descriptor properties will contain the following values:
/// - FullName: 'NamespaceA.NamespaceB.Type.Member'
/// - Namespace: 'NamespaceA.NamespaceB'
/// - Type: 'Type'
/// - Identifier: 'Member'
/// </example>
/// <example>
/// For a member with the fully-qualified name 'Type.Member',
/// the descriptor properties will contain the following values:
/// - FullName: 'Type.Member'
/// - Namespace: ''
/// - Type: 'Type'
/// - Identifier: 'Member'
/// </example>
internal sealed class ApiDescriptor
{
    /// <summary>
    /// Fully-qualified name that identifies the API.
    /// The original name that was used to create this <see cref="ApiDescriptor"/>.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Fully-qualified name of the namespace or type that contains <see cref="Type"/>.
    /// </summary>
    public required string? Namespace { get; init; }

    /// <summary>
    /// Name of the type or namespace that contains the <see cref="Identifier"/>.
    /// </summary>
    public required string? Type { get; init; }

    /// <summary>
    /// Name of the member, type, or namespace that this API refers to.
    /// </summary>
    public required string Identifier { get; init; }

    private ApiDescriptor() { }

    [return: NotNullIfNotNull(nameof(fullName))]
    public static ApiDescriptor? CreateFromFullName(string? fullName, ApiMapKind kind)
    {
        // To create a descriptor, we have to split the fully-qualified name by the access operators ('.').
        // The last part (after the last access operator) is the identifier, and everything that comes before
        // are the namespace(s) and type(s).

        if (string.IsNullOrWhiteSpace(fullName))
        {
            // If it's empty it must be the mapping value for a removal.
            return null;
        }

        SplitByLastIdentifier(fullName, out var beforeIdentifier, out var identifier);
        if (beforeIdentifier.IsEmpty)
        {
            // If the fully-qualified name doesn't contain any access operators,
            // the whole thing must be the identifier. The only kinds of APIs
            // that can be in the top-level are types and namespaces, so it must
            // be one of those.

            Debug.Assert(kind is ApiMapKind.Type or ApiMapKind.Namespace, $"Expected type or namespace but found '{kind}' for API '{fullName}'.");

            return new ApiDescriptor()
            {
                FullName = fullName,
                Namespace = null,
                Type = null,
                Identifier = fullName,
            };
        }

        SplitByLastIdentifier(beforeIdentifier, out var namespaceName, out var typeName);

        return new ApiDescriptor()
        {
            FullName = fullName,
            Namespace = namespaceName.ToString(),
            Type = typeName.ToString(),
            Identifier = identifier.ToString(),
        };
    }

    /// <summary>
    /// Splits <paramref name="fullName"/> by the last accessor operator (<c>.</c>)
    /// and returns a span of the contents before (<paramref name="prefix"/>) and
    /// after (<paramref name="identifier"/>) that position.
    /// </summary>
    /// <param name="fullName">The fully-qualified name to split.</param>
    /// <param name="prefix">The contents before the last accessor operator or empty.</param>
    /// <param name="identifier">The contents after the last accessor operator.</param>
    private static void SplitByLastIdentifier(ReadOnlySpan<char> fullName, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> identifier)
    {
        if (fullName.IsEmpty)
        {
            // The fully-qualified name was empty, so there's nothing to split.
            prefix = default;
            identifier = default;
            return;
        }

        int lastAccessOperator = fullName.LastIndexOf('.');
        if (lastAccessOperator != -1)
        {
            // Split the prefix and identifier by the last accessor operator.
            prefix = fullName[..lastAccessOperator];
            identifier = fullName[(lastAccessOperator + 1)..];
            return;
        }

        // There was no accessor operator, so take the whole thing as the identifier.
        prefix = default;
        identifier = fullName;
    }
}

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class SymbolExtensions
{
    private static readonly SymbolDisplayFormat _fullyQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    private static readonly SymbolDisplayFormat _fullyQualifiedWithGlobalFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);

    private static readonly SymbolDisplayFormat _fullyQualifiedWithoutGenericTypeArguments =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .WithGenericsOptions(SymbolDisplayGenericsOptions.None);

    /// <summary>
    /// Get the fully-qualified name for the given namespace.
    /// </summary>
    /// <param name="namespaceSymbol">Namespace to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the namespace.</returns>
    public static string FullName(this INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.ToDisplayString(_fullyQualifiedFormat);
    }

    /// <summary>
    /// Get the fully-qualified name for the given type.
    /// </summary>
    /// <param name="typeSymbol">Type to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the type.</returns>
    public static string FullName(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(_fullyQualifiedFormat);
    }

    /// <summary>
    /// Get the fully-qualified name, including the global namespace, for the given type.
    /// </summary>
    /// <param name="typeSymbol">Type to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the type.</returns>
    public static string FullNameWithGlobal(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(_fullyQualifiedWithGlobalFormat);
    }

    /// <summary>
    /// Get the fully-qualified name for the given type (excluding generic type arguments).
    /// </summary>
    /// <param name="typeSymbol">Type to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the type.</returns>
    public static string FullNameWithoutGenericTypeArguments(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(_fullyQualifiedWithoutGenericTypeArguments);
    }

    /// <summary>
    /// Check if <paramref name="typeSymbol"/> derives from a type with a fully-qualified
    /// name that matches <paramref name="baseTypeFullyQualifiedName"/>.
    /// </summary>
    /// <param name="typeSymbol">The type to check the base type for.</param>
    /// <param name="baseTypeFullyQualifiedName">
    /// The fully-qualified name of the base type to check for.
    /// </param>
    /// <returns>Whether the type derives from a type with the specified name.</returns>
    public static bool DerivesFrom(this ITypeSymbol typeSymbol, string baseTypeFullyQualifiedName)
    {
        ITypeSymbol? t = typeSymbol;

        while (t is not null)
        {
            if (t.FullName() == baseTypeFullyQualifiedName)
            {
                return true;
            }

            t = t.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Try to get the <see cref="AttributeData"/> for an attribute in <paramref name="symbol"/> that
    /// matches the specified fully-qualified name.
    /// condition.
    /// </summary>
    /// <param name="symbol">Symbol to get the attribute from.</param>
    /// <param name="fullyQualifiedAttributeTypeName">
    /// Fully-qualified type name for the attribute to look for.
    /// </param>
    /// <param name="attributeData">Returned attribute data, if an attribute was found.</param>
    /// <returns>Whether an attribute that matches the given name was found.</returns>
    public static bool TryGetAttribute(this ISymbol symbol, string fullyQualifiedAttributeTypeName, [NotNullWhen(true)] out AttributeData? attributeData)
    {
        Debug.Assert(fullyQualifiedAttributeTypeName.EndsWith("Attribute", StringComparison.Ordinal), $"Attribute type name '{fullyQualifiedAttributeTypeName}' doesn't end with 'Attribute' suffix.");

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass is null)
            {
                continue;
            }

            if (attribute.AttributeClass.FullName() == fullyQualifiedAttributeTypeName)
            {
                attributeData = attribute;
                return true;
            }
        }

        attributeData = null;
        return false;
    }

    /// <summary>
    /// Get the native Godot type that <paramref name="typeSymbol"/> derives from,
    /// or <see langword="null"/> if the type does not derive from a native Godot
    /// type.
    /// </summary>
    /// <param name="typeSymbol">Type to get the native Godot type for.</param>
    /// <returns>The native Godot base type.</returns>
    public static ITypeSymbol? GetGodotNativeType(this ITypeSymbol typeSymbol)
    {
        ITypeSymbol? t = typeSymbol;

        while (t is not null)
        {
            if (t.ContainingAssembly?.Name == "Godot.Bindings")
            {
                return t;
            }

            t = t.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Get the class name of the native Godot type that <paramref name="typeSymbol"/>
    /// derives from. The class name is the name that the native Godot type is
    /// registered with in ClassDB.
    /// </summary>
    /// <param name="typeSymbol">Type to get the native class name for.</param>
    /// <returns>The class name of the native Godot base type.</returns>
    public static string? GetGodotNativeTypeName(this ITypeSymbol typeSymbol)
    {
        ITypeSymbol? nativeTypeSymbol = typeSymbol.GetGodotNativeType();

        if (nativeTypeSymbol is null)
        {
            return null;
        }

        nativeTypeSymbol.TryGetAttribute(KnownTypeNames.GodotNativeClassNameAttribute, out var attribute);

        string? godotClassName = null;

        if (attribute is not null)
        {
            godotClassName = attribute.ConstructorArguments[0].Value as string;
        }

        return godotClassName ?? nativeTypeSymbol.Name;
    }

    /// <summary>
    /// Get the class name that is used in Godot's ClassDB to refer to
    /// <paramref name="typeSymbol"/>.
    /// This is the name that was used to register the GDExtension class if it's an
    /// extension class, or the native name if it's a built-in class.
    /// </summary>
    /// <param name="typeSymbol">Type to get the class name for.</param>
    /// <returns>The name of the class as registered in ClassDB.</returns>
    public static string GetGodotClassName(this ITypeSymbol typeSymbol)
    {
        string? className = null;

        // If the type is a wrapper of a built-in class, we need to use
        // GetGodotNativeTypeName() because the name of the type may not
        // match the name in ClassDB.
        if (typeSymbol.ContainingAssembly.Name == "Godot.Bindings")
        {
            className = typeSymbol.GetGodotNativeTypeName();
        }

        return className ?? typeSymbol.Name;
    }
}

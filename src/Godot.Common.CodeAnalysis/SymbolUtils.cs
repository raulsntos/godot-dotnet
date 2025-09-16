using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.Common.CodeAnalysis;

/// <summary>
/// Helper methods for working with Roslyn symbols.
/// </summary>
internal static class SymbolUtils
{
    private static readonly SymbolDisplayFormat _fullyQualifiedOmitGlobalFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    private static readonly SymbolDisplayFormat _fullyQualifiedWithGlobalFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);

    private static readonly SymbolDisplayFormat _fullyQualifiedOmitGlobalWithoutGenericTypeArgumentsFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .WithGenericsOptions(SymbolDisplayGenericsOptions.None);

    /// <summary>
    /// Get the fully-qualified name, omitting the global namespace, for the given namespace.
    /// </summary>
    /// <param name="namespaceSymbol">Namespace to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the namespace.</returns>
    public static string FullQualifiedNameOmitGlobal(this INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.ToDisplayString(_fullyQualifiedOmitGlobalFormat);
    }

    /// <summary>
    /// Get the fully-qualified name, omitting the global namespace, for the given type.
    /// </summary>
    /// <param name="typeSymbol">Type to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the type.</returns>
    public static string FullQualifiedNameOmitGlobal(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(_fullyQualifiedOmitGlobalFormat);
    }

    /// <summary>
    /// Get the fully-qualified name, including the global namespace, for the given type.
    /// </summary>
    /// <param name="typeSymbol">Type to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the type.</returns>
    public static string FullQualifiedNameWithGlobal(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(_fullyQualifiedWithGlobalFormat);
    }

    /// <summary>
    /// Get the fully-qualified name, omitting the global namespace, for the given type
    /// (excluding generic type arguments).
    /// </summary>
    /// <param name="typeSymbol">Type to get the fully-qualified name for.</param>
    /// <returns>Fully-qualified name of the type.</returns>
    public static string FullQualifiedNameOmitGlobalWithoutGenericTypeArguments(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(_fullyQualifiedOmitGlobalWithoutGenericTypeArgumentsFormat);
    }

    /// <summary>
    /// Check if <paramref name="typeSymbol"/> is or derives from the type specified
    /// by <paramref name="assemblyName"/> and <paramref name="fullyQualifiedTypeName"/>.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <param name="assemblyName">
    /// The name of the assembly that contains the type to compare against.
    /// If <see langword="null"/>, the assembly name is not checked.
    /// </param>
    /// <param name="fullyQualifiedTypeName">
    /// The fully-qualified name of the type to compare against.
    /// </param>
    /// <returns>Whether the type is or derives from the specified type.</returns>
    public static bool DerivesFrom(this ITypeSymbol? typeSymbol, string fullyQualifiedTypeName, string? assemblyName = null)
    {
        while (typeSymbol is not null)
        {
            if (EqualsType(typeSymbol, fullyQualifiedTypeName, assemblyName))
            {
                return true;
            }

            typeSymbol = typeSymbol.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Check if the type represented by <paramref name="typeSymbol"/> has a fully-qualified name
    /// that matches <paramref name="assemblyName"/> and <paramref name="fullyQualifiedTypeName"/>.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <param name="assemblyName">
    /// The name of the assembly that contains the type to compare against.
    /// If <see langword="null"/>, the assembly name is not checked.
    /// </param>
    /// <param name="fullyQualifiedTypeName">
    /// The fully-qualified name of the type to compare against.
    /// </param>
    /// <returns>Whether the type matches the specified name.</returns>
    public static bool EqualsType(this ITypeSymbol? typeSymbol, string fullyQualifiedTypeName, string? assemblyName = null)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(assemblyName))
        {
            if (typeSymbol.ContainingAssembly.Name != assemblyName)
            {
                return false;
            }
        }

        return typeSymbol.FullQualifiedNameOmitGlobal() == fullyQualifiedTypeName;
    }

    /// <summary>
    /// Gets the <see cref="Location"/> of the type syntax for the specified symbol, if available.
    /// </summary>
    /// <remarks>
    /// This method handles the different ways to access the type syntax for various different declaration
    /// syntax nodes: fields, properties, parameters, and method return types.
    /// </remarks>
    /// <param name="symbol">The symbol to retrieve the type syntax location for.</param>
    /// <returns>
    /// The <see cref="Location"/> of the type syntax if found and in source; otherwise, <see langword="null"/>.
    /// </returns>
    public static Location? GetTypeSyntaxLocation(this ISymbol symbol)
    {
        foreach (var declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
        {
            var declarationSyntax = declaringSyntaxReference.GetSyntax();

            var location = declarationSyntax switch
            {
                VariableDeclaratorSyntax variableDeclaratorSyntax =>
                    // VariableDeclaratorSyntax is used for field declarations.
                    // We need to get the parent FieldDeclarationSyntax to get the type.
                    variableDeclaratorSyntax.Parent?.Parent is FieldDeclarationSyntax fieldDeclaration
                        ? fieldDeclaration.Declaration.Type.GetLocation()
                        : null,

                FieldDeclarationSyntax fieldDeclarationSyntax =>
                    fieldDeclarationSyntax.Declaration.Type.GetLocation(),

                PropertyDeclarationSyntax propertyDeclarationSyntax =>
                    propertyDeclarationSyntax.Type.GetLocation(),

                ParameterSyntax parameterSyntax =>
                    parameterSyntax.Type?.GetLocation(),

                MethodDeclarationSyntax methodSyntax =>
                    methodSyntax.ReturnType.GetLocation(),

                _ =>
                    null,
            };

            if (location is { IsInSource: true })
            {
                return location;
            }
        }

        return null;
    }

    /// <summary>
    /// Check if <paramref name="symbol"/> has an attribute that matches the specified
    /// fully-qualified name.
    /// </summary>
    /// <param name="symbol">Symbol to check for the attribute.</param>
    /// <param name="fullyQualifiedAttributeTypeName">
    /// Fully-qualified name of the attribute to look for.
    /// </param>
    /// <returns>Whether an attribute that matches the given name was found.</returns>
    public static bool HasAttribute(this ISymbol symbol, string fullyQualifiedAttributeTypeName)
    {
        return TryGetAttribute(symbol, fullyQualifiedAttributeTypeName, out _);
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

            if (attribute.AttributeClass.FullQualifiedNameOmitGlobal() == fullyQualifiedAttributeTypeName)
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
    /// <remarks>
    /// This method expects the type to use the Godot .NET bindings, not GodotSharp.
    /// </remarks>
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
    /// <remarks>
    /// This method expects the type to use the Godot .NET bindings, not GodotSharp.
    /// </remarks>
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
    /// <remarks>
    /// This method expects the type to use the Godot .NET bindings, not GodotSharp.
    /// </remarks>
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

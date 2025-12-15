using System;
using System.Diagnostics;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant.Providers;

internal static class SymbolUtils
{
    public static bool DeclaredInGodotSharp(this ISymbol symbol)
    {
        return symbol.ContainingAssembly.Name is "GodotSharp" or "GodotSharpEditor";
    }

    public static bool DerivesFromGodotSharpObject(this ITypeSymbol? typeSymbol)
    {
        // The type was called 'Godot.Object' in 3.x and was renamed to 'Godot.GodotObject' in 4.0.
        return typeSymbol.DerivesFrom("Godot.Object", "GodotSharp")
            || typeSymbol.DerivesFrom("Godot.GodotObject", "GodotSharp");
    }

    public static bool OverridesFromGodotSharp(this IMethodSymbol? methodSymbol)
    {
        return methodSymbol.OverridesFromAssembly("GodotSharp")
            || methodSymbol.OverridesFromAssembly("GodotSharpEditor");
    }

    public static bool IsGodotSharpVariantCompatible(this ITypeSymbol typeSymbol)
    {
        var specialType = typeSymbol.SpecialType;

        switch (specialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Char:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Byte:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_String:
                return true;

            default:
            {
                var typeKind = typeSymbol.TypeKind;

                if (typeKind == TypeKind.Enum)
                {
                    return true;
                }

                if (typeKind == TypeKind.Struct)
                {
                    return typeSymbol.EqualsType("Godot.Vector2", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Vector2I", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Rect2", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Rect2I", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Transform2D", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Vector3", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Vector3I", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Basis", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Quaternion", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Transform3D", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Vector4", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Vector4I", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Projection", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Aabb", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Color", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Plane", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Rid", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Callable", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Signal", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Variant", "GodotSharp");
                }
                else if (typeKind == TypeKind.Array)
                {
                    // GodotSharp supported arrays as Packed Arrays (and some special cases).

                    var arrayTypeSymbol = (IArrayTypeSymbol)typeSymbol;

                    if (arrayTypeSymbol.Rank != 1)
                    {
                        return false;
                    }

                    var elementTypeSymbol = arrayTypeSymbol.ElementType;

                    return elementTypeSymbol.SpecialType switch
                    {
                        SpecialType.System_Byte => true,
                        SpecialType.System_Int32 => true,
                        SpecialType.System_Int64 => true,
                        SpecialType.System_Single => true,
                        SpecialType.System_Double => true,
                        SpecialType.System_String => true,
                        _ => elementTypeSymbol.DerivesFromGodotSharpObject()
                            || elementTypeSymbol.EqualsType("Godot.Vector2", "GodotSharp")
                            || elementTypeSymbol.EqualsType("Godot.Vector3", "GodotSharp")
                            || elementTypeSymbol.EqualsType("Godot.Vector4", "GodotSharp")
                            || elementTypeSymbol.EqualsType("Godot.Color", "GodotSharp")
                            || elementTypeSymbol.EqualsType("Godot.StringName", "GodotSharp")
                            || elementTypeSymbol.EqualsType("Godot.NodePath", "GodotSharp")
                            || elementTypeSymbol.EqualsType("Godot.Rid", "GodotSharp"),
                    };
                }
                else
                {
                    return typeSymbol.DerivesFromGodotSharpObject()
                        || typeSymbol.EqualsType("Godot.StringName", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.NodePath", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Collections.Array", "GodotSharp")
                        || typeSymbol.EqualsType("Godot.Collections.Dictionary", "GodotSharp")
                        || typeSymbol.EqualsGenericType("Godot.Collections.Array<T>", "GodotSharp")
                        || typeSymbol.EqualsGenericType("Godot.Collections.Dictionary<TKey, TValue>", "GodotSharp");
                }
            }
        }
    }

    /// <summary>
    /// Check if <paramref name="symbol"/> has an attribute that matches the specified
    /// fully-qualified name, including the variant without the 'Attribute' suffix
    /// and the simple name without namespaces.
    /// </summary>
    /// <remarks>
    /// This method only exists to support scenarios where the attribute type
    /// is not available as a symbol, such as when analyzing code that references
    /// an assembly that is not available during the analysis.
    /// When the attribute type is a valid symbol, prefer using
    /// <see cref="Common.CodeAnalysis.SymbolUtils.HasAttribute(ISymbol, string)"/>
    /// which ensures the attribute type name matches the fully-qualified name exactly.
    /// </remarks>
    /// <param name="symbol">Symbol to check for the attribute.</param>
    /// <param name="fullyQualifiedAttributeTypeName">
    /// Fully-qualified name of the attribute to look for.
    /// </param>
    /// <returns>Whether an attribute that matches the given name was found.</returns>
    public static bool HasUnknownAttribute(this ISymbol symbol, string fullyQualifiedAttributeTypeName)
    {
        Debug.Assert(fullyQualifiedAttributeTypeName.EndsWith("Attribute", StringComparison.Ordinal), $"Attribute type name '{fullyQualifiedAttributeTypeName}' doesn't end with 'Attribute' suffix.");

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass is null)
            {
                continue;
            }

            if (IsAnyAttributeNameVariant(attribute.AttributeClass.FullQualifiedNameOmitGlobal(), fullyQualifiedAttributeTypeName))
            {
                return true;
            }
        }

        return false;

        static bool IsAnyAttributeNameVariant(string actualAttributeName, string fullyQualifiedAttributeTypeName)
        {
            // Quick path, exact match with the fully qualified name.
            if (actualAttributeName == fullyQualifiedAttributeTypeName)
            {
                return true;
            }

            if (!actualAttributeName.EndsWith("Attribute", StringComparison.Ordinal))
            {
                var fullyQualifiedAttributeTypeNameWithoutSuffix = fullyQualifiedAttributeTypeName.AsSpan(..^"Attribute".Length);

                // Exact match with fully qualified name without the 'Attribute' suffix.
                if (fullyQualifiedAttributeTypeNameWithoutSuffix.SequenceEqual(actualAttributeName))
                {
                    return true;
                }
            }

            int lastIdentifierSeparatorIndex = fullyQualifiedAttributeTypeName.LastIndexOf('.');
            if (lastIdentifierSeparatorIndex != -1)
            {
                var attributeTypeName = fullyQualifiedAttributeTypeName.AsSpan(lastIdentifierSeparatorIndex + 1);

                // Exact match with the attribute type name without the namespace.
                if (attributeTypeName.SequenceEqual(actualAttributeName))
                {
                    return true;
                }

                // Exact match with the attribute type name without the namespace and the 'Attribute' suffix.
                attributeTypeName = attributeTypeName[..^"Attribute".Length];
                if (attributeTypeName.SequenceEqual(actualAttributeName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

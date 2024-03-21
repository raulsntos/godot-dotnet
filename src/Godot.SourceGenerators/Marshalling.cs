using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class Marshalling
{
    public static MarshalInfo GetMarshallingInformation(Compilation compilation, ITypeSymbol typeSymbol)
    {
        if (!TryGetVariantType(typeSymbol, out VariantType variantType, out VariantTypeMetadata variantTypeMetadata))
        {
            throw new ArgumentException($"Can't marshal type '{typeSymbol}'.", nameof(typeSymbol));
        }

        TryGetDefaultPropertyHint(compilation, typeSymbol, variantType, out PropertyHint hint, out string? hintString);

        PropertyUsageFlags usage = GetPropertyUsageFlags(variantType);

        // Object-derived types should specify their ClassDB name.
        string? className = variantType == VariantType.Object
            ? typeSymbol.GetGodotNativeTypeName()
            : null;

        return new MarshalInfo()
        {
            VariantType = variantType,
            VariantTypeMetadata = variantTypeMetadata,
            FullyQualifiedTypeName = typeSymbol.FullNameWithGlobal(),

            Hint = hint,
            HintString = hintString,
            ClassName = className,
            Usage = usage,
        };
    }

    private static bool TryGetVariantType(ITypeSymbol typeSymbol, out VariantType variantType, out VariantTypeMetadata variantTypeMetadata)
    {
        variantType = VariantType.Nil;
        variantTypeMetadata = VariantTypeMetadata.None;

        (VariantType? maybeVariantType, variantTypeMetadata) = typeSymbol.SpecialType switch
        {
            SpecialType.System_Void => (VariantType.Nil, VariantTypeMetadata.None),

            SpecialType.System_Boolean => (VariantType.Bool, VariantTypeMetadata.None),

            // TODO: Not sure what the metadata should be for char.
            SpecialType.System_Char => (VariantType.Int, VariantTypeMetadata.None),
            SpecialType.System_SByte => (VariantType.Int, VariantTypeMetadata.SByte),
            SpecialType.System_Int16 => (VariantType.Int, VariantTypeMetadata.Int16),
            SpecialType.System_Int32 => (VariantType.Int, VariantTypeMetadata.Int32),
            SpecialType.System_Int64 => (VariantType.Int, VariantTypeMetadata.Int64),
            SpecialType.System_Byte => (VariantType.Int, VariantTypeMetadata.Byte),
            SpecialType.System_UInt16 => (VariantType.Int, VariantTypeMetadata.UInt16),
            SpecialType.System_UInt32 => (VariantType.Int, VariantTypeMetadata.UInt32),
            SpecialType.System_UInt64 => (VariantType.Int, VariantTypeMetadata.UInt64),

            _ when typeSymbol.FullName() == KnownTypeNames.SystemHalf => (VariantType.Float, VariantTypeMetadata.None),
            SpecialType.System_Single => (VariantType.Float, VariantTypeMetadata.Single),
            SpecialType.System_Double => (VariantType.Float, VariantTypeMetadata.Double),

            SpecialType.System_String => (VariantType.String, VariantTypeMetadata.None),

            _ => (default(VariantType?), VariantTypeMetadata.None),
        };
        if (maybeVariantType is not null)
        {
            variantType = maybeVariantType.Value;
            return true;
        }

        var typeKind = typeSymbol.TypeKind;

        if (typeKind is TypeKind.Enum)
        {
            variantType = VariantType.Int;
            return true;
        }

        if (typeKind is TypeKind.Struct or TypeKind.Class)
        {
            if (typeSymbol.ContainingAssembly.Name == "Godot.Bindings")
            {
                string typeName = typeSymbol.FullName();
                maybeVariantType = typeName switch
                {
                    KnownTypeNames.GodotAabb => VariantType.Aabb,
                    KnownTypeNames.GodotBasis => VariantType.Basis,
                    KnownTypeNames.GodotCallable => VariantType.Callable,
                    KnownTypeNames.GodotColor => VariantType.Color,
                    KnownTypeNames.GodotNodePath => VariantType.NodePath,
                    KnownTypeNames.GodotPlane => VariantType.Plane,
                    KnownTypeNames.GodotProjection => VariantType.Projection,
                    KnownTypeNames.GodotQuaternion => VariantType.Quaternion,
                    KnownTypeNames.GodotRect2 => VariantType.Rect2,
                    KnownTypeNames.GodotRect2I => VariantType.Rect2I,
                    KnownTypeNames.GodotRid => VariantType.Rid,
                    KnownTypeNames.GodotSignal => VariantType.Signal,
                    KnownTypeNames.GodotStringName => VariantType.StringName,
                    KnownTypeNames.GodotTransform2D => VariantType.Transform2D,
                    KnownTypeNames.GodotTransform3D => VariantType.Transform3D,
                    KnownTypeNames.GodotVector2 => VariantType.Vector2,
                    KnownTypeNames.GodotVector2I => VariantType.Vector2I,
                    KnownTypeNames.GodotVector3 => VariantType.Vector3,
                    KnownTypeNames.GodotVector3I => VariantType.Vector3I,
                    KnownTypeNames.GodotVector4 => VariantType.Vector4,
                    KnownTypeNames.GodotVector4I => VariantType.Vector4I,
                    KnownTypeNames.GodotVariant => VariantType.Nil,

                    KnownTypeNames.GodotPackedByteArray => VariantType.PackedByteArray,
                    KnownTypeNames.GodotPackedInt32Array => VariantType.PackedInt32Array,
                    KnownTypeNames.GodotPackedInt64Array => VariantType.PackedInt64Array,
                    KnownTypeNames.GodotPackedFloat32Array => VariantType.PackedFloat32Array,
                    KnownTypeNames.GodotPackedFloat64Array => VariantType.PackedFloat64Array,
                    KnownTypeNames.GodotPackedStringArray => VariantType.PackedStringArray,
                    KnownTypeNames.GodotPackedVector2Array => VariantType.PackedVector2Array,
                    KnownTypeNames.GodotPackedVector3Array => VariantType.PackedVector3Array,
                    KnownTypeNames.GodotPackedColorArray => VariantType.PackedColorArray,

                    _ => null,
                };
                if (maybeVariantType is not null)
                {
                    variantType = maybeVariantType.Value;
                    return true;
                }

                typeName = typeSymbol.FullNameWithoutGenericTypeArguments();
                maybeVariantType = typeName switch
                {
                    KnownTypeNames.GodotArray => VariantType.Array,
                    KnownTypeNames.GodotDictionary => VariantType.Dictionary,
                    _ => null,
                };
                if (maybeVariantType is not null)
                {
                    variantType = maybeVariantType.Value;
                    return true;
                }
            }

            if (typeSymbol.DerivesFrom(KnownTypeNames.GodotObject))
            {
                variantType = VariantType.Object;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetArrayLikeElementType(Compilation compilation, ITypeSymbol arrayLikeTypeSymbol, [NotNullWhen(true)] out ITypeSymbol? elementTypeSymbol)
    {
        elementTypeSymbol = null;

        if (arrayLikeTypeSymbol.ContainingAssembly.Name == "Godot.Bindings")
        {
            string arrayLikeTypeName = arrayLikeTypeSymbol.FullName();
            string? elementTypeName = arrayLikeTypeName switch
            {
                KnownTypeNames.GodotPackedByteArray => KnownTypeNames.SystemByte,
                KnownTypeNames.GodotPackedInt32Array => KnownTypeNames.SystemInt32,
                KnownTypeNames.GodotPackedInt64Array => KnownTypeNames.SystemInt64,
                KnownTypeNames.GodotPackedFloat32Array => KnownTypeNames.SystemSingle,
                KnownTypeNames.GodotPackedFloat64Array => KnownTypeNames.SystemDouble,
                KnownTypeNames.GodotPackedStringArray => KnownTypeNames.SystemString,
                KnownTypeNames.GodotPackedVector2Array => KnownTypeNames.GodotVector2,
                KnownTypeNames.GodotPackedVector3Array => KnownTypeNames.GodotVector3,
                KnownTypeNames.GodotPackedColorArray => KnownTypeNames.GodotColor,

                _ => null,
            };
            if (!string.IsNullOrEmpty(elementTypeName))
            {
                elementTypeSymbol = GetKnownTypeFromCompilation(compilation, elementTypeName!);
                return true;

                static ITypeSymbol GetKnownTypeFromCompilation(Compilation compilation, string knownTypeName)
                {
                    var typeSymbol = compilation.GetTypeByMetadataName(knownTypeName);

                    // The type must not be null because we are using the name for types that we know exist.
                    Debug.Assert(typeSymbol is not null, $"Type '{knownTypeName}' does not exist.");

                    return typeSymbol!;
                }
            }

            arrayLikeTypeName = arrayLikeTypeSymbol.FullNameWithoutGenericTypeArguments();
            if (arrayLikeTypeName == KnownTypeNames.GodotArray)
            {
                if (arrayLikeTypeSymbol is not INamedTypeSymbol { IsGenericType: true } genericArrayLikeTypeSymbol)
                {
                    // The array-like type is the non-generic GodotArray so the element type
                    // is Variant, but that's the same as not being able to get the element
                    // type because we can't get a property hint from Variant.
                    return false;
                }

                // The array-like type must be GodotArray<T> so we can get
                // the element type from its first type argument.
                elementTypeSymbol = genericArrayLikeTypeSymbol.TypeArguments[0];
                return true;
            }
        }

        return false;
    }

    private static bool TryGetDictionaryLikeKeyValueTypes(Compilation compilation, ITypeSymbol dictionaryLikeTypeSymbol, [NotNullWhen(true)] out ITypeSymbol? keyTypeSymbol, [NotNullWhen(true)] out ITypeSymbol? valueTypeSymbol)
    {
        keyTypeSymbol = null;
        valueTypeSymbol = null;

        if (dictionaryLikeTypeSymbol.ContainingAssembly.Name == "Godot.Bindings")
        {
            string dictionaryLikeTypeName = dictionaryLikeTypeSymbol.FullNameWithoutGenericTypeArguments();
            if (dictionaryLikeTypeName == KnownTypeNames.GodotDictionary)
            {
                if (dictionaryLikeTypeSymbol is not INamedTypeSymbol { IsGenericType: true } genericArrayLikeTypeSymbol)
                {
                    // The dictionary-like type is the non-generic GodotDictionary so the
                    // element type is Variant, but that's the same as not being able to
                    // get the element type because we can't get a property hint from Variant.
                    return false;
                }

                // The dictionary-like type must be GodotDictionary<TKey, TValue> so
                // we can get the key and value types from its type arguments.
                keyTypeSymbol = genericArrayLikeTypeSymbol.TypeArguments[0];
                valueTypeSymbol = genericArrayLikeTypeSymbol.TypeArguments[1];
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to get the default <see cref="PropertyHint"/> and hint string for the given
    /// <paramref name="typeSymbol"/>.
    /// </summary>
    public static bool TryGetDefaultPropertyHint(Compilation compilation, ITypeSymbol typeSymbol, VariantType variantType, out PropertyHint hint, out string? hintString)
    {
        return TryGetDefaultPropertyHintCore(compilation, typeSymbol, variantType, isNestedType: false, out hint, out hintString);
    }

    private static bool TryGetDefaultPropertyHintCore(Compilation compilation, ITypeSymbol typeSymbol, VariantType variantType, bool isNestedType, out PropertyHint hint, out string? hintString)
    {
        hint = PropertyHint.None;
        hintString = null;

        if (variantType == VariantType.Nil)
        {
            // Can't get a property hint for the Variant type.
            return true;
        }

        if (variantType is VariantType.Int or VariantType.String or VariantType.StringName
         && typeSymbol.TypeKind == TypeKind.Enum)
        {
            GetEnumPropertyHint(typeSymbol, out hint, out hintString);
            return true;
        }

        if (variantType == VariantType.Object)
        {
            if (typeSymbol.DerivesFrom(KnownTypeNames.GodotNode))
            {
                hint = PropertyHint.NodeType;
                hintString = typeSymbol.GetGodotClassName();
                return true;
            }

            if (typeSymbol.DerivesFrom(KnownTypeNames.GodotResource))
            {
                hint = PropertyHint.ResourceType;
                hintString = typeSymbol.GetGodotClassName();
                return true;
            }

            // Node-derived and Resource-derived are the only Object-derived types
            // that we can generate a property hint for.
            return false;
        }

        // Array-like and dictionary-like types don't support nested property hints
        // for more than one level. So if the type is "nested", avoid trying to
        // retrieve a hint from array-like and dictionary-like types.
        if (!isNestedType)
        {
            if (variantType == VariantType.Array || variantType.IsPackedArray())
            {
                return TryGetArrayLikePropertyHint(compilation, typeSymbol, out hint, out hintString);
            }

            if (variantType == VariantType.Dictionary)
            {
                return TryGetDictionaryLikePropertyHint(compilation, typeSymbol, out hint, out hintString);
            }
        }

        return false;
    }

    private static void GetEnumPropertyHint(ITypeSymbol enumTypeSymbol, out PropertyHint hint, out string? hintString)
    {
        Debug.Assert(enumTypeSymbol.TypeKind == TypeKind.Enum, $"Type '{enumTypeSymbol}' must be an enum to get the enum property hint.");

        bool hasFlagsAttr = enumTypeSymbol.TryGetAttribute("System.FlagsAttribute", out _);

        hint = hasFlagsAttr ? PropertyHint.Flags : PropertyHint.Enum;

        var enumFields = enumTypeSymbol.GetMembers()
            .Where(s => s.Kind == SymbolKind.Field && s.IsStatic && s.DeclaredAccessibility == Accessibility.Public && !s.IsImplicitlyDeclared)
            .Cast<IFieldSymbol>().ToArray();

        var hintStringBuilder = new StringBuilder();
        var nameOnlyHintStringBuilder = new StringBuilder();

        // True: enum Foo { Bar, Baz, Qux }
        // True: enum Foo { Bar = 0, Baz = 1, Qux = 2 }
        // False: enum Foo { Bar = 0, Baz = 7, Qux = 5 }
        bool usesDefaultValues = true;

        for (int i = 0; i < enumFields.Length; i++)
        {
            var enumField = enumFields[i];

            if (i > 0)
            {
                hintStringBuilder.Append(',');
                nameOnlyHintStringBuilder.Append(',');
            }

            string enumFieldName = enumField.Name;
            hintStringBuilder.Append(enumFieldName);
            nameOnlyHintStringBuilder.Append(enumFieldName);

            long val = enumField.ConstantValue switch
            {
                sbyte v => v,
                short v => v,
                int v => v,
                long v => v,
                byte v => v,
                ushort v => v,
                uint v => v,
                ulong v => (long)v,
                _ => 0,
            };

            uint expectedVal = (uint)(hint == PropertyHint.Flags ? 1 << i : i);
            if (val != expectedVal)
            {
                usesDefaultValues = false;
            }

            hintStringBuilder.Append(':');
            hintStringBuilder.Append(val);
        }

        hintString = !usesDefaultValues
            ? hintStringBuilder.ToString()
            // If we use the format NAME:VAL, that's what the editor displays.
            // That's annoying if the user is not using custom values for the enum constants.
            // This may not be needed in the future if the editor is changed to not display values.
            : nameOnlyHintStringBuilder.ToString();
    }

    private static bool TryGetArrayLikePropertyHint(Compilation compilation, ITypeSymbol arrayLikeTypeSymbol, out PropertyHint hint, out string? hintString)
    {
        hint = PropertyHint.None;
        hintString = null;

        if (!TryGetArrayLikeElementType(compilation, arrayLikeTypeSymbol, out ITypeSymbol? elementTypeSymbol))
        {
            // Array is not generic or the type is not recognized as an array-like type.
            return false;
        }

        if (!TryGetVariantType(elementTypeSymbol, out VariantType elementVariantType, out _))
        {
            // The element type, and by extension the array-like type, is not marshallable.
            return false;
        }

        if (!TryGetDefaultPropertyHintCore(compilation, elementTypeSymbol, elementVariantType, isNestedType: true, out PropertyHint elementHint, out string? elementHintString))
        {
            // We were unable to get a property hint for the element type,
            // but we can still create a property hint for the array.
            hint = PropertyHint.TypeString;
            hintString = ConstructArrayHintString(elementVariantType, PropertyHint.None, null);
            return true;
        }

        hint = PropertyHint.TypeString;
        hintString = ConstructArrayHintString(elementVariantType, elementHint, elementHintString);
        return true;

        static string ConstructArrayHintString(VariantType elementVariantType, PropertyHint elementHint, string? elementHintString)
        {
            // Format: "type/hint:hint_string"
            // IMPORTANT: The enums are formatted as numeric values.
            return $"{elementVariantType:D}/{elementHint:D}:{elementHintString}";
        }
    }

    private static bool TryGetDictionaryLikePropertyHint(Compilation compilation, ITypeSymbol dictionaryLikeTypeSymbol, out PropertyHint hint, out string? hintString)
    {
        hint = PropertyHint.None;
        hintString = null;

        if (!TryGetDictionaryLikeKeyValueTypes(compilation, dictionaryLikeTypeSymbol, out ITypeSymbol? keyTypeSymbol, out ITypeSymbol? valueTypeSymbol))
        {
            // Dictionary is not generic or the type is not recognized as an dictionary-like type.
            return false;
        }

        if (!TryGetVariantType(keyTypeSymbol, out VariantType keyVariantType, out _)
         || !TryGetVariantType(valueTypeSymbol, out VariantType valueVariantType, out _))
        {
            // The key type or the value type, and by extension the dictionary-like type, is not marshallable.
            return false;
        }

        // TODO: Dictionaries don't have a hint that can be used for inspector support yet.
        // https://github.com/godotengine/godot/pull/78656
        return false;
    }

    private static PropertyUsageFlags GetPropertyUsageFlags(VariantType variantType)
    {
        PropertyUsageFlags usage = PropertyUsageFlags.Default;

        if (variantType == VariantType.Nil)
        {
            // Properties and parameters should always have a type,
            // so we assume 'nil' means Variant here.
            usage |= PropertyUsageFlags.NilIsVariant;
        }

        return usage;
    }
}

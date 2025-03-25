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
        if (!TryGetVariantType(compilation, typeSymbol, out VariantType variantType, out VariantTypeMetadata variantTypeMetadata, out string? fullyQualifiedMarshalAsTypeName))
        {
            throw new ArgumentException($"Can't marshal type '{typeSymbol}'.", nameof(typeSymbol));
        }

        TryGetDefaultPropertyHint(compilation, typeSymbol, variantType, out PropertyHint hint, out string? hintString);

        PropertyUsageFlags usage = GetPropertyUsageFlags(variantType);

        // Object-derived types should specify their ClassDB name.
        string? className = variantType == VariantType.Object
            ? typeSymbol.GetGodotNativeTypeName()
            : null;

        string fullyQualifiedTypeName = typeSymbol.FullNameWithGlobal();

        // Ensure the type name is fully-qualified, including the global namespace.
        if (!string.IsNullOrEmpty(fullyQualifiedMarshalAsTypeName)
         && !fullyQualifiedMarshalAsTypeName!.StartsWith("global::", StringComparison.Ordinal))
        {
            fullyQualifiedMarshalAsTypeName = $"global::{fullyQualifiedMarshalAsTypeName}";
        }

        return new MarshalInfo()
        {
            VariantType = variantType,
            VariantTypeMetadata = variantTypeMetadata,
            FullyQualifiedTypeName = fullyQualifiedTypeName,
            FullyQualifiedMarshalAsTypeName = fullyQualifiedMarshalAsTypeName,
            FullyQualifiedMarshallerTypeName = null,

            Hint = hint,
            HintString = hintString,
            ClassName = className,
            Usage = usage,
        };
    }

    private static bool TryGetVariantType(Compilation compilation, ITypeSymbol typeSymbol, out VariantType variantType, out VariantTypeMetadata variantTypeMetadata, out string? fullyQualifiedMarshalAsTypeName)
    {
        fullyQualifiedMarshalAsTypeName = null;

        if (TryGetVariantTypeForCoreTypes(typeSymbol, out variantType, out variantTypeMetadata))
        {
            return true;
        }

        if (TryGetVariantTypeForSpecialTypes(compilation, typeSymbol, out variantType, out variantTypeMetadata, out fullyQualifiedMarshalAsTypeName))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Try to get the variant type and metadata associated with one of the types in the Variant union.
    /// </summary>
    /// <param name="typeSymbol">The managed type symbol.</param>
    /// <param name="variantType">The associated Variant type kind.</param>
    /// <param name="variantTypeMetadata">The associated Variant type metadata.</param>
    /// <returns>Whether an associated Variant type exists for the specified managed type.</returns>
    private static bool TryGetVariantTypeForCoreTypes(ITypeSymbol typeSymbol, out VariantType variantType, out VariantTypeMetadata variantTypeMetadata)
    {
        variantType = VariantType.Nil;
        variantTypeMetadata = VariantTypeMetadata.None;

        (VariantType? maybeVariantType, variantTypeMetadata) = typeSymbol.SpecialType switch
        {
            SpecialType.System_Void => (VariantType.Nil, VariantTypeMetadata.None),

            SpecialType.System_Boolean => (VariantType.Bool, VariantTypeMetadata.None),

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

            SpecialType.System_Char => (VariantType.Int, VariantTypeMetadata.Char16),
            _ when typeSymbol.FullName() == KnownTypeNames.SystemTextRune => (VariantType.Int, VariantTypeMetadata.Char32),

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
                    KnownTypeNames.GodotPackedVector4Array => VariantType.PackedVector4Array,

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

    /// <summary>
    /// Try to get the variant type and metadata associated with one of the specially-recognized managed types.
    /// </summary>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="typeSymbol">The managed type symbol.</param>
    /// <param name="variantType">The associated Variant type kind.</param>
    /// <param name="variantTypeMetadata">The associated Variant type metadata.</param>
    /// <param name="fullyQualifiedMarshalAsTypeName">
    /// The fully-qualifed name of the type to use for marshalling.
    /// </param>
    /// <returns>Whether an associated Variant type exists for the specified managed type.</returns>
    private static bool TryGetVariantTypeForSpecialTypes(Compilation compilation, ITypeSymbol typeSymbol, out VariantType variantType, out VariantTypeMetadata variantTypeMetadata, [NotNullWhen(true)] out string? fullyQualifiedMarshalAsTypeName)
    {
        variantType = VariantType.Nil;
        variantTypeMetadata = VariantTypeMetadata.None;
        fullyQualifiedMarshalAsTypeName = null;

        var typeKind = typeSymbol.TypeKind;

        if (typeKind is TypeKind.Array)
        {
            var arrayTypeSymbol = (IArrayTypeSymbol)typeSymbol;

            if (arrayTypeSymbol.Rank != 1)
            {
                return false;
            }

            var elementTypeSymbol = arrayTypeSymbol.ElementType;

            if (TryGetPackedArrayType(typeSymbol, elementTypeSymbol, out variantType, out fullyQualifiedMarshalAsTypeName))
            {
                return true;
            }

            // Validate that the element type is compatible.
            if (TryGetVariantTypeForCoreTypes(elementTypeSymbol, out _, out _))
            {
                variantType = VariantType.Array;
                fullyQualifiedMarshalAsTypeName = MakeGenericGodotArray(elementTypeSymbol);
                return true;
            }

            return false;
        }

        if (typeKind is TypeKind.Class)
        {
            string typeName = typeSymbol.FullNameWithoutGenericTypeArguments();

            switch (typeName)
            {
                case KnownTypeNames.SystemCollectionsGenericList:
                {
                    if (!TryGetArrayLikeElementType(compilation, typeSymbol, out var elementTypeSymbol))
                    {
                        return false;
                    }

                    // Prefer packed arrays for array-like types.
                    if (TryGetPackedArrayType(typeSymbol, elementTypeSymbol, out variantType, out fullyQualifiedMarshalAsTypeName))
                    {
                        return true;
                    }

                    // Validate that the element type is compatible.
                    if (TryGetVariantTypeForCoreTypes(elementTypeSymbol, out _, out _))
                    {
                        variantType = VariantType.Array;
                        fullyQualifiedMarshalAsTypeName = MakeGenericGodotArray(elementTypeSymbol);
                        return true;
                    }

                    return false;
                }

                case KnownTypeNames.SystemCollectionsGenericDictionary:
                {
                    if (!TryGetDictionaryLikeKeyValueTypes(compilation, typeSymbol, out var keyTypeSymbol, out var valueTypeSymbol))
                    {
                        return false;
                    }

                    // Validate that the key and value types are compatible.
                    if (TryGetVariantTypeForCoreTypes(keyTypeSymbol, out _, out _)
                     && TryGetVariantTypeForCoreTypes(valueTypeSymbol, out _, out _))
                    {
                        variantType = VariantType.Dictionary;
                        fullyQualifiedMarshalAsTypeName = MakeGenericGodotDictionary(keyTypeSymbol, valueTypeSymbol);
                        return true;
                    }

                    return false;
                }
            }
        }

        return false;

        static bool TryGetPackedArrayType(ITypeSymbol typeSymbol, ITypeSymbol elementTypeSymbol, out VariantType packedArrayType, [NotNullWhen(true)] out string? fullyQualifiedMarshalAsTypeName)
        {
            (packedArrayType, fullyQualifiedMarshalAsTypeName) = elementTypeSymbol.SpecialType switch
            {
                SpecialType.System_Byte => (VariantType.PackedByteArray, KnownTypeNames.GodotPackedByteArray),
                SpecialType.System_Int32 => (VariantType.PackedInt32Array, KnownTypeNames.GodotPackedInt32Array),
                SpecialType.System_Int64 => (VariantType.PackedInt64Array, KnownTypeNames.GodotPackedInt64Array),
                SpecialType.System_Single => (VariantType.PackedFloat64Array, KnownTypeNames.GodotPackedFloat64Array),
                SpecialType.System_Double => (VariantType.PackedFloat64Array, KnownTypeNames.GodotPackedFloat64Array),
                SpecialType.System_String => (VariantType.PackedStringArray, KnownTypeNames.GodotPackedStringArray),

                _ => (VariantType.Nil, null),
            };

            if (fullyQualifiedMarshalAsTypeName != null)
            {
                return true;
            }

            (packedArrayType, fullyQualifiedMarshalAsTypeName) = typeSymbol.FullName() switch
            {
                KnownTypeNames.GodotVector2 => (VariantType.PackedVector2Array, KnownTypeNames.GodotPackedVector2Array),
                KnownTypeNames.GodotVector3 => (VariantType.PackedVector3Array, KnownTypeNames.GodotPackedVector3Array),
                KnownTypeNames.GodotVector4 => (VariantType.PackedVector4Array, KnownTypeNames.GodotPackedVector4Array),

                _ => (VariantType.Nil, null),
            };

            return fullyQualifiedMarshalAsTypeName != null;
        }
    }

    private static bool TryGetArrayLikeElementType(Compilation compilation, ITypeSymbol arrayLikeTypeSymbol, [NotNullWhen(true)] out ITypeSymbol? elementTypeSymbol)
    {
        elementTypeSymbol = null;

        // Godot Core types.

        // NOTE: ContainingAssembly can be null if the type is a C# array (e.g.: int[])
        if (arrayLikeTypeSymbol.ContainingAssembly?.Name == "Godot.Bindings")
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
                KnownTypeNames.GodotPackedVector4Array => KnownTypeNames.GodotVector4,

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
                // If the type is generic, it must be `GodotArray<T>` so we can get the element type
                // from its first type argument. Otherwise, it's the non-generic `GodotArray` so the
                // element type is Variant, but that's the same as not being able to get the element
                // type because we can't get a property hint from Variant.
                return TryGetFirstTypeArgument(arrayLikeTypeSymbol, out elementTypeSymbol);
            }
        }

        // Specially-recognized types.

        var typeKind = arrayLikeTypeSymbol.TypeKind;

        if (typeKind == TypeKind.Array)
        {
            var arrayTypeSymbol = (IArrayTypeSymbol)arrayLikeTypeSymbol;

            if (arrayTypeSymbol.Rank != 1)
            {
                return false;
            }

            elementTypeSymbol = arrayTypeSymbol.ElementType;
            return true;
        }

        if (typeKind == TypeKind.Class)
        {
            string typeName = arrayLikeTypeSymbol.FullNameWithoutGenericTypeArguments();
            if (typeName == KnownTypeNames.SystemCollectionsGenericList)
            {
                // We don't specially-recognize any non-generic array-like types.
                // If the type is generic, it must be `System.Collections.Generic.List<T>`
                // so we can get the element type from its first type argument.
                return TryGetFirstTypeArgument(arrayLikeTypeSymbol, out elementTypeSymbol);
            }
        }

        return false;

        static bool TryGetFirstTypeArgument(ITypeSymbol typeSymbol, [NotNullWhen(true)] out ITypeSymbol? firstTypeArgument)
        {
            if (typeSymbol is not INamedTypeSymbol { IsGenericType: true } genericTypeSymbol)
            {
                // Type is not generic, so there are no type arguments.
                firstTypeArgument = null;
                return false;
            }

            Debug.Assert(genericTypeSymbol.TypeArguments.Length == 1);

            firstTypeArgument = genericTypeSymbol.TypeArguments[0];
            return true;
        }
    }

    private static bool TryGetDictionaryLikeKeyValueTypes(Compilation compilation, ITypeSymbol dictionaryLikeTypeSymbol, [NotNullWhen(true)] out ITypeSymbol? keyTypeSymbol, [NotNullWhen(true)] out ITypeSymbol? valueTypeSymbol)
    {
        keyTypeSymbol = null;
        valueTypeSymbol = null;

        // Godot Core types.

        if (dictionaryLikeTypeSymbol.ContainingAssembly.Name == "Godot.Bindings")
        {
            string dictionaryLikeTypeName = dictionaryLikeTypeSymbol.FullNameWithoutGenericTypeArguments();
            if (dictionaryLikeTypeName == KnownTypeNames.GodotDictionary)
            {
                // If the type is generic, it must be `GodotDictionary<TKey, TValue>` so we can get
                // the element type from its first and second type arguments. Otherwise, it's the
                // non-generic `GodotDictionary` so the key and value types are both Variant, but
                // that's the same as not being able to get the key and value types because we can't
                // get a property hint from Variant.
                return TryGetFirstAndSecondTypeArguments(dictionaryLikeTypeSymbol, out keyTypeSymbol, out valueTypeSymbol);
            }
        }

        // Specially-recognized types.

        var typeKind = dictionaryLikeTypeSymbol.TypeKind;

        if (typeKind == TypeKind.Class)
        {
            string typeName = dictionaryLikeTypeSymbol.FullNameWithoutGenericTypeArguments();
            if (typeName == KnownTypeNames.SystemCollectionsGenericList)
            {
                // We don't specially-recognize any non-generic dictionary-like types.
                // If the type is generic, it must be `System.Collections.Generic.Dictionary<TKey, TValue>`
                // so we can get the element type from its first and second type arguments.
                return TryGetFirstAndSecondTypeArguments(dictionaryLikeTypeSymbol, out keyTypeSymbol, out valueTypeSymbol);
            }
        }

        return false;

        static bool TryGetFirstAndSecondTypeArguments(ITypeSymbol typeSymbol, [NotNullWhen(true)] out ITypeSymbol? firstTypeArgument, out ITypeSymbol? secondTypeArgument)
        {
            if (typeSymbol is not INamedTypeSymbol { IsGenericType: true } genericTypeSymbol)
            {
                // Type is not generic, so there are no type arguments.
                firstTypeArgument = null;
                secondTypeArgument = null;
                return false;
            }

            Debug.Assert(genericTypeSymbol.TypeArguments.Length == 2);

            firstTypeArgument = genericTypeSymbol.TypeArguments[0];
            secondTypeArgument = genericTypeSymbol.TypeArguments[1];
            return true;
        }
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

        if (!TryGetVariantTypeForCoreTypes(elementTypeSymbol, out VariantType elementVariantType, out _))
        {
            // The element type, and by extension the array-like type, is not marshallable.
            return false;
        }

        if (!TryGetDefaultPropertyHintCore(compilation, elementTypeSymbol, elementVariantType, isNestedType: true, out PropertyHint elementHint, out string? elementHintString))
        {
            // We were unable to get a property hint for the element type,
            // but we can still create a property hint for the array.
            hint = PropertyHint.TypeString;
            hintString = ConstructTypeStringHint(elementVariantType, PropertyHint.None, null);
            return true;
        }

        hint = PropertyHint.TypeString;
        hintString = ConstructTypeStringHint(elementVariantType, elementHint, elementHintString);
        return true;
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

        if (!TryGetVariantTypeForCoreTypes(keyTypeSymbol, out VariantType keyVariantType, out _)
         || !TryGetVariantTypeForCoreTypes(valueTypeSymbol, out VariantType valueVariantType, out _))
        {
            // The key type or the value type, and by extension the dictionary-like type, is not marshallable.
            return false;
        }

        if (!TryGetDefaultPropertyHintCore(compilation, keyTypeSymbol, keyVariantType, isNestedType: true, out PropertyHint keyHint, out string? keyHintString))
        {
            // We were unable to get a property hint for the key type,
            // but we can still create a property hint for the dictionary.
            keyHint = PropertyHint.None;
            keyHintString = null;
        }

        if (!TryGetDefaultPropertyHintCore(compilation, valueTypeSymbol, valueVariantType, isNestedType: true, out PropertyHint valueHint, out string? valueHintString))
        {
            // We were unable to get a property hint for the key type,
            // but we can still create a property hint for the dictionary.
            valueHint = PropertyHint.None;
            valueHintString = null;
        }

        hint = PropertyHint.TypeString;
        string keyTypeStringHint = ConstructTypeStringHint(keyVariantType, keyHint, keyHintString);
        string valueTypeStringHint = ConstructTypeStringHint(valueVariantType, valueHint, valueHintString);
        hintString = ConstructDictionaryHintString(keyTypeStringHint, valueTypeStringHint);
        return true;

        static string ConstructDictionaryHintString(string keyTypeStringHint, string valueTypeStringHint)
        {
            // Format: "key_type_string_hint;value_type_string_hint"
            return $"{keyTypeStringHint};{valueTypeStringHint}";
        }
    }

    private static string ConstructTypeStringHint(VariantType variantType, PropertyHint hint, string? hintString)
    {
        // Format: "type/hint:hint_string"
        // IMPORTANT: The enums are formatted as numeric values.
        return $"{variantType:D}/{hint:D}:{hintString}";
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

    private static string MakeGenericGodotArray(ITypeSymbol elementTypeSymbol)
    {
        return $"global::{KnownTypeNames.GodotArray}<{elementTypeSymbol.FullNameWithGlobal()}>";
    }

    private static string MakeGenericGodotDictionary(ITypeSymbol keyTypeSymbol, ITypeSymbol valueTypeSymbol)
    {
        return $"global::{KnownTypeNames.GodotDictionary}<{keyTypeSymbol.FullNameWithGlobal()}, {valueTypeSymbol.FullNameWithGlobal()}>";
    }
}

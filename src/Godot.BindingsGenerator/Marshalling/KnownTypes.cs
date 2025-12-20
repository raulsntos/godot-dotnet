using System;
using System.Collections.Generic;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal static class KnownTypes
{
    // System types.

    public static TypeInfo SystemVoid { get; } = new TypeInfo("Void", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemVoidPtr { get; } = SystemVoid.MakePointerType();

    public static TypeInfo SystemByte { get; } = new TypeInfo("Byte", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemBoolean { get; } = new TypeInfo("Boolean", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemSByte { get; } = new TypeInfo("SByte", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemChar { get; } = new TypeInfo("Char", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemTextRune { get; } = new TypeInfo("Rune", "System.Text")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemInt16 { get; } = new TypeInfo("Int16", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemInt32 { get; } = new TypeInfo("Int32", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemInt64 { get; } = new TypeInfo("Int64", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemUInt16 { get; } = new TypeInfo("UInt16", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemUInt32 { get; } = new TypeInfo("UInt32", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemUInt64 { get; } = new TypeInfo("UInt64", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemHalf { get; } = new TypeInfo("Half", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemSingle { get; } = new TypeInfo("Single", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemDouble { get; } = new TypeInfo("Double", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemDecimal { get; } = new TypeInfo("Decimal", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemString { get; } = new TypeInfo("String", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemIntPtr { get; } = new TypeInfo("IntPtr", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemUIntPtr { get; } = new TypeInfo("UIntPtr", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo SystemObject { get; } = new TypeInfo("Object", "System")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    // The type System.Enum itself is not an enum but it's more convenient to use EnumInfo for it anyway.
    public static TypeInfo SystemEnum { get; } = new EnumInfo("Enum", "System");

    public static TypeInfo SystemSpan { get; } = new TypeInfo("Span", "System")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
        GenericTypeArgumentCount = 1,
    };

    public static TypeInfo SystemSpanOf(TypeInfo elementType)
    {
        return SystemSpan.MakeGenericType([elementType]);
    }

    public static TypeInfo SystemReadOnlySpan { get; } = new TypeInfo("ReadOnlySpan", "System")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
        GenericTypeArgumentCount = 1,
    };

    public static TypeInfo SystemReadOnlySpanOf(TypeInfo elementType)
    {
        return SystemReadOnlySpan.MakeGenericType([elementType]);
    }

    public static TypeInfo SystemArray { get; } = new TypeInfo("Array", "System")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
        GenericTypeArgumentCount = 1,
    };

    public static TypeInfo SystemArrayOf(TypeInfo elementType)
    {
        return SystemArray.MakeGenericType([elementType]);
    }

    public static TypeInfo SystemDictionary { get; } = new TypeInfo("Dictionary", "System.Collections.Generic")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
        GenericTypeArgumentCount = 2,
    };

    public static TypeInfo SystemDictionaryOf(TypeInfo keyType, TypeInfo valueType)
    {
        return SystemDictionary.MakeGenericType([keyType, valueType]);
    }

    public static TypeInfo SystemFrozenDictionary { get; } = new TypeInfo("FrozenDictionary", "System.Collections.Frozen")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
        GenericTypeArgumentCount = 2,
    };

    public static TypeInfo SystemFrozenDictionaryOf(TypeInfo keyType, TypeInfo valueType)
    {
        return SystemFrozenDictionary.MakeGenericType([keyType, valueType]);
    }

    public static TypeInfo SystemAction { get; } = new TypeInfo("Action", "System")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
        GenericTypeArgumentCount = 0,
    };

    public static TypeInfo SystemActionOf(IEnumerable<TypeInfo> typeArguments)
    {
        int typeArgumentsCount = typeArguments.Count();

        // The System.Action type only supports from 0 to 16 generic type arguments.
        ArgumentOutOfRangeException.ThrowIfNegative(typeArgumentsCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(typeArgumentsCount, 16);

        if (typeArgumentsCount == 0)
        {
            return SystemAction;
        }

        return new TypeInfo("Action", "System")
        {
            TypeAttributes = TypeAttributes.ReferenceType,
            GenericTypeArgumentCount = typeArgumentsCount,
        }.MakeGenericType(typeArguments);
    }

    public static TypeInfo SystemFuncOf(IEnumerable<TypeInfo> typeArguments)
    {
        int typeArgumentsCount = typeArguments.Count();

        // The System.Func type only supports from 1 to 16 generic type arguments
        // (with the last one being the return type).
        ArgumentOutOfRangeException.ThrowIfZero(typeArgumentsCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(typeArgumentsCount, 16);

        return new TypeInfo("Func", "System")
        {
            TypeAttributes = TypeAttributes.ReferenceType,
            GenericTypeArgumentCount = typeArgumentsCount,
        }.MakeGenericType(typeArguments);
    }

    public static TypeInfo Nullable { get; } = new TypeInfo("Nullable", "System")
    {
        TypeAttributes = TypeAttributes.ValueType,
        GenericTypeArgumentCount = 1,
    };

    public static TypeInfo NullableOf(TypeInfo valueType)
    {
        return Nullable.MakeGenericType([valueType]);
    }

    private static readonly Dictionary<int, TypeInfo> _inlineArrays = [];

    public static TypeInfo InlineArrayOf(int length)
    {
        if (_inlineArrays.TryGetValue(length, out var inlineArrayType))
        {
            return inlineArrayType;
        }

        if (length is < 2 or > 16)
        {
            // The runtime only provides InlineArray support for lengths 2 to 16.
            throw new InvalidOperationException("Inline arrays are only supported for lengths 2 to 16.");
        }

        inlineArrayType = new TypeInfo($"InlineArray{length}", "System.Runtime.CompilerServices")
        {
            TypeAttributes = TypeAttributes.ValueType,
            GenericTypeArgumentCount = 1,
        };
        _inlineArrays[length] = inlineArrayType;
        return inlineArrayType;
    }

    // Godot types.

    public static TypeInfo GodotAabb { get; } = new TypeInfo("Aabb", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotBasis { get; } = new TypeInfo("Basis", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotCallable { get; } = new TypeInfo("Callable", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotColor { get; } = new TypeInfo("Color", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotNodePath { get; } = new TypeInfo("NodePath", "Godot")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPlane { get; } = new TypeInfo("Plane", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotProjection { get; } = new TypeInfo("Projection", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotQuaternion { get; } = new TypeInfo("Quaternion", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotRect2 { get; } = new TypeInfo("Rect2", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotRect2I { get; } = new TypeInfo("Rect2I", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotRid { get; } = new TypeInfo("Rid", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotSignal { get; } = new TypeInfo("Signal", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotStringName { get; } = new TypeInfo("StringName", "Godot")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotTransform2D { get; } = new TypeInfo("Transform2D", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotTransform3D { get; } = new TypeInfo("Transform3D", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotVector2 { get; } = new TypeInfo("Vector2", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotVector2I { get; } = new TypeInfo("Vector2I", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotVector3 { get; } = new TypeInfo("Vector3", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotVector3I { get; } = new TypeInfo("Vector3I", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotVector4 { get; } = new TypeInfo("Vector4", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotVector4I { get; } = new TypeInfo("Vector4I", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotVariant { get; } = new TypeInfo("Variant", "Godot")
    {
        TypeAttributes = TypeAttributes.ValueType,
    };

    public static TypeInfo GodotObject { get; } = new TypeInfo("GodotObject", "Godot")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    // Godot collections.

    public static TypeInfo GodotPackedByteArray { get; } = new TypeInfo("PackedByteArray", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedInt32Array { get; } = new TypeInfo("PackedInt32Array", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedInt64Array { get; } = new TypeInfo("PackedInt64Array", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedFloat32Array { get; } = new TypeInfo("PackedFloat32Array", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedFloat64Array { get; } = new TypeInfo("PackedFloat64Array", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedStringArray { get; } = new TypeInfo("PackedStringArray", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedVector2Array { get; } = new TypeInfo("PackedVector2Array", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedVector3Array { get; } = new TypeInfo("PackedVector3Array", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedColorArray { get; } = new TypeInfo("PackedColorArray", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotPackedVector4Array { get; } = new TypeInfo("PackedVector4Array", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotArray { get; } = new TypeInfo("GodotArray", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotArrayGeneric { get; } = new TypeInfo("GodotArray", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
        GenericTypeArgumentCount = 1,
    };

    public static TypeInfo GodotArrayOf(TypeInfo elementType)
    {
        return GodotArrayGeneric.MakeGenericType([elementType]);
    }

    public static TypeInfo GodotDictionary { get; } = new TypeInfo("GodotDictionary", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
    };

    public static TypeInfo GodotDictionaryGeneric { get; } = new TypeInfo("GodotDictionary", "Godot.Collections")
    {
        TypeAttributes = TypeAttributes.ReferenceType,
        GenericTypeArgumentCount = 2,
    };

    public static TypeInfo GodotDictionaryOf(TypeInfo keyType, TypeInfo valueType)
    {
        return GodotDictionaryGeneric.MakeGenericType([keyType, valueType]);
    }

    // Godot native interop types.

    public static TypeInfo NativeGodotString { get; } = new TypeInfo("NativeGodotString", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotStringName { get; } = new TypeInfo("NativeGodotStringName", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotNodePath { get; } = new TypeInfo("NativeGodotNodePath", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotCallable { get; } = new TypeInfo("NativeGodotCallable", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotSignal { get; } = new TypeInfo("NativeGodotSignal", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotVariant { get; } = new TypeInfo("NativeGodotVariant", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotVector { get; } = new TypeInfo("NativeGodotVector", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
        GenericTypeArgumentCount = 1,
    };

    public static TypeInfo NativeGodotVectorOf(TypeInfo elementType)
    {
        return NativeGodotVector.MakeGenericType([elementType]);
    }

    public static TypeInfo NativeGodotPackedByteArray { get; } = new TypeInfo("NativeGodotPackedByteArray", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedInt32Array { get; } = new TypeInfo("NativeGodotPackedInt32Array", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedInt64Array { get; } = new TypeInfo("NativeGodotPackedInt64Array", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedFloat32Array { get; } = new TypeInfo("NativeGodotPackedFloat32Array", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedFloat64Array { get; } = new TypeInfo("NativeGodotPackedFloat64Array", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedStringArray { get; } = new TypeInfo("NativeGodotPackedStringArray", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedVector2Array { get; } = new TypeInfo("NativeGodotPackedVector2Array", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedVector3Array { get; } = new TypeInfo("NativeGodotPackedVector3Array", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedColorArray { get; } = new TypeInfo("NativeGodotPackedColorArray", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotPackedVector4Array { get; } = new TypeInfo("NativeGodotPackedVector4Array", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotArray { get; } = new TypeInfo("NativeGodotArray", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotDictionary { get; } = new TypeInfo("NativeGodotDictionary", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };

    public static TypeInfo NativeGodotVariantPtrSpan { get; } = new TypeInfo("NativeGodotVariantPtrSpan", "Godot.NativeInterop")
    {
        TypeAttributes = TypeAttributes.ByRefLikeType,
    };
}

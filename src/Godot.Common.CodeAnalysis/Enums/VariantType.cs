using System;

namespace Godot.Common.CodeAnalysis;

// IMPORTANT: Must match the type defined in GodotBindings/Generated/GlobalEnums/VariantType.cs

internal enum VariantType
{
    Nil = 0,
    Bool = 1,
    Int = 2,
    Float = 3,
    String = 4,
    Vector2 = 5,
    Vector2I = 6,
    Rect2 = 7,
    Rect2I = 8,
    Vector3 = 9,
    Vector3I = 10,
    Transform2D = 11,
    Vector4 = 12,
    Vector4I = 13,
    Plane = 14,
    Quaternion = 15,
    Aabb = 16,
    Basis = 17,
    Transform3D = 18,
    Projection = 19,
    Color = 20,
    StringName = 21,
    NodePath = 22,
    Rid = 23,
    Object = 24,
    Callable = 25,
    Signal = 26,
    Dictionary = 27,
    Array = 28,
    PackedByteArray = 29,
    PackedInt32Array = 30,
    PackedInt64Array = 31,
    PackedFloat32Array = 32,
    PackedFloat64Array = 33,
    PackedStringArray = 34,
    PackedVector2Array = 35,
    PackedVector3Array = 36,
    PackedColorArray = 37,
    PackedVector4Array = 38,
}

internal static class VariantTypeExtensions
{
    public static string FullNameWithGlobal(this VariantType variantType)
    {
        if (!Enum.IsDefined(typeof(VariantType), variantType))
        {
            throw new ArgumentOutOfRangeException(nameof(variantType), $"Unrecognized VariantType value '{variantType}'.");
        }

        return $"global::Godot.VariantType.{Enum.GetName(typeof(VariantType), variantType)}";
    }

    public static bool IsPackedArray(this VariantType variantType)
    {
        return variantType
            is VariantType.PackedByteArray
            or VariantType.PackedInt32Array
            or VariantType.PackedInt64Array
            or VariantType.PackedFloat32Array
            or VariantType.PackedFloat64Array
            or VariantType.PackedStringArray
            or VariantType.PackedVector2Array
            or VariantType.PackedVector3Array
            or VariantType.PackedColorArray
            or VariantType.PackedVector4Array;
    }
}

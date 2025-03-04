using Godot.Bridge;

namespace Godot.NativeInterop;

partial struct NativeGodotVariant
{
    internal static NativeGodotVariant Create(scoped in NativeGodotVariant from)
    {
        return from.Type switch
        {
            // These types can be created without using an interop call.
            VariantType.Nil => default,
            VariantType.Bool => new() { Bool = from.Bool, Type = VariantType.Bool },
            VariantType.Int => new() { Int = from.Int, Type = VariantType.Int },
            VariantType.Float => new() { Float = from.Float, Type = VariantType.Float },
            VariantType.Vector2 => new() { Vector2 = from.Vector2, Type = VariantType.Vector2 },
            VariantType.Vector2I => new() { Vector2I = from.Vector2I, Type = VariantType.Vector2I },
            VariantType.Rect2 => new() { Rect2 = from.Rect2, Type = VariantType.Rect2 },
            VariantType.Rect2I => new() { Rect2I = from.Rect2I, Type = VariantType.Rect2I },
            VariantType.Vector3 => new() { Vector3 = from.Vector3, Type = VariantType.Vector3 },
            VariantType.Vector3I => new() { Vector3I = from.Vector3I, Type = VariantType.Vector3I },
            VariantType.Vector4 => new() { Vector4 = from.Vector4, Type = VariantType.Vector4 },
            VariantType.Vector4I => new() { Vector4I = from.Vector4I, Type = VariantType.Vector4I },
            VariantType.Plane => new() { Plane = from.Plane, Type = VariantType.Plane },
            VariantType.Quaternion => new() { Quaternion = from.Quaternion, Type = VariantType.Quaternion },
            VariantType.Color => new() { Color = from.Color, Type = VariantType.Color },
            VariantType.Rid => new() { Rid = from.Rid, Type = VariantType.Rid },

            // Fallback to making an interop call to create the copy Variant.
            _ => CreateCore(from),
        };
    }

    private static unsafe NativeGodotVariant CreateCore(scoped in NativeGodotVariant from)
    {
        NativeGodotVariant dest = default;
        GodotBridge.GDExtensionInterface.variant_new_copy(&dest, from.GetUnsafeAddress());
        return dest;
    }

    internal static unsafe bool Equals(scoped in NativeGodotVariant left, scoped in NativeGodotVariant right)
    {
        return GodotBridge.GDExtensionInterface.variant_hash_compare(left.GetUnsafeAddress(), right.GetUnsafeAddress());
    }

    internal static unsafe int Compare(scoped in NativeGodotVariant left, scoped in NativeGodotVariant right)
    {
        // If the types are different we can avoid interop and just compare the type numeric value,
        // comparing different types is questionable anyway.
        if (left.Type != right.Type)
        {
            if (left.Type < right.Type)
            {
                return -1;
            }
            if (left.Type > right.Type)
            {
                return 1;
            }
            return 0;
        }

        if (LessThan(left, right))
        {
            return -1;
        }
        if (GreaterThan(left, right))
        {
            return 1;
        }
        return 0;

        static bool LessThan(scoped in NativeGodotVariant left, scoped in NativeGodotVariant right)
        {
            bool valid = false;
            NativeGodotVariant result = default;
            GodotBridge.GDExtensionInterface.variant_evaluate(GDExtensionVariantOperator.GDEXTENSION_VARIANT_OP_LESS, left.GetUnsafeAddress(), right.GetUnsafeAddress(), &result, &valid);
            return ConvertToBool(result);
        }

        static bool GreaterThan(scoped in NativeGodotVariant left, scoped in NativeGodotVariant right)
        {
            bool valid = false;
            NativeGodotVariant result = default;
            GodotBridge.GDExtensionInterface.variant_evaluate(GDExtensionVariantOperator.GDEXTENSION_VARIANT_OP_LESS, left.GetUnsafeAddress(), right.GetUnsafeAddress(), &result, &valid);
            return ConvertToBool(result);
        }
    }

    public unsafe void Dispose()
    {
        switch (Type)
        {
            case VariantType.Nil:
            case VariantType.Bool:
            case VariantType.Int:
            case VariantType.Float:
            case VariantType.Vector2:
            case VariantType.Vector2I:
            case VariantType.Rect2:
            case VariantType.Rect2I:
            case VariantType.Vector3:
            case VariantType.Vector3I:
            case VariantType.Vector4:
            case VariantType.Vector4I:
            case VariantType.Plane:
            case VariantType.Quaternion:
            case VariantType.Color:
            case VariantType.Rid:
                return;
        }

        GodotBridge.GDExtensionInterface.variant_destroy(GetUnsafeAddress());
        Type = VariantType.Nil;
    }
}

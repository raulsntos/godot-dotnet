using System;
using System.Runtime.CompilerServices;
using Godot.Collections;
using Godot.NativeInterop.Marshallers;

namespace Godot.NativeInterop;

partial class Marshalling
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NativeGodotVariant ConvertToVariant<[MustBeVariant] T>(scoped in T value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static TTo UnsafeAs<TTo>(in T value) => Unsafe.As<T, TTo>(ref Unsafe.AsRef(in value));

        // `typeof(T1) == typeof(T2)` is optimized away. We cannot cache `typeof(T)` in a local variable, as it's not optimized when done like that.

        if (typeof(T) == typeof(bool))
        {
            return NativeGodotVariant.CreateFromBool(UnsafeAs<bool>(value));
        }

        if (typeof(T) == typeof(char))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<char>(value));
        }

        if (typeof(T) == typeof(sbyte))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<sbyte>(value));
        }

        if (typeof(T) == typeof(short))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<short>(value));
        }

        if (typeof(T) == typeof(int))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<int>(value));
        }

        if (typeof(T) == typeof(long))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<long>(value));
        }

        if (typeof(T) == typeof(byte))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<byte>(value));
        }

        if (typeof(T) == typeof(ushort))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<ushort>(value));
        }

        if (typeof(T) == typeof(uint))
        {
            return NativeGodotVariant.CreateFromInt(UnsafeAs<uint>(value));
        }

        if (typeof(T) == typeof(ulong))
        {
            return NativeGodotVariant.CreateFromInt((long)UnsafeAs<ulong>(value));
        }

        if (typeof(T) == typeof(Half))
        {
            return NativeGodotVariant.CreateFromFloat((double)UnsafeAs<Half>(value));
        }

        if (typeof(T) == typeof(float))
        {
            return NativeGodotVariant.CreateFromFloat(UnsafeAs<float>(value));
        }

        if (typeof(T) == typeof(double))
        {
            return NativeGodotVariant.CreateFromFloat(UnsafeAs<double>(value));
        }

        if (typeof(T) == typeof(string))
        {
            return NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(UnsafeAs<string>(value)));
        }

        if (typeof(T) == typeof(Aabb))
        {
            return NativeGodotVariant.CreateFromAabb(UnsafeAs<Aabb>(value));
        }

        if (typeof(T) == typeof(Basis))
        {
            return NativeGodotVariant.CreateFromBasis(UnsafeAs<Basis>(value));
        }

        if (typeof(T) == typeof(Callable))
        {
            return NativeGodotVariant.CreateFromCallableCopying(UnsafeAs<Callable>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(Color))
        {
            return NativeGodotVariant.CreateFromColor(UnsafeAs<Color>(value));
        }

        if (typeof(T) == typeof(NodePath))
        {
            NodePath? nodePath = UnsafeAs<NodePath?>(value);
            return nodePath is not null ? NativeGodotVariant.CreateFromNodePathCopying(nodePath.NativeValue.DangerousSelfRef) : default;
        }

        if (typeof(T) == typeof(Plane))
        {
            return NativeGodotVariant.CreateFromPlane(UnsafeAs<Plane>(value));
        }

        if (typeof(T) == typeof(Projection))
        {
            return NativeGodotVariant.CreateFromProjection(UnsafeAs<Projection>(value));
        }

        if (typeof(T) == typeof(Quaternion))
        {
            return NativeGodotVariant.CreateFromQuaternion(UnsafeAs<Quaternion>(value));
        }

        if (typeof(T) == typeof(Rect2))
        {
            return NativeGodotVariant.CreateFromRect2(UnsafeAs<Rect2>(value));
        }

        if (typeof(T) == typeof(Rect2I))
        {
            return NativeGodotVariant.CreateFromRect2I(UnsafeAs<Rect2I>(value));
        }

        if (typeof(T) == typeof(Rid))
        {
            return NativeGodotVariant.CreateFromRid(UnsafeAs<Rid>(value));
        }

        if (typeof(T) == typeof(Signal))
        {
            return NativeGodotVariant.CreateFromSignalCopying(UnsafeAs<Signal>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(StringName))
        {
            StringName? stringName = UnsafeAs<StringName?>(value);
            return stringName is not null ? NativeGodotVariant.CreateFromStringNameCopying(stringName.NativeValue.DangerousSelfRef) : default;
        }

        if (typeof(T) == typeof(Transform2D))
        {
            return NativeGodotVariant.CreateFromTransform2D(UnsafeAs<Transform2D>(value));
        }

        if (typeof(T) == typeof(Transform3D))
        {
            return NativeGodotVariant.CreateFromTransform3D(UnsafeAs<Transform3D>(value));
        }

        if (typeof(T) == typeof(Vector2))
        {
            return NativeGodotVariant.CreateFromVector2(UnsafeAs<Vector2>(value));
        }

        if (typeof(T) == typeof(Vector2I))
        {
            return NativeGodotVariant.CreateFromVector2I(UnsafeAs<Vector2I>(value));
        }

        if (typeof(T) == typeof(Vector3))
        {
            return NativeGodotVariant.CreateFromVector3(UnsafeAs<Vector3>(value));
        }

        if (typeof(T) == typeof(Vector3I))
        {
            return NativeGodotVariant.CreateFromVector3I(UnsafeAs<Vector3I>(value));
        }

        if (typeof(T) == typeof(Vector4))
        {
            return NativeGodotVariant.CreateFromVector4(UnsafeAs<Vector4>(value));
        }

        if (typeof(T) == typeof(Vector4I))
        {
            return NativeGodotVariant.CreateFromVector4I(UnsafeAs<Vector4I>(value));
        }

        if (typeof(T) == typeof(Variant))
        {
            return NativeGodotVariant.Create(UnsafeAs<Variant>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedByteArray))
        {
            return NativeGodotVariant.CreateFromPackedByteArrayCopying(UnsafeAs<PackedByteArray>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedInt32Array))
        {
            return NativeGodotVariant.CreateFromPackedInt32ArrayCopying(UnsafeAs<PackedInt32Array>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedInt64Array))
        {
            return NativeGodotVariant.CreateFromPackedInt64ArrayCopying(UnsafeAs<PackedInt64Array>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedFloat32Array))
        {
            return NativeGodotVariant.CreateFromPackedFloat32ArrayCopying(UnsafeAs<PackedFloat32Array>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedFloat64Array))
        {
            return NativeGodotVariant.CreateFromPackedFloat64ArrayCopying(UnsafeAs<PackedFloat64Array>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedStringArray))
        {
            return NativeGodotVariant.CreateFromPackedStringArrayCopying(UnsafeAs<PackedStringArray>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedVector2Array))
        {
            return NativeGodotVariant.CreateFromPackedVector2ArrayCopying(UnsafeAs<PackedVector2Array>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedVector3Array))
        {
            return NativeGodotVariant.CreateFromPackedVector3ArrayCopying(UnsafeAs<PackedVector3Array>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(PackedColorArray))
        {
            return NativeGodotVariant.CreateFromPackedColorArrayCopying(UnsafeAs<PackedColorArray>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(GodotArray))
        {
            return NativeGodotVariant.CreateFromArrayCopying(UnsafeAs<GodotArray>(value).NativeValue.DangerousSelfRef);
        }

        if (typeof(T) == typeof(GodotDictionary))
        {
            return NativeGodotVariant.CreateFromDictionaryCopying(UnsafeAs<GodotDictionary>(value).NativeValue.DangerousSelfRef);
        }

        // More complex checks here at the end, to avoid screwing the simple ones in case they're not optimized away.

        // `typeof(T1).IsAssignableFrom(typeof(T2))` is optimized away.

        if (typeof(GodotObject).IsAssignableFrom(typeof(T)))
        {
            return NativeGodotVariant.CreateFromObject(GodotObject.GetNativePtr(UnsafeAs<GodotObject>(value)));
        }

        // `typeof(T).IsEnum` is optimized away.

        if (typeof(T).IsEnum)
        {
            // `Type.GetTypeCode(typeof(T).GetEnumUnderlyingType())` is not optimized away.
            // Fortunately, `Unsafe.SizeOf<T>()` works and is optimized away.
            // We don't need to know whether it's signed or unsigned.

            if (Unsafe.SizeOf<T>() == 1)
            {
                return NativeGodotVariant.CreateFromInt(UnsafeAs<sbyte>(value));
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                return NativeGodotVariant.CreateFromInt(UnsafeAs<short>(value));
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                return NativeGodotVariant.CreateFromInt(UnsafeAs<int>(value));
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                return NativeGodotVariant.CreateFromInt(UnsafeAs<long>(value));
            }

            ThrowUnsupportedType<T>();
        }

        return GenericConversion<T>.ConvertToVariant(in value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T ConvertFromVariant<[MustBeVariant] T>(scoped in NativeGodotVariant value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T UnsafeAsT<TFrom>(TFrom value) => Unsafe.As<TFrom, T>(ref Unsafe.AsRef(ref value));

        // `typeof(T1) == typeof(T2)` is optimized away. We cannot cache `typeof(T)` in a local variable, as it's not optimized when done like that.

        if (typeof(T) == typeof(bool))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToBool(value));
        }

        if (typeof(T) == typeof(char))
        {
            return UnsafeAsT((char)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(sbyte))
        {
            return UnsafeAsT((sbyte)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(short))
        {
            return UnsafeAsT((short)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(int))
        {
            return UnsafeAsT((int)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(long))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(byte))
        {
            return UnsafeAsT((byte)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(ushort))
        {
            return UnsafeAsT((ushort)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(uint))
        {
            return UnsafeAsT((uint)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(ulong))
        {
            return UnsafeAsT((ulong)NativeGodotVariant.ConvertToInt(value));
        }

        if (typeof(T) == typeof(Half))
        {
            return UnsafeAsT((Half)NativeGodotVariant.ConvertToFloat(value));
        }

        if (typeof(T) == typeof(float))
        {
            return UnsafeAsT((float)NativeGodotVariant.ConvertToFloat(value));
        }

        if (typeof(T) == typeof(double))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToFloat(value));
        }

        if (typeof(T) == typeof(string))
        {
            using NativeGodotString valueNative = NativeGodotVariant.GetOrConvertToString(value);
            return UnsafeAsT(valueNative.ToString());
        }

        if (typeof(T) == typeof(Aabb))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToAabb(value));
        }

        if (typeof(T) == typeof(Basis))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToBasis(value));
        }

        if (typeof(T) == typeof(Callable))
        {
            return UnsafeAsT(Callable.CreateTakingOwnership(NativeGodotVariant.ConvertToCallable(value)));
        }

        if (typeof(T) == typeof(Color))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToColor(value));
        }

        if (typeof(T) == typeof(NodePath))
        {
            return UnsafeAsT(NodePath.CreateTakingOwnership(NativeGodotVariant.ConvertToNodePath(value)));
        }

        if (typeof(T) == typeof(Plane))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToPlane(value));
        }

        if (typeof(T) == typeof(Projection))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToProjection(value));
        }

        if (typeof(T) == typeof(Quaternion))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToQuaternion(value));
        }

        if (typeof(T) == typeof(Rect2))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToRect2(value));
        }

        if (typeof(T) == typeof(Rect2I))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToRect2I(value));
        }

        if (typeof(T) == typeof(Rid))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToRid(value));
        }

        if (typeof(T) == typeof(Signal))
        {
            return UnsafeAsT(Signal.CreateTakingOwnership(NativeGodotVariant.ConvertToSignal(value)));
        }

        if (typeof(T) == typeof(StringName))
        {
            return UnsafeAsT(StringName.CreateTakingOwnership(NativeGodotVariant.ConvertToStringName(value)));
        }

        if (typeof(T) == typeof(Transform2D))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToTransform2D(value));
        }

        if (typeof(T) == typeof(Transform3D))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToTransform3D(value));
        }

        if (typeof(T) == typeof(Vector2))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToVector2(value));
        }

        if (typeof(T) == typeof(Vector2I))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToVector2I(value));
        }

        if (typeof(T) == typeof(Vector3))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToVector3(value));
        }

        if (typeof(T) == typeof(Vector3I))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToVector3I(value));
        }

        if (typeof(T) == typeof(Vector4))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToVector4(value));
        }

        if (typeof(T) == typeof(Vector4I))
        {
            return UnsafeAsT(NativeGodotVariant.ConvertToVector4I(value));
        }

        if (typeof(T) == typeof(Variant))
        {
            return UnsafeAsT(Variant.CreateCopying(value));
        }

        if (typeof(T) == typeof(PackedByteArray))
        {
            return UnsafeAsT(PackedByteArray.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedByteArray(value)));
        }

        if (typeof(T) == typeof(PackedInt32Array))
        {
            return UnsafeAsT(PackedInt32Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedInt32Array(value)));
        }

        if (typeof(T) == typeof(PackedInt64Array))
        {
            return UnsafeAsT(PackedInt64Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedInt64Array(value)));
        }

        if (typeof(T) == typeof(PackedFloat32Array))
        {
            return UnsafeAsT(PackedFloat32Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedFloat32Array(value)));
        }

        if (typeof(T) == typeof(PackedFloat64Array))
        {
            return UnsafeAsT(PackedFloat64Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedFloat64Array(value)));
        }

        if (typeof(T) == typeof(PackedStringArray))
        {
            return UnsafeAsT(PackedStringArray.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedStringArray(value)));
        }

        if (typeof(T) == typeof(PackedVector2Array))
        {
            return UnsafeAsT(PackedVector2Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedVector2Array(value)));
        }

        if (typeof(T) == typeof(PackedVector3Array))
        {
            return UnsafeAsT(PackedVector3Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedVector3Array(value)));
        }

        if (typeof(T) == typeof(PackedColorArray))
        {
            return UnsafeAsT(PackedColorArray.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedColorArray(value)));
        }

        if (typeof(T) == typeof(GodotArray))
        {
            return UnsafeAsT(GodotArray.CreateTakingOwnership(NativeGodotVariant.ConvertToArray(value)));
        }

        if (typeof(T) == typeof(GodotDictionary))
        {
            return UnsafeAsT(GodotDictionary.CreateTakingOwnership(NativeGodotVariant.ConvertToDictionary(value)));
        }

        // More complex checks here at the end, to avoid screwing the simple ones in case they're not optimized away.

        // `typeof(T1).IsAssignableFrom(typeof(T2))` is optimized away.

        if (typeof(GodotObject).IsAssignableFrom(typeof(T)))
        {
            return (T)(object)GodotObjectMarshaller.GetOrCreateManagedInstance(NativeGodotVariant.ConvertToObject(value))!;
        }

        // `typeof(T).IsEnum` is optimized away.

        if (typeof(T).IsEnum)
        {
            // `Type.GetTypeCode(typeof(T).GetEnumUnderlyingType())` is not optimized away.
            // Fortunately, `Unsafe.SizeOf<T>()` works and is optimized away.
            // We don't need to know whether it's signed or unsigned.

            if (Unsafe.SizeOf<T>() == 1)
            {
                return UnsafeAsT((sbyte)NativeGodotVariant.ConvertToInt(value));
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                return UnsafeAsT((short)NativeGodotVariant.ConvertToInt(value));
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                return UnsafeAsT((int)NativeGodotVariant.ConvertToInt(value));
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                return UnsafeAsT(NativeGodotVariant.ConvertToInt(value));
            }

            ThrowUnsupportedType<T>();
        }

        return GenericConversion<T>.ConvertFromVariant(in value);
    }
}

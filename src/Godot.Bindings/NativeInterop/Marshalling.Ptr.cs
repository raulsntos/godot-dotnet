using System;
using System.Runtime.CompilerServices;
using System.Text;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop.Marshallers;

namespace Godot.NativeInterop;

partial class Marshalling
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static unsafe void WriteUnmanaged<[MustBeVariant] T>(void* destination, scoped ref readonly T value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static TTo UnsafeAs<TTo>(in T value) => Unsafe.As<T, TTo>(ref Unsafe.AsRef(in value));

        // `typeof(T1) == typeof(T2)` is optimized away. We cannot cache `typeof(T)` in a local variable, as it's not optimized when done like that.

        if (typeof(T) == typeof(bool))
        {
            *(bool*)destination = UnsafeAs<bool>(value);
            return;
        }

        if (typeof(T) == typeof(char))
        {
            *(long*)destination = UnsafeAs<char>(value);
            return;
        }

        if (typeof(T) == typeof(Rune))
        {
            *(long*)destination = UnsafeAs<Rune>(value).Value;
        }

        if (typeof(T) == typeof(sbyte))
        {
            *(long*)destination = UnsafeAs<sbyte>(value);
            return;
        }

        if (typeof(T) == typeof(short))
        {
            *(long*)destination = UnsafeAs<short>(value);
            return;
        }

        if (typeof(T) == typeof(int))
        {
            *(long*)destination = UnsafeAs<int>(value);
            return;
        }

        if (typeof(T) == typeof(long))
        {
            *(long*)destination = UnsafeAs<long>(value);
            return;
        }

        if (typeof(T) == typeof(byte))
        {
            *(long*)destination = UnsafeAs<byte>(value);
            return;
        }

        if (typeof(T) == typeof(ushort))
        {
            *(long*)destination = UnsafeAs<ushort>(value);
            return;
        }

        if (typeof(T) == typeof(uint))
        {
            *(long*)destination = UnsafeAs<uint>(value);
            return;
        }

        if (typeof(T) == typeof(ulong))
        {
            *(long*)destination = (long)UnsafeAs<ulong>(value);
            return;
        }

        if (typeof(T) == typeof(Half))
        {
            *(double*)destination = (double)UnsafeAs<Half>(value);
        }

        if (typeof(T) == typeof(float))
        {
            *(double*)destination = UnsafeAs<float>(value);
            return;
        }

        if (typeof(T) == typeof(double))
        {
            *(double*)destination = UnsafeAs<double>(value);
            return;
        }

        if (typeof(T) == typeof(string))
        {
            *(NativeGodotString*)destination = NativeGodotString.Create(UnsafeAs<string>(value));
            return;
        }

        if (typeof(T) == typeof(Aabb))
        {
            *(Aabb*)destination = UnsafeAs<Aabb>(value);
            return;
        }

        if (typeof(T) == typeof(Basis))
        {
            *(Basis*)destination = UnsafeAs<Basis>(value);
            return;
        }

        if (typeof(T) == typeof(Callable))
        {
            *(NativeGodotCallable*)destination = UnsafeAs<Callable>(value).NativeValue.DangerousSelfRef;
            return;
        }

        if (typeof(T) == typeof(Color))
        {
            *(Color*)destination = UnsafeAs<Color>(value);
            return;
        }

        if (typeof(T) == typeof(NodePath))
        {
            *(NativeGodotNodePath*)destination = (UnsafeAs<NodePath?>(value)?.NativeValue ?? default).DangerousSelfRef;
            return;
        }

        if (typeof(T) == typeof(Plane))
        {
            *(Plane*)destination = UnsafeAs<Plane>(value);
            return;
        }

        if (typeof(T) == typeof(Projection))
        {
            *(Projection*)destination = UnsafeAs<Projection>(value);
            return;
        }

        if (typeof(T) == typeof(Quaternion))
        {
            *(Quaternion*)destination = UnsafeAs<Quaternion>(value);
            return;
        }

        if (typeof(T) == typeof(Rect2))
        {
            *(Rect2*)destination = UnsafeAs<Rect2>(value);
            return;
        }

        if (typeof(T) == typeof(Rect2I))
        {
            *(Rect2I*)destination = UnsafeAs<Rect2I>(value);
            return;
        }

        if (typeof(T) == typeof(Rid))
        {
            *(Rid*)destination = UnsafeAs<Rid>(value);
            return;
        }

        if (typeof(T) == typeof(Signal))
        {
            *(NativeGodotSignal*)destination = UnsafeAs<Signal>(value).NativeValue.DangerousSelfRef;
            return;
        }

        if (typeof(T) == typeof(StringName))
        {
            *(NativeGodotStringName*)destination = (UnsafeAs<StringName?>(value)?.NativeValue ?? default).DangerousSelfRef;
            return;
        }

        if (typeof(T) == typeof(Transform2D))
        {
            *(Transform2D*)destination = UnsafeAs<Transform2D>(value);
            return;
        }

        if (typeof(T) == typeof(Transform3D))
        {
            *(Transform3D*)destination = UnsafeAs<Transform3D>(value);
            return;
        }

        if (typeof(T) == typeof(Vector2))
        {
            *(Vector2*)destination = UnsafeAs<Vector2>(value);
            return;
        }

        if (typeof(T) == typeof(Vector2I))
        {
            *(Vector2I*)destination = UnsafeAs<Vector2I>(value);
            return;
        }

        if (typeof(T) == typeof(Vector3))
        {
            *(Vector3*)destination = UnsafeAs<Vector3>(value);
            return;
        }

        if (typeof(T) == typeof(Vector3I))
        {
            *(Vector3I*)destination = UnsafeAs<Vector3I>(value);
            return;
        }

        if (typeof(T) == typeof(Vector4))
        {
            *(Vector4*)destination = UnsafeAs<Vector4>(value);
            return;
        }

        if (typeof(T) == typeof(Vector4I))
        {
            *(Vector4I*)destination = UnsafeAs<Vector4I>(value);
            return;
        }

        if (typeof(T) == typeof(Variant))
        {
            *(NativeGodotVariant*)destination = UnsafeAs<Variant>(value).NativeValue.DangerousSelfRef;
            return;
        }

        if (typeof(T) == typeof(PackedByteArray))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedByteArray*)destination, UnsafeAs<PackedByteArray?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedInt32Array))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedInt32Array*)destination, UnsafeAs<PackedInt32Array?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedInt64Array))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedInt64Array*)destination, UnsafeAs<PackedInt64Array?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedFloat32Array))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedFloat32Array*)destination, UnsafeAs<PackedFloat32Array?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedFloat64Array))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedFloat64Array*)destination, UnsafeAs<PackedFloat64Array?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedStringArray))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedStringArray*)destination, UnsafeAs<PackedStringArray?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedVector2Array))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedVector2Array*)destination, UnsafeAs<PackedVector2Array?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedVector3Array))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedVector3Array*)destination, UnsafeAs<PackedVector3Array?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedColorArray))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedColorArray*)destination, UnsafeAs<PackedColorArray?>(value));
            return;
        }

        if (typeof(T) == typeof(PackedVector4Array))
        {
            PackedArrayMarshaller.WriteUnmanaged((NativeGodotPackedVector4Array*)destination, UnsafeAs<PackedVector4Array?>(value));
            return;
        }

        if (typeof(T) == typeof(GodotArray))
        {
            GodotArrayMarshaller.WriteUnmanaged((NativeGodotArray*)destination, UnsafeAs<GodotArray?>(value));
            return;
        }

        if (typeof(T) == typeof(GodotDictionary))
        {
            GodotDictionaryMarshaller.WriteUnmanaged((NativeGodotDictionary*)destination, UnsafeAs<GodotDictionary?>(value));
            return;
        }

        // More complex checks here at the end, to avoid screwing the simple ones in case they're not optimized away.

        // `typeof(T1).IsAssignableFrom(typeof(T2))` is optimized away.

        if (typeof(GodotObject).IsAssignableFrom(typeof(T)))
        {
            GodotObjectMarshaller.WriteUnmanaged((nint*)destination, UnsafeAs<GodotObject?>(value));
            return;
        }

        // `typeof(T).IsEnum` is optimized away.

        if (typeof(T).IsEnum)
        {
            // `Type.GetTypeCode(typeof(T).GetEnumUnderlyingType())` is not optimized away.
            // Fortunately, `Unsafe.SizeOf<T>()` works and is optimized away.
            // We don't need to know whether it's signed or unsigned.

            if (Unsafe.SizeOf<T>() == 1)
            {
                *(long*)destination = UnsafeAs<sbyte>(value);
                return;
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                *(long*)destination = UnsafeAs<short>(value);
                return;
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                *(long*)destination = UnsafeAs<int>(value);
                return;
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                *(long*)destination = UnsafeAs<long>(value);
                return;
            }

            ThrowUnsupportedType<T>();
        }

        GenericConversion<T>.WriteUnmanaged(in value, destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static unsafe T ConvertFromUnmanaged<[MustBeVariant] T>(void* value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T UnsafeAsT<TFrom>(TFrom value) => Unsafe.As<TFrom, T>(ref Unsafe.AsRef(ref value));

        // `typeof(T1) == typeof(T2)` is optimized away. We cannot cache `typeof(T)` in a local variable, as it's not optimized when done like that.

        if (typeof(T) == typeof(bool))
        {
            return UnsafeAsT(*(bool*)value);
        }

        if (typeof(T) == typeof(char))
        {
            return UnsafeAsT((char)*(long*)value);
        }

        if (typeof(T) == typeof(Rune))
        {
            return UnsafeAsT((Rune)(int)*(long*)value);
        }

        if (typeof(T) == typeof(sbyte))
        {
            return UnsafeAsT((sbyte)*(long*)value);
        }

        if (typeof(T) == typeof(short))
        {
            return UnsafeAsT((short)*(long*)value);
        }

        if (typeof(T) == typeof(int))
        {
            return UnsafeAsT((int)*(long*)value);
        }

        if (typeof(T) == typeof(long))
        {
            return UnsafeAsT(*(long*)value);
        }

        if (typeof(T) == typeof(byte))
        {
            return UnsafeAsT((byte)*(long*)value);
        }

        if (typeof(T) == typeof(ushort))
        {
            return UnsafeAsT((short)*(long*)value);
        }

        if (typeof(T) == typeof(uint))
        {
            return UnsafeAsT((uint)*(long*)value);
        }

        if (typeof(T) == typeof(ulong))
        {
            return UnsafeAsT((ulong)*(long*)value);
        }

        if (typeof(T) == typeof(Half))
        {
            return UnsafeAsT((Half)(*(double*)value));
        }

        if (typeof(T) == typeof(float))
        {
            return UnsafeAsT((float)*(double*)value);
        }

        if (typeof(T) == typeof(double))
        {
            return UnsafeAsT(*(double*)value);
        }

        if (typeof(T) == typeof(string))
        {
            return UnsafeAsT((*(NativeGodotString*)value).ToString());
        }

        if (typeof(T) == typeof(Aabb))
        {
            return UnsafeAsT(*(Aabb*)value);
        }

        if (typeof(T) == typeof(Basis))
        {
            return UnsafeAsT(*(Basis*)value);
        }

        if (typeof(T) == typeof(Callable))
        {
            return UnsafeAsT(CallableMarshaller.ConvertFromUnmanaged((NativeGodotCallable*)value));
        }

        if (typeof(T) == typeof(Color))
        {
            return UnsafeAsT(*(Color*)value);
        }

        if (typeof(T) == typeof(NodePath))
        {
            return UnsafeAsT(NodePath.CreateTakingOwnership(*(NativeGodotNodePath*)value));
        }

        if (typeof(T) == typeof(Plane))
        {
            return UnsafeAsT(*(Plane*)value);
        }

        if (typeof(T) == typeof(Projection))
        {
            return UnsafeAsT(*(Projection*)value);
        }

        if (typeof(T) == typeof(Quaternion))
        {
            return UnsafeAsT(*(Quaternion*)value);
        }

        if (typeof(T) == typeof(Rect2))
        {
            return UnsafeAsT(*(Rect2*)value);
        }

        if (typeof(T) == typeof(Rect2I))
        {
            return UnsafeAsT(*(Rect2I*)value);
        }

        if (typeof(T) == typeof(Rid))
        {
            return UnsafeAsT(*(Rid*)value);
        }

        if (typeof(T) == typeof(Signal))
        {
            return UnsafeAsT(Signal.CreateTakingOwnership(*(NativeGodotSignal*)value));
        }

        if (typeof(T) == typeof(StringName))
        {
            return UnsafeAsT(StringName.CreateTakingOwnership(*(NativeGodotStringName*)value));
        }

        if (typeof(T) == typeof(Transform2D))
        {
            return UnsafeAsT(*(Transform2D*)value);
        }

        if (typeof(T) == typeof(Transform3D))
        {
            return UnsafeAsT(*(Transform3D*)value);
        }

        if (typeof(T) == typeof(Vector2))
        {
            return UnsafeAsT(*(Vector2*)value);
        }

        if (typeof(T) == typeof(Vector2I))
        {
            return UnsafeAsT(*(Vector2I*)value);
        }

        if (typeof(T) == typeof(Vector3))
        {
            return UnsafeAsT(*(Vector3*)value);
        }

        if (typeof(T) == typeof(Vector3I))
        {
            return UnsafeAsT(*(Vector3I*)value);
        }

        if (typeof(T) == typeof(Vector4))
        {
            return UnsafeAsT(*(Vector4*)value);
        }

        if (typeof(T) == typeof(Vector4I))
        {
            return UnsafeAsT(*(Vector4I*)value);
        }

        if (typeof(T) == typeof(Variant))
        {
            return UnsafeAsT(Variant.CreateTakingOwnership(*(NativeGodotVariant*)value));
        }

        if (typeof(T) == typeof(PackedByteArray))
        {
            return UnsafeAsT(PackedByteArray.CreateTakingOwnership(*(NativeGodotPackedByteArray*)value));
        }

        if (typeof(T) == typeof(PackedInt32Array))
        {
            return UnsafeAsT(PackedInt32Array.CreateTakingOwnership(*(NativeGodotPackedInt32Array*)value));
        }

        if (typeof(T) == typeof(PackedInt64Array))
        {
            return UnsafeAsT(PackedInt64Array.CreateTakingOwnership(*(NativeGodotPackedInt64Array*)value));
        }

        if (typeof(T) == typeof(PackedFloat32Array))
        {
            return UnsafeAsT(PackedFloat32Array.CreateTakingOwnership(*(NativeGodotPackedFloat32Array*)value));
        }

        if (typeof(T) == typeof(PackedFloat64Array))
        {
            return UnsafeAsT(PackedFloat64Array.CreateTakingOwnership(*(NativeGodotPackedFloat64Array*)value));
        }

        if (typeof(T) == typeof(PackedStringArray))
        {
            return UnsafeAsT(PackedStringArray.CreateTakingOwnership(*(NativeGodotPackedStringArray*)value));
        }

        if (typeof(T) == typeof(PackedVector2Array))
        {
            return UnsafeAsT(PackedVector2Array.CreateTakingOwnership(*(NativeGodotPackedVector2Array*)value));
        }

        if (typeof(T) == typeof(PackedVector3Array))
        {
            return UnsafeAsT(PackedVector3Array.CreateTakingOwnership(*(NativeGodotPackedVector3Array*)value));
        }

        if (typeof(T) == typeof(PackedColorArray))
        {
            return UnsafeAsT(PackedColorArray.CreateTakingOwnership(*(NativeGodotPackedColorArray*)value));
        }

        if (typeof(T) == typeof(PackedVector4Array))
        {
            return UnsafeAsT(PackedVector4Array.CreateTakingOwnership(*(NativeGodotPackedVector4Array*)value));
        }

        if (typeof(T) == typeof(GodotArray))
        {
            return UnsafeAsT(GodotArray.CreateTakingOwnership(*(NativeGodotArray*)value));
        }

        if (typeof(T) == typeof(GodotDictionary))
        {
            return UnsafeAsT(GodotDictionary.CreateTakingOwnership(*(NativeGodotDictionary*)value));
        }

        // More complex checks here at the end, to avoid screwing the simple ones in case they're not optimized away.

        // `typeof(T1).IsAssignableFrom(typeof(T2))` is optimized away.

        if (typeof(GodotObject).IsAssignableFrom(typeof(T)))
        {
            // If the pointer is null we can skip all the operations below.
            if (value is null)
            {
                return default!;
            }

            if (typeof(RefCounted).IsAssignableFrom(typeof(T)))
            {
                // In virtual methods, if T is RefCounted we need to use `ref_get_object` to get
                // the real NativePtr for the GodotObject.
                // See https://github.com/godotengine/godot-cpp/issues/954.
                value = GodotBridge.GDExtensionInterface.ref_get_object(value);
            }
            else
            {
                // Even if T is not RefCounted, the instance could still be RefCounted.
                // For example, in a virtual method like `EditorPlugin::_Handles(GodotObject)`
                // where the GodotObject parameter could be a RefCounted since it's a derived type.
                // So let's attempt to get the ref ptr anyway, even thought doing this has a cost
                // it should not crash. If the GodotObject is not RefCounted it will just return null.
                void* refPtr = GodotBridge.GDExtensionInterface.ref_get_object(value);
                if (refPtr is not null)
                {
                    value = refPtr;
                }
                else
                {
                    // If the GodotObject is not RefCounted, the pointer is 'Object**'
                    // so we need to dereference it.
                    value = *(void**)value;
                }
            }

            return (T)(object)GodotObjectMarshaller.GetOrCreateManagedInstance((nint)value)!;
        }

        // `typeof(T).IsEnum` is optimized away.

        if (typeof(T).IsEnum)
        {
            // `Type.GetTypeCode(typeof(T).GetEnumUnderlyingType())` is not optimized away.
            // Fortunately, `Unsafe.SizeOf<T>()` works and is optimized away.
            // We don't need to know whether it's signed or unsigned.

            if (Unsafe.SizeOf<T>() == 1)
            {
                return UnsafeAsT((sbyte)*(long*)value);
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                return UnsafeAsT((short)*(long*)value);
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                return UnsafeAsT((int)*(long*)value);
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                return UnsafeAsT(*(long*)value);
            }

            ThrowUnsupportedType<T>();
        }

        return GenericConversion<T>.ConvertFromUnmanaged(value);
    }
}

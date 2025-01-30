using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.Collections;

namespace Godot.NativeInterop.Marshallers;

internal static unsafe class PackedArrayMarshaller
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedByteArray* destination, PackedByteArray? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedByteArray* ConvertToUnmanaged(PackedByteArray? value)
    {
        NativeGodotPackedByteArray* ptr = (NativeGodotPackedByteArray*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedByteArray));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedByteArray ConvertFromUnmanaged(NativeGodotPackedByteArray* value)
    {
        Debug.Assert(value is not null);
        return PackedByteArray.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedByteArray* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedInt32Array* destination, PackedInt32Array? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedInt32Array* ConvertToUnmanaged(PackedInt32Array? value)
    {
        NativeGodotPackedInt32Array* ptr = (NativeGodotPackedInt32Array*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedInt32Array));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedInt32Array ConvertFromUnmanaged(NativeGodotPackedInt32Array* value)
    {
        Debug.Assert(value is not null);
        return PackedInt32Array.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedInt32Array* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedInt64Array* destination, PackedInt64Array? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedInt64Array* ConvertToUnmanaged(PackedInt64Array? value)
    {
        NativeGodotPackedInt64Array* ptr = (NativeGodotPackedInt64Array*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedInt64Array));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedInt64Array ConvertFromUnmanaged(NativeGodotPackedInt64Array* value)
    {
        Debug.Assert(value is not null);
        return PackedInt64Array.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedInt64Array* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedFloat32Array* destination, PackedFloat32Array? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedFloat32Array* ConvertToUnmanaged(PackedFloat32Array? value)
    {
        NativeGodotPackedFloat32Array* ptr = (NativeGodotPackedFloat32Array*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedFloat32Array));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedFloat32Array ConvertFromUnmanaged(NativeGodotPackedFloat32Array* value)
    {
        Debug.Assert(value is not null);
        return PackedFloat32Array.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedFloat32Array* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedFloat64Array* destination, PackedFloat64Array? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedFloat64Array* ConvertToUnmanaged(PackedFloat64Array? value)
    {
        NativeGodotPackedFloat64Array* ptr = (NativeGodotPackedFloat64Array*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedFloat64Array));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedFloat64Array ConvertFromUnmanaged(NativeGodotPackedFloat64Array* value)
    {
        Debug.Assert(value is not null);
        return PackedFloat64Array.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedFloat64Array* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedStringArray* destination, PackedStringArray? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedStringArray* ConvertToUnmanaged(PackedStringArray? value)
    {
        NativeGodotPackedStringArray* ptr = (NativeGodotPackedStringArray*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedStringArray));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedStringArray ConvertFromUnmanaged(NativeGodotPackedStringArray* value)
    {
        Debug.Assert(value is not null);
        return PackedStringArray.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedStringArray* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedVector2Array* destination, PackedVector2Array? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedVector2Array* ConvertToUnmanaged(PackedVector2Array? value)
    {
        NativeGodotPackedVector2Array* ptr = (NativeGodotPackedVector2Array*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedVector2Array));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedVector2Array ConvertFromUnmanaged(NativeGodotPackedVector2Array* value)
    {
        Debug.Assert(value is not null);
        return PackedVector2Array.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedVector2Array* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedVector3Array* destination, PackedVector3Array? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedVector3Array* ConvertToUnmanaged(PackedVector3Array? value)
    {
        NativeGodotPackedVector3Array* ptr = (NativeGodotPackedVector3Array*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedVector3Array));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedVector3Array ConvertFromUnmanaged(NativeGodotPackedVector3Array* value)
    {
        Debug.Assert(value is not null);
        return PackedVector3Array.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedVector3Array* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedColorArray* destination, PackedColorArray? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedColorArray* ConvertToUnmanaged(PackedColorArray? value)
    {
        NativeGodotPackedColorArray* ptr = (NativeGodotPackedColorArray*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedColorArray));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedColorArray ConvertFromUnmanaged(NativeGodotPackedColorArray* value)
    {
        Debug.Assert(value is not null);
        return PackedColorArray.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedColorArray* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotPackedVector4Array* destination, PackedVector4Array? value)
    {
        *destination = (value?.NativeValue ?? default).DangerousSelfRef;
    }

    public static NativeGodotPackedVector4Array* ConvertToUnmanaged(PackedVector4Array? value)
    {
        NativeGodotPackedVector4Array* ptr = (NativeGodotPackedVector4Array*)NativeMemory.Alloc((nuint)sizeof(NativeGodotPackedVector4Array));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static PackedVector4Array ConvertFromUnmanaged(NativeGodotPackedVector4Array* value)
    {
        Debug.Assert(value is not null);
        return PackedVector4Array.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotPackedVector4Array* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }
}

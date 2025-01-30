using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot.Collections;

namespace Godot.NativeInterop.Marshallers;

internal static unsafe class GodotArrayMarshaller
{
    // This is used to avoid creating a new instance of GodotArray everytime
    // when marshalling null arrays because the engine needs to receive a non-null
    // instance.
    private static readonly GodotArray _emptyArray;

    static GodotArrayMarshaller()
    {
        // We make the array read-only to ensure it's not mutated,
        // since this instance will be shared.
        _emptyArray = [];
        _emptyArray.MakeReadOnly();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotArray* destination, GodotArray? value)
    {
        value ??= _emptyArray;
        *destination = value.NativeValue.DangerousSelfRef;
    }

    public static NativeGodotArray* ConvertToUnmanaged(GodotArray? value)
    {
        value ??= _emptyArray;
        return value.NativeValue.DangerousSelfRef.GetUnsafeAddress();
    }

    public static GodotArray ConvertFromUnmanaged(NativeGodotArray* value)
    {
        Debug.Assert(value is not null);
        return GodotArray.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotArray* value) { }
}

internal static unsafe class GodotArrayMarshaller<[MustBeVariant] T>
{
    public static NativeGodotArray* ConvertToUnmanaged(GodotArray<T>? value)
    {
        if (value is null)
        {
            return GodotArrayMarshaller.ConvertToUnmanaged(null);
        }

        return value.NativeValue.DangerousSelfRef.GetUnsafeAddress();
    }

    public static GodotArray<T> ConvertFromUnmanaged(NativeGodotArray* value)
    {
        Debug.Assert(value is not null);
        return GodotArray<T>.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotArray* value) { }
}

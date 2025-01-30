using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot.Collections;

namespace Godot.NativeInterop.Marshallers;

internal static unsafe class GodotDictionaryMarshaller
{
    // This is used to avoid creating a new instance of GodotDictionary everytime
    // when marshalling null arrays because the engine needs to receive a non-null
    // instance.
    private static readonly GodotDictionary _emptyDictionary;

    static GodotDictionaryMarshaller()
    {
        // We make the dictionary read-only to ensure it's not mutated,
        // since this instance will be shared.
        _emptyDictionary = [];
        _emptyDictionary.MakeReadOnly();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotDictionary* destination, GodotDictionary? value)
    {
        value ??= _emptyDictionary;
        *destination = value.NativeValue.DangerousSelfRef;
    }

    public static NativeGodotDictionary* ConvertToUnmanaged(GodotDictionary? value)
    {
        value ??= _emptyDictionary;
        return value.NativeValue.DangerousSelfRef.GetUnsafeAddress();
    }

    public static GodotDictionary ConvertFromUnmanaged(NativeGodotDictionary* value)
    {
        Debug.Assert(value is not null);
        return GodotDictionary.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotDictionary* value) { }
}

internal static unsafe class GodotDictionaryMarshaller<[MustBeVariant] TKey, [MustBeVariant] TValue>
{
    public static NativeGodotDictionary* ConvertToUnmanaged(GodotDictionary<TKey, TValue>? value)
    {
        if (value is null)
        {
            return GodotDictionaryMarshaller.ConvertToUnmanaged(null);
        }

        return value.NativeValue.DangerousSelfRef.GetUnsafeAddress();
    }

    public static GodotDictionary<TKey, TValue> ConvertFromUnmanaged(NativeGodotDictionary* value)
    {
        Debug.Assert(value is not null);
        return GodotDictionary<TKey, TValue>.CreateTakingOwnership(*value);
    }

    public static void Free(NativeGodotDictionary* value) { }
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.Bridge;

namespace Godot.NativeInterop;

// IMPORTANT:
// A correctly constructed value needs to call the native default constructor to allocate `_p`.
// Don't pass a C# default constructed `NativeGodotArray` to native code, unless it's going to
// be re-assigned a new value (the copy constructor checks if `_p` is null so that's fine).
partial struct NativeGodotArray
{
    [FieldOffset(0)]
    private unsafe void* _p;

    internal readonly unsafe bool IsAllocated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null;
    }

    internal readonly unsafe int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null ? checked((int)GetSize(in this)) : 0;
    }

    internal readonly unsafe bool IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null && GetIsReadOnly(in this);
    }

    internal readonly unsafe NativeGodotVariant* GetPtrw()
    {
        return GodotBridge.GDExtensionInterface.array_operator_index(GetUnsafeAddress(), 0);
    }

    internal static unsafe NativeGodotArray Create<[MustBeVariant] T>(ReadOnlySpan<T> value)
    {
        NativeGodotArray destination = Create();

        Resize(ref destination, value.Length);

        NativeGodotVariant* buffer = destination.GetPtrw();

        for (int i = 0; i < value.Length; i++)
        {
            buffer[i] = Marshalling.ConvertToVariant(value[i]);
        }

        return destination;
    }

    internal readonly unsafe T[] ToArray<[MustBeVariant] T>()
    {
        int size = Size;
        if (size == 0)
        {
            return [];
        }

        NativeGodotVariant* buffer = GetPtrw();

        T[] destination = new T[size];
        for (int i = 0; i < size; i++)
        {
            destination[i] = Marshalling.ConvertFromVariant<T>(buffer[i]);
        }

        return destination;
    }
}

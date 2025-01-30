using System;
using System.Runtime.CompilerServices;

namespace Godot.NativeInterop;

partial struct NativeGodotPackedStringArray
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe NativeGodotPackedStringArray Create(scoped ReadOnlySpan<string> value)
    {
        if (value.IsEmpty)
        {
            return default;
        }

        NativeGodotPackedStringArray destination = default;
        Resize(ref destination, value.Length);

        NativeGodotString* ptrw = destination.GetPtrw();

        for (int i = 0; i < value.Length; i++)
        {
            ptrw[i] = NativeGodotString.Create(value[i]);
        }

        return destination;
    }

    internal readonly unsafe string[] ToArray()
    {
        int size = Size;
        if (size == 0)
        {
            return [];
        }

        NativeGodotString* buffer = GetPtrw();

        string[] destination = new string[size];
        for (int i = 0; i < size; i++)
        {
            destination[i] = buffer[i].ToString();
        }

        return destination;
    }
}

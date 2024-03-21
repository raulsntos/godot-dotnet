using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Godot.Bridge;

namespace Godot.NativeInterop;

partial struct NativeGodotString
{
    [FieldOffset(0)]
    private nint _ptr;

    internal readonly bool IsAllocated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _ptr != 0;
    }

    // Size including the null termination character.
    internal readonly unsafe int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // This code must match the method 'CowData::get_size' in the engine side.
            return _ptr != 0 ? (int)*((ulong*)_ptr - 1) : 0;
        }
    }

    internal unsafe static NativeGodotString Create(char* utf16)
    {
        NativeGodotString dest = default;
        GodotBridge.GDExtensionInterface.string_new_with_utf16_chars(dest.GetUnsafeAddress(), (ushort*)utf16);
        return dest;
    }

    internal unsafe static NativeGodotString Create(string? utf16)
    {
        if (utf16 is null)
        {
            return default;
        }

        fixed (char* utf16Ptr = utf16)
        {
            return Create(utf16Ptr);
        }
    }

    internal unsafe static NativeGodotString Create(ReadOnlySpan<byte> utf8)
    {
        if (utf8.IsEmpty)
        {
            return default;
        }

        NativeGodotString dest = default;
        fixed (byte* utf8Ptr = utf8)
        {
            GodotBridge.GDExtensionInterface.string_new_with_utf8_chars_and_len(&dest, utf8Ptr, utf8.Length);
            return dest;
        }
    }

    public override readonly unsafe string ToString()
    {
        if (!IsAllocated)
        {
            return string.Empty;
        }

        int size = Size;
        if (size == 0)
        {
            return string.Empty;
        }

        // Size of the string without the null termination character.
        size -= 1;

        const int SizeOfChar32 = 4;
        int sizeInBytes = size * SizeOfChar32;
        return Encoding.UTF32.GetString((byte*)_ptr, sizeInBytes);
    }
}

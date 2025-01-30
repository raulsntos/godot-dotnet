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

    internal static unsafe NativeGodotString Create(scoped ReadOnlySpan<byte> utf8)
    {
        if (!TryCreate(utf8, out NativeGodotString dest))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_UnableToParseString(Encoding.UTF8.GetString(utf8), "UTF-8"));
        }

        return dest;
    }

    internal static unsafe NativeGodotString Create(scoped ReadOnlySpan<char> utf16)
    {
        if (!TryCreate(utf16, out NativeGodotString dest))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_UnableToParseString(utf16.ToString(), "UTF-16"));
        }

        return dest;
    }

    internal static unsafe bool TryCreate(scoped ReadOnlySpan<byte> utf8, out NativeGodotString value)
    {
        if (utf8.IsEmpty)
        {
            value = default;
            return true;
        }

        NativeGodotString dest = default;
        fixed (byte* utf8Ptr = utf8)
        {
            Error error = (Error)GodotBridge.GDExtensionInterface.string_new_with_utf8_chars_and_len2(&dest, utf8Ptr, utf8.Length);
            if (error != Error.Ok)
            {
                value = default;
                return false;
            }

            value = dest;
            return true;
        }
    }

    internal static unsafe bool TryCreate(scoped ReadOnlySpan<char> utf16, out NativeGodotString value)
    {
        if (utf16.IsEmpty)
        {
            value = default;
            return true;
        }

        NativeGodotString dest = default;
        fixed (char* utf16Ptr = utf16)
        {
            Error error = (Error)GodotBridge.GDExtensionInterface.string_new_with_utf16_chars_and_len2(&dest, (ushort*)utf16Ptr, utf16.Length, BitConverter.IsLittleEndian);
            if (error != Error.Ok)
            {
                value = default;
                return false;
            }

            value = dest;
            return true;
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

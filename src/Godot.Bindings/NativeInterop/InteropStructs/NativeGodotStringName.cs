using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Godot.Bridge;

namespace Godot.NativeInterop;

partial struct NativeGodotStringName
{
    [FieldOffset(0)]
    private nint _ptr;

    internal readonly bool IsAllocated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _ptr != 0;
    }

    internal readonly bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // This is all that's needed to check if it's empty. Equivalent to `== StringName()` in C++.
        get => _ptr == 0;
    }

    public static bool operator ==(NativeGodotStringName left, NativeGodotStringName right)
    {
        return left._ptr == right._ptr;
    }

    public static bool operator !=(NativeGodotStringName left, NativeGodotStringName right)
    {
        return left._ptr != right._ptr;
    }

    public static bool operator ==(NativeGodotStringName left, StringName? right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NativeGodotStringName left, StringName? right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(StringName? left, NativeGodotStringName right)
    {
        return right.Equals(left);
    }

    public static bool operator !=(StringName? left, NativeGodotStringName right)
    {
        return !right.Equals(left);
    }

    public readonly bool Equals(NativeGodotStringName other)
    {
        return _ptr == other._ptr;
    }

    public readonly bool Equals([NotNullWhen(true)] StringName? other)
    {
        return other is not null && other.NativeValue.DangerousSelfRef.Equals(this);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is StringName s && Equals(s);
    }

    public override readonly int GetHashCode()
    {
        return _ptr.GetHashCode();
    }

    internal unsafe static NativeGodotStringName Create(scoped ReadOnlySpan<byte> utf8)
    {
        if (utf8.IsEmpty)
        {
            return default;
        }

        NativeGodotStringName dest = default;
        fixed (byte* utf8Ptr = utf8)
        {
            GodotBridge.GDExtensionInterface.string_name_new_with_utf8_chars_and_len(&dest, utf8Ptr, utf8.Length);
            return dest;
        }
    }

    internal unsafe static NativeGodotStringName Create(scoped ReadOnlySpan<byte> ascii, bool isStatic)
    {
        if (ascii.IsEmpty)
        {
            return default;
        }

        NativeGodotStringName dest = default;
        fixed (byte* asciiPtr = ascii)
        {
            GodotBridge.GDExtensionInterface.string_name_new_with_latin1_chars(&dest, asciiPtr, isStatic);
            return dest;
        }
    }

    internal unsafe static NativeGodotStringName Create(scoped ReadOnlySpan<char> utf16)
    {
        if (utf16.IsEmpty)
        {
            return default;
        }

        const int MaxByteCount = 16;
        int byteCount = Encoding.UTF8.GetByteCount(utf16);

        byte[]? tempArray = null;
        Span<byte> utf8 = byteCount <= MaxByteCount
            ? stackalloc byte[MaxByteCount]
            : tempArray = ArrayPool<byte>.Shared.Rent(byteCount);

        try
        {
            int actualByteCount = Encoding.UTF8.GetBytes(utf16, utf8);
            utf8 = utf8[..actualByteCount];
            return Create(utf8);
        }
        finally
        {
            if (tempArray is not null)
            {
                utf8.Clear();
                ArrayPool<byte>.Shared.Return(tempArray);
            }
        }
    }
}

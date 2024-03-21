using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct NativeGodotVector<T> where T : unmanaged
{
    private readonly nint _writeProxy;

    private unsafe readonly T* _ptr;

    public unsafe readonly int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // This code must match the method 'CowData::get_size' in the engine side.
            return _ptr is not null ? (int)*((ulong*)_ptr - 1) : 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe readonly T* GetPtrw()
    {
        return _ptr;
    }
}

// These types are used when the generic type can't be used because T would be a ref struct
// and ref structs can't be used as generic type arguments yet.
// TODO: Remove when C# implements support for ref structs in generic type arguments.
// See: https://github.com/dotnet/csharplang/issues/1148

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct NativeGodotVectorOfString
{
    private readonly nint _writeProxy;

    private unsafe readonly NativeGodotString* _ptr;

    public unsafe readonly int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // This code must match the method 'CowData::get_size' in the engine side.
            return _ptr is not null ? (int)*((ulong*)_ptr - 1) : 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe readonly NativeGodotString* GetPtrw()
    {
        return _ptr;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct NativeGodotVectorOfVariant
{
    private readonly nint _writeProxy;

    private unsafe readonly NativeGodotVariant* _ptr;

    public unsafe readonly int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // This code must match the method 'CowData::get_size' in the engine side.
            return _ptr is not null ? (int)*((ulong*)_ptr - 1) : 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe readonly NativeGodotVariant* GetPtrw()
    {
        return _ptr;
    }
}

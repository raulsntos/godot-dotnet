using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct NativeGodotVector<T> where T : unmanaged, allows ref struct
{
    private readonly nint _writeProxy;

    private readonly unsafe T* _ptr;

    public readonly unsafe int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // This code must match the method 'CowData::get_size' in the engine side.
            return _ptr is not null ? (int)*((ulong*)_ptr - 1) : 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly unsafe T* GetPtrw()
    {
        return _ptr;
    }
}

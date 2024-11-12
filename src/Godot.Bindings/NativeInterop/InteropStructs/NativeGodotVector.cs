using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct NativeGodotVector<T> where T : unmanaged, allows ref struct
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

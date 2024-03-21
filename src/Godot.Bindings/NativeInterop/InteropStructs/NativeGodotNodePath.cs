using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

partial struct NativeGodotNodePath
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
        // This is all that's needed to check if it's empty. It's what the `is_empty()` C++ method does.
        get => _ptr == 0;
    }
}

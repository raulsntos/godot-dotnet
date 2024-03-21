using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

partial struct NativeGodotSignal
{
    [FieldOffset(0)]
    private NativeGodotStringName _name;

    // There's padding here on 32-bit

    [FieldOffset(8)]
    private ulong _objectId;

    private readonly bool IsAllocated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _name.IsAllocated;
    }

    internal readonly NativeGodotStringName Name
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _name;
    }

    internal readonly ulong ObjectId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _objectId;
    }
}

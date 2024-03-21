using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

partial struct NativeGodotCallable
{
    [FieldOffset(0)]
    private NativeGodotStringName _method;

    // There's padding here on 32-bit

    [FieldOffset(8)]
    private ulong _objectId;

    [FieldOffset(8)]
    private nint _custom;

    private readonly bool IsAllocated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _method.IsAllocated || _custom != 0;
    }

    internal readonly NativeGodotStringName Method
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _method;
    }

    internal readonly ulong ObjectId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _objectId;
    }
}

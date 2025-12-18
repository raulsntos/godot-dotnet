using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.Bridge;

namespace Godot.NativeInterop;

// IMPORTANT:
// A correctly constructed value needs to call the native default constructor to allocate `_p`.
// Don't pass a C# default constructed `NativeGodotDictionary` to native code, unless it's going to
// be re-assigned a new value (the copy constructor checks if `_p` is null so that's fine).
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The name matches the name of the native Godot type that it represents.")]
partial struct NativeGodotDictionary
{
    [FieldOffset(0)]
    private unsafe void* _p;

    internal readonly unsafe bool IsAllocated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null;
    }

    internal readonly unsafe bool IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null && GetIsReadOnly(in this);
    }

    internal readonly unsafe NativeGodotVariant* GetPtrw(scoped in NativeGodotVariant key)
    {
        return GodotBridge.GDExtensionInterface.dictionary_operator_index(GetUnsafeAddress(), key.GetUnsafeAddress());
    }
}

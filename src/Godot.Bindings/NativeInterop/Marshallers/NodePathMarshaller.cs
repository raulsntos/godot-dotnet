using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop.Marshallers;

internal unsafe static class NodePathMarshaller
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotNodePath* destination, NodePath? value)
    {
        value ??= NodePath.Empty;
        *destination = value.NativeValue.DangerousSelfRef;
    }

    public static NativeGodotNodePath* ConvertToUnmanaged(NodePath? value)
    {
        NativeGodotNodePath* ptr = (NativeGodotNodePath*)Marshal.AllocHGlobal(sizeof(NativeGodotNodePath));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static NodePath? ConvertFromUnmanaged(NativeGodotNodePath* value)
    {
        Debug.Assert(value is not null);
        return value->IsAllocated
            ? NodePath.CreateTakingOwnership(*value)
            : null;
    }

    public static void Free(NativeGodotNodePath* value)
    {
        Marshal.FreeHGlobal((nint)value);
    }

    public static NativeGodotVariant* ConvertToVariant(NodePath? value)
    {
        value ??= NodePath.Empty;
        NativeGodotVariant* ptr = (NativeGodotVariant*)Marshal.AllocHGlobal(sizeof(NativeGodotVariant));
        *ptr = NativeGodotVariant.CreateFromNodePathTakingOwnership(value.NativeValue.DangerousSelfRef);
        return ptr;
    }

    public static NodePath? ConvertFromVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        Debug.Assert(value->Type == VariantType.NodePath);
        NativeGodotNodePath valueNative = NativeGodotVariant.ConvertToNodePath(*value);
        return valueNative.IsAllocated
            ? NodePath.CreateTakingOwnership(valueNative)
            : null;
    }

    public static void FreeVariant(NativeGodotVariant* value)
    {
        Marshal.FreeHGlobal((nint)value);
    }
}


using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop.Marshallers;

internal static unsafe class StringNameMarshaller
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotStringName* destination, StringName? value)
    {
        value ??= StringName.Empty;
        *destination = value.NativeValue.DangerousSelfRef;
    }

    public static NativeGodotStringName* ConvertToUnmanaged(StringName? value)
    {
        NativeGodotStringName* ptr = (NativeGodotStringName*)NativeMemory.Alloc((nuint)sizeof(NativeGodotStringName));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static StringName? ConvertFromUnmanaged(NativeGodotStringName* value)
    {
        Debug.Assert(value is not null);
        return value->IsAllocated
            ? StringName.CreateTakingOwnership(*value)
            : null;
    }

    public static void Free(NativeGodotStringName* value)
    {
        Debug.Assert(value is not null);
        NativeMemory.Free(value);
    }

    public static NativeGodotVariant* ConvertToVariant(StringName? value)
    {
        value ??= StringName.Empty;
        NativeGodotVariant* ptr = (NativeGodotVariant*)NativeMemory.Alloc((nuint)sizeof(NativeGodotVariant));
        *ptr = NativeGodotVariant.CreateFromStringNameTakingOwnership(value.NativeValue.DangerousSelfRef);
        return ptr;
    }

    public static StringName? ConvertFromVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        Debug.Assert(value->Type == VariantType.StringName);
        NativeGodotStringName valueNative = NativeGodotVariant.ConvertToStringName(*value);
        return valueNative.IsAllocated
            ? StringName.CreateTakingOwnership(valueNative)
            : null;
    }

    public static void FreeVariant(NativeGodotVariant* value)
    {
        NativeMemory.Free(value);
    }
}

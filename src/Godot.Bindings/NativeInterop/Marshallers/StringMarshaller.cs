using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop.Marshallers;

internal static unsafe class StringMarshaller
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(NativeGodotString* destination, string? value)
    {
        *destination = NativeGodotString.Create(value);
    }

    public static NativeGodotString* ConvertToUnmanaged(string? value)
    {
        NativeGodotString* ptr = (NativeGodotString*)NativeMemory.Alloc((nuint)sizeof(NativeGodotString));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static string ConvertFromUnmanaged(NativeGodotString* value)
    {
        Debug.Assert(value is not null);
        return value->ToString();
    }

    public static void Free(NativeGodotString* value)
    {
        Debug.Assert(value is not null);
        value->Dispose();
        NativeMemory.Free(value);
    }

    public static NativeGodotVariant* ConvertToVariant(string? value)
    {
        NativeGodotVariant* ptr = (NativeGodotVariant*)NativeMemory.Alloc((nuint)sizeof(NativeGodotVariant));
        *ptr = NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(value));
        return ptr;
    }

    public static string ConvertFromVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        return NativeGodotVariant.GetOrConvertToString(*value).ToString();
    }

    public static void FreeVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        value->Dispose();
        NativeMemory.Free(value);
    }
}

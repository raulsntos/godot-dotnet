using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.Bridge;

namespace Godot.NativeInterop.Marshallers;

internal unsafe class GodotObjectMarshaller
{
    /// <summary>
    /// Retrieves the existing managed instance of a <see cref="GodotObject"/> for the given
    /// native pointer if one already exists; otherwise, it creates a new managed instance
    /// for the unmanaged instance referenced by the native pointer.
    /// </summary>
    /// <param name="nativePtr">Pointer to the unmanaged <see cref="GodotObject"/> instance.</param>
    /// <returns>
    /// A managed <see cref="GodotObject"/> instance that represents the unmanaged instance in C#.
    /// </returns>
    internal static GodotObject? GetOrCreateManagedInstance(nint nativePtr)
    {
        if (nativePtr == 0)
        {
            return null;
        }

        // Get existing instance binding, if one already exists.
        void* instance = GodotBridge.GDExtensionInterface.object_get_instance_binding((void*)nativePtr, GodotBridge.LibraryPtr, null);
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            return (GodotObject?)gcHandle.Target;
        }

        // Otherwise, try to look up the create helper.
        NativeGodotStringName nativeClassName = default;
        if (GodotBridge.GDExtensionInterface.object_get_class_name((void*)nativePtr, GodotBridge.LibraryPtr, nativeClassName.GetUnsafeAddress()))
        {
            using StringName nativeClassNameManaged = StringName.CreateCopying(nativeClassName);
            Debug.Assert(InteropUtils.CreateHelpers.ContainsKey(nativeClassNameManaged), $"Create helper for class named '{nativeClassNameManaged}' not found.");
            if (InteropUtils.CreateHelpers.TryGetValue(nativeClassNameManaged, out var createHelper))
            {
                return createHelper(nativePtr);
            }
        }

        // We couldn't find an existing C# instance or create helper.
        // We'll just create a GodotObject instance since that should always be a common ancestor.
        return new GodotObject(nativePtr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(nint* destination, GodotObject? value)
    {
        *destination = GodotObject.GetNativePtr(value);
    }

    public static nint* ConvertToUnmanaged(GodotObject? value)
    {
        nint* ptr = (nint*)NativeMemory.Alloc((nuint)sizeof(nint));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static GodotObject? ConvertFromUnmanaged(nint* value)
    {
        Debug.Assert(value is not null);
        return GetOrCreateManagedInstance(*value);
    }

    public static void Free(nint* value)
    {
        NativeMemory.Free(value);
    }

    public static NativeGodotVariant* ConvertToVariant(GodotObject? value)
    {
        NativeGodotVariant* ptr = (NativeGodotVariant*)NativeMemory.Alloc((nuint)sizeof(NativeGodotVariant));
        *ptr = NativeGodotVariant.CreateFromObject(GodotObject.GetNativePtr(value));
        return ptr;
    }

    public static GodotObject? ConvertFromVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        Debug.Assert(value->Type == VariantType.Object);
        nint nativePtr = value->Object;
        return GetOrCreateManagedInstance(nativePtr);
    }

    public static void FreeVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        value->Dispose();
        NativeMemory.Free(value);
    }
}

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
    /// <param name="memoryOwn">
    /// Indicates whether the managed instance is a new object and should initialize the reference count
    /// if it's a <see cref="RefCounted"/> instance.
    /// </param>
    /// <returns>
    /// A managed <see cref="GodotObject"/> instance that represents the unmanaged instance in C#.
    /// </returns>
    internal static GodotObject? GetOrCreateManagedInstance(nint nativePtr, bool memoryOwn = true)
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
            var target = (GodotObject?)gcHandle.Target;
            if (target is RefCounted refCounted)
            {
                refCounted.Unreference();
            }
            return target;
        }

        // Otherwise, try to look up the create helper.
        NativeGodotStringName nativeClassName = default;
        if (GodotBridge.GDExtensionInterface.object_get_class_name((void*)nativePtr, GodotBridge.LibraryPtr, nativeClassName.GetUnsafeAddress()))
        {
            using StringName nativeClassNameManaged = StringName.CreateCopying(nativeClassName);
            Debug.Assert(InteropUtils.CreateHelpers.ContainsKey(nativeClassNameManaged), $"Create helper for class named '{nativeClassNameManaged}' not found.");
            if (InteropUtils.CreateHelpers.TryGetValue(nativeClassNameManaged, out var createHelper))
            {
                return createHelper(nativePtr, memoryOwn);
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

    public static GodotObject? ConvertFromUnmanaged(nint* value, bool memoryOwn = true)
    {
        Debug.Assert(value is not null);
        return GetOrCreateManagedInstance(*value, memoryOwn);
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

    public static GodotObject? ConvertFromVariant(NativeGodotVariant* value, bool memoryOwn = true)
    {
        Debug.Assert(value is not null);
        Debug.Assert(value->Type == VariantType.Object);
        nint nativePtr = value->Object;
        return GetOrCreateManagedInstance(nativePtr, memoryOwn);
    }

    public static void FreeVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        value->Dispose();
        NativeMemory.Free(value);
    }
}

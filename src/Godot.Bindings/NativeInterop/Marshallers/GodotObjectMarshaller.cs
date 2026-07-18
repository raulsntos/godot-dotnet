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
        // IMPORTANT: `incrementReferenceCount` is set to true here because this method is used when
        // unmarshalling and we have to increment the reference count to account for the C# reference.
        // Calling it with `incrementReferenceCount` set to false would be dangerous and should only
        // be done in very specific scenarios, so we don't want to expose the parameter to avoid misuse.
        return GetOrCreateManagedInstance(nativePtr, incrementReferenceCount: true);
    }

    private static GodotObject? GetOrCreateManagedInstance(nint nativePtr, bool incrementReferenceCount)
    {
        if (nativePtr == 0)
        {
            return null;
        }

        // Get existing instance binding, if one already exists.
        void* instance = GodotBridge.GDExtensionInterface.object_get_instance_binding((void*)nativePtr, GodotBridge.LibraryPtr, null);
        if (instance is not null)
        {
            var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
            var target = gcHandle.Target;
            HandleRefCounted(target, incrementReferenceCount);
            return target;
        }

        // Otherwise, try to look up the correct binding callbacks.
        NativeGodotStringName nativeClassName = default;
        GDExtensionInstanceBindingCallbacks bindingCallbacks;
        if (GodotBridge.GDExtensionInterface.object_get_class_name((void*)nativePtr, GodotBridge.LibraryPtr, nativeClassName.GetUnsafeAddress()))
        {
            if (GodotRegistry.TryGetClassRegistrationContext(nativeClassName, out var context))
            {
                // If the returned class name is in the registry, it means this is an user-defined type
                // so it doesn't have binding callbacks and should have been found by the previous lookup.
                // This likely means the C# instance has been disposed, so we'll attempt to fall back to
                // the closest native type. This is undefined behavior and will likely still break somewhere
                // down the line, but it seems safer than falling back to GodotObject.
                nativeClassName = context.NativeClassName.NativeValue.DangerousSelfRef;
            }

            if (!InteropUtils.TryGetBindingCallbacks(nativeClassName, out bindingCallbacks))
            {
                Debug.Fail($"Binding callbacks for '{StringName.CreateTakingOwnership(nativeClassName)}' not found.");
                bindingCallbacks = GodotObject.BindingCallbacks;
            }
        }
        else
        {
            bindingCallbacks = GodotObject.BindingCallbacks;
        }

        {
            instance = GodotBridge.GDExtensionInterface.object_get_instance_binding((void*)nativePtr, GodotBridge.LibraryPtr, &bindingCallbacks);
            Debug.Assert(instance is not null, "Instance binding should have been created by now.");
            var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
            var target = gcHandle.Target;
            HandleRefCounted(target, incrementReferenceCount);
            return target;
        }

        static void HandleRefCounted(GodotObject obj, bool incrementReferenceCount)
        {
            if (obj is not RefCounted refCounted)
            {
                return;
            }

            if (!incrementReferenceCount)
            {
                // If we're receiving an instance but we're not supposed to call `InitRef`,
                // it means the engine already incremented the reference count for us
                // (e.g., from a ptrcall return). So we just mark it as owned to avoid
                // incrementing the reference count again in the future.
                refCounted.MarkAsReferenceOwned();
                return;
            }

            refCounted.InitRefOnlyOnce();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUnmanaged(nint* destination, GodotObject? value)
    {
        *destination = GodotObject.GetNativePtr(value);
    }

    /// <summary>
    /// Writes a <see cref="RefCounted"/>-derived return value into an engine-side <c>Ref&lt;T&gt;</c>
    /// slot for a ptrcall return. Unlike <see cref="WriteUnmanaged(nint*, GodotObject?)"/> (which
    /// writes a raw object pointer, correct for <c>Object*</c> slots and for encoding arguments), a
    /// <c>Ref&lt;T&gt;</c> return slot must be populated through <c>ref_set_object</c> so the engine
    /// takes its own counted reference; writing the raw pointer would leave the engine with an
    /// un-counted reference and free the object once the managed reference is released
    /// (use-after-free). Mirrors godot-cpp's <c>PtrToArg&lt;Ref&lt;T&gt;&gt;::encode</c>.
    /// </summary>
    public static void WriteUnmanagedRefCounted(void* destination, GodotObject? value)
    {
        nint nativePtr = GodotObject.GetNativePtr(value);
        if (nativePtr == 0)
        {
            // The engine's return slot starts as an unset (null) Ref<T>; leave it for a null return.
            return;
        }
        GodotBridge.GDExtensionInterface.ref_set_object(destination, (void*)nativePtr);
    }

    public static nint* ConvertToUnmanaged(GodotObject? value)
    {
        nint* ptr = (nint*)NativeMemory.Alloc((nuint)sizeof(nint));
        WriteUnmanaged(ptr, value);
        return ptr;
    }

    public static GodotObject? ConvertFromUnmanaged(nint* value)
    {
        // IMPORTANT: `incrementReferenceCount` is set to false here because this method
        // is always used to unmarshall ptrcall returns, and in these cases the reference
        // count has already been incremented to account for the C# reference.
        Debug.Assert(value is not null);
        return GetOrCreateManagedInstance(*value, incrementReferenceCount: false);
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
        nint nativePtr = NativeGodotVariant.ConvertToObject(*value);
        return GetOrCreateManagedInstance(nativePtr);
    }

    public static void FreeVariant(NativeGodotVariant* value)
    {
        Debug.Assert(value is not null);
        value->Dispose();
        NativeMemory.Free(value);
    }
}

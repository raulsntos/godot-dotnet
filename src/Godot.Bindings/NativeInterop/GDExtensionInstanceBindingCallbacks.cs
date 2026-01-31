using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

partial struct GDExtensionInstanceBindingCallbacks
{
    internal static unsafe GDExtensionInstanceBindingCallbacks Default => new()
    {
        create_callback = &DefaultCreateBindingCallback_Native,
        free_callback = &DefaultFreeBindingCallback_Native,
        reference_callback = &DefaultReferenceBindingCallback_Native,
    };

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void* DefaultCreateBindingCallback_Native(void* token, void* nativePtr)
    {
        return null;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void DefaultFreeBindingCallback_Native(void* token, void* nativePtr, void* instance) { }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool DefaultReferenceBindingCallback_Native(void* token, void* nativePtr, bool reference)
    {
        return true;
    }
}

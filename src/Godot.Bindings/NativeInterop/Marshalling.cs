using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Godot.NativeInterop;

internal static partial class Marshalling
{
    internal static class GenericConversion<T>
    {
        public static unsafe NativeGodotVariant ConvertToVariant(scoped in T from)
        {
            ThrowUnsupportedTypeIfNull<T>(ToVariantCb);
            return ToVariantCb(Unsafe.AsRef(in from));
        }

        public static unsafe T ConvertFromVariant(scoped in NativeGodotVariant variant)
        {
            ThrowUnsupportedTypeIfNull<T>(FromVariantCb);
            return FromVariantCb(variant);
        }

        public static unsafe T ConvertFromUnmanaged(void* ptr)
        {
            ThrowUnsupportedTypeIfNull<T>(FromPtrCb);
            return FromPtrCb(ptr);
        }

        public static unsafe void WriteUnmanaged(ref readonly T value, void* destination)
        {
            if (AssignToPtrCb is not null)
            {
                AssignToPtrCb(in value, destination);
                return;
            }

            ThrowUnsupportedType<T>();
        }

        internal static unsafe delegate*<in T, NativeGodotVariant> ToVariantCb;

        internal static unsafe delegate*<in NativeGodotVariant, T> FromVariantCb;

        internal static unsafe delegate*<void*, T> FromPtrCb;

        internal static unsafe delegate*<in T, void*, void> AssignToPtrCb;

        // Force T's static constructor to run so that types like GodotArray<T> and
        // GodotDictionary<TKey, TValue> register their marshalling callbacks (the fields above)
        // before they're marshalled. RunClassConstructor is trim/AOT-safe, unlike reflecting over
        // Type.TypeInitializer which returns null under trimming/NativeAOT and silently skips the
        // registration (leaving the callbacks null and marshalling unsupported). This matches the
        // Godot mono glue (VariantUtils.generic.cs).
        [UnconditionalSuppressMessage("Trimming", "IL2059",
            Justification = "For bound members the source generator emits a concrete " +
                "RunClassConstructor(typeof(...).TypeHandle) which is what actually preserves and runs the " +
                "collection type's static constructor under trimming/NativeAOT. This generic call is a " +
                "best-effort fallback for other paths (matching the Godot mono glue, VariantUtils.generic.cs); " +
                "collection instantiations that are never otherwise constructed (e.g. Variant.As<T> on such a " +
                "type) are not guaranteed under NativeAOT.")]
        static GenericConversion()
        {
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        }
    }

    private static unsafe void ThrowUnsupportedTypeIfNull<T>([NotNull] void* cb)
    {
        if (cb is null)
        {
            ThrowUnsupportedType<T>();
        }
    }

    [DoesNotReturn]
    private static void ThrowUnsupportedType<T>()
    {
        throw new InvalidOperationException(SR.FormatInvalidOperation_MarshallingUnsupportedForType(typeof(T).FullName));
    }
}

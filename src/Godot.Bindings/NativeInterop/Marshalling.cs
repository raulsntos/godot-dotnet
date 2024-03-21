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

        static GenericConversion()
        {
            // TODO: This won't work with trimming because the static constructors
            // in GodotArray<T> and GodotDictionary<TKey, TValue> that initialize
            // GenericConversion<T> could be trimmed.
            typeof(T).TypeInitializer?.Invoke(null, null);
        }
    }

    private unsafe static void ThrowUnsupportedTypeIfNull<T>([NotNull] void* cb)
    {
        if (cb is null)
        {
            ThrowUnsupportedType<T>();
        }
    }

    [DoesNotReturn]
    private static void ThrowUnsupportedType<T>()
    {
        throw new InvalidOperationException($"The type is not supported for conversion to/from Variant: '{typeof(T).FullName}'.");
    }
}

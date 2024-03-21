# Godot.BindingsGenerator (Marshalling)

This directory contains the marshaller writers. These writers are used to write the code that converts between the managed and unmanaged blittable types so they cross between managed and native code.

These types are part of the low-level API that is only meant to be used for [marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/type-marshalling) and interoperability between the C# managed types and the native Godot types so they can be used with the GDExtension APIs.

This repository [disables .NET runtime marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/disabled-marshalling) so every C# managed type must be converted to an unmanaged blittable type so it can cross between managed and native code. Most primitive .NET types (including `bool`) are blittable when runtime marshalling is disabled, some C# structs in `Variant` and the [native interop structs](../../Godot.Bindings/NativeInterop/InteropStructs) are also blittable.

These types are used to handle the types when a simple conversion is possible, like for most structs, but when the code to generate is too complex a `RuntimeMarshallerWriter` is used instead. This marshaller writer delegates the conversion to a [runtime marshaller](../../Godot.Bindings/NativeInterop/Marshallers) implemented in the bindings package.

There are 2 types of marshaller writers:

- `PtrMarshallerWriter` writes conversion code to be used in ptrcalls. It converts between a managed type `T` and an unmanaged pointer type `U*`.

- `VariantMarshallerWriter` writes conversion code to be used in vararg calls. It converts between a managed type `T` and the interop struct `NativeGodotVariant`.

Every marshaller writer implements one of those base writers and is registered to be used by the generator when it needs to implement marshalling (i.e.: inside a `CallMethodBody`).

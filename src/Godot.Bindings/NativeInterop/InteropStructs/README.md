# Godot.NativeInterop (InteropStructs)

This directory contains the interop struct types. These types are unmanaged types that must match the same struct layout as defined in the engine, their layout and some functions are generated from `extensions_api.json`. The types in this directory are partial definitions that add to the generated types.

These types are part of the low-level API that is only meant to be used for [marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/type-marshalling) and interoperability between the C# managed types and the native Godot types so they can be used with the GDExtension APIs.

This repository [disables .NET runtime marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/disabled-marshalling) so every C# managed type must be converted to an unmanaged blittable type so it can cross between managed and native code. Most primitive .NET types (including `bool`) are blittable when runtime marshalling is disabled, some C# structs in `Variant` and the native interop structs contained in this directory are also blittable.

## Constructing and converting interop structs

Every interop struct contains a static `Create` method that serves as a constructor for the interop struct. The parameter-less method will create an zeroed or default initialized instance, the methods that take a parameter copy the value of that parameter to construct a new instance.

The `NativeGodotVariant` type contains multiple `CreateFrom*` static methods that allow creating a native Variant instance from the given value. These methods can follow one of three patterns:

- `CreateFrom{Type}` (no suffix) - These methods are usually for blittable types where we can copy the value and store it in the `NativeGodotVariant` instance directly without calling a native constructor so it's really cheap.

- `CreateFrom{Type}Copying` - These methods are for types that are disposable so they use a native constructor to copy the value. The constructed `NativeGodotVariant` can be disposed without affecting the original value.

- `CreateFrom{Type}TakingOwnership` - These methods are for types that are disposable but they take the value and store it in the `NativeGodotVariant` instance directly without calling a native constructor. It's really cheap, but disposing the constructed `NativeGodotVariant` will also dispose the original value.

The `NativeGodotVariant` type also contains multiple `ConvertTo*` static methods that allow converting from a native Variant instance to the specified type. These methods can follow one of two patterns:

- `ConvertTo{Type}` - These methods create a new instance of the given type from the `NativeGodotVariant` instance. If the type is not disposable, it will try to get a copy of the value directly stored in the `NativeGodotVariant` instance if the Variant matches the requested type, otherwise it will fallback to using a native constructor to convert the value. If the type is disposable it will **always** use a native constructor to copy or convert the value, so disposing the returned value will not affect the `NativeGodotVariant` instance.

- `GetOrConvertTo{Type}` - These methods will try to get the value directly stored in the `NativeGodotVariant` instance if the Variant matches the requested type, otherwise it will fallback to using a native constructor to convert the value. If the Variant matches the type it's a really cheap conversion because it avoids interop but since it takes the value directly, if the type is disposable it will also dispose the `NativeGodotVariant` instance.

Packed arrays also have an `AsSpan` method that allows getting direct access to the backing memory of the packed array. The span element type must the blittable type that matches the struct layout in the engine, so this method is only available for the packed arrays where this type is not a `ref struct` (because `ref structs` can't be used as generic types).

## Movable types

The interop structs are defined as `ref struct` so they can't be stored in types that aren't also a `ref struct`. This prevents accidentally copying or storing these types which can lead to memory leaks, but it also means we can't store a reference to them in the managed C# types that are implemented to expose the types in the public API. To by-pass this limitation we also generate _movable_ types that are a `struct` version of the interop struct (same struct layout) that can be stored anywhere, but this is only need when strictly necessary. The _movable_ type should only be used for storing the value, and converted to the interop struct to perform any other operation.

# Godot.NativeInterop.Marshallers

This directory contains the runtime marshallers. These marshallers are used to convert between the managed and unmanaged blittable types so they cross between managed and native code.

These types are part of the low-level API that is only meant to be used for [marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/type-marshalling) and interoperability between the C# managed types and the native Godot types so they can be used with the GDExtension APIs.

This repository [disables .NET runtime marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/disabled-marshalling) so every C# managed type must be converted to an unmanaged blittable type so it can cross between managed and native code. Most primitive .NET types (including `bool`) are blittable when runtime marshalling is disabled, some C# structs in `Variant` and the [native interop structs](../InteropStructs) are also blittable.

The runtime marshallers in this directory are used by the generated bindings to convert some types, the pattern is:

```csharp
NativeGodotString* nativeString = null;
try
{
	nativeString = StringMarshaller.ConvertToUnmanaged("Hello world");
	NativeMethodThatTakesString(nativeString);
}
finally
{
	StringMarshaller.Free(nativeString);
}
```

The `ConvertToUnmanaged` method may allocate memory to store the value that is referenced by the returned pointer. We need to ensure that this memory is _fixed_ so it can cross to native code. The `Free` method will free any memory that may have been allocated, the `finally` ensures this only happens after the native code has finished using the _fixed_ memory.

The runtime marshallers are static types which means they are stateless. It is currently not possible to implement stateful marshallers. They are also shape-based, they don't implement an interface, which means the shape must be followed carefully. Here's an example of a marshaller to describe the shape:

```csharp
internal unsafe static class StringMarshaller
{
	// This method converts the managed value of type string to the unmanaged blittable type NativeGodotString.
	// The destination pointer should already be allocated so there's no need to allocate native memory to
	// store the NativeGodotString instance.
	public static void WriteUnmanaged(NativeGodotString* destination, string value);

	// This method converts the managed value of type string to the unmanaged blittable type NativeGodotString.
	// This time an allocated pointer is not provided, so we must allocate native memory to store the NativeGodotString
	// instance and return a pointer that references the created instance which will be freed in the Free method.
	public static NativeGodotString* ConvertToUnmanaged(string value);

	// This method dereferences the provided pointer which should contain an instance of NativeGodotString
	// and convert it to the managed type string to return it. Nothing should be allocated or released here.
	public static string ConvertFromUnmanaged(NativeGodotString* value);

	// This method releases the native memory that was allocated in the ConvertToUnmanaged method.
	// If the pointer is null or the ConvertToUnmanaged method did not allocate memory, it does nothing.
	public static void Free(NativeGodotString* value);

	// This method converts the managed value of type string to the unmanaged blittable type NativeGodotVariant.
	// This time an allocated pointer is not provided, so we must allocate native memory to store the NativeGodotVariant
	// instance and return a pointer that references the created instance which will be freed in the FreeVariant method.
	public static NativeGodotVariant* ConvertToVariant(string value);

	// This method dereferences the provided pointer which should contain an instance of NativeGodotString
	// and convert it to the managed type string to return it. Nothing should be allocated or released here.
	public static string ConvertFromVariant(NativeGodotVariant* value);

	// This method releases the native memory that was allocated in the ConvertToVariant method.
	// If the pointer is null or the ConvertToVariant method did not allocate memory, it does nothing.
	public static void FreeVariant(NativeGodotVariant* value);
}
```

For types where a simpler conversion is possible, a [marshaller writer](../../../Godot.BindingsGenerator/Marshalling) is preferred.

# Godot.Bindings

.NET bindings for Godot using the GDExtension APIs generated using the [Godot.BindingsGenerator](../Godot.BindingsGenerator) CLI tool.

## Usage

The bindings can be used to create a GDExtension that Godot can load. In order to do this you need to create a .NET native library, since Godot expects to load a native library with an exposed C compatible method specified in a `.gdextension` file.

For more information about creating a .NET native library see the official [.NET NativeAOT sample](https://github.com/dotnet/samples/tree/main/core/nativeaot/NativeLibrary) and the [Publish AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot) documentation. For more information about creating GDExtension library see the [godot-cpp README](https://github.com/godotengine/godot-cpp/blob/master/README.md).

Using NativeAOT, a C compatible method can exposed using the `[UnmanagedCallersOnly]` attribute on a method. The method must be static and match the signature expected by Godot (defined as `GDExtensionInitializationFunction` in `gdextension_interface.h`).

The name of the method must match the name specified in the `.gdextension` file, it doesn't matter where it is defined.

```csharp
[UnmanagedCallersOnly(EntryPoint = "my_library_init")]
private static bool MyLibraryInit(nint getProcAddress, nint library, nint initialization)
{
	GodotBridge.Initialize(getProcAddress, library, initialization, config =>
	{
		config.SetMinimumLibraryInitializationLevel(InitializationLevel.Scene);
		config.RegisterInitializer(InitializeMyLibrary),
		config.RegisterTerminator(DeinitializeMyLibrary),
	});

	return true;
}
```

> [!WARNING]
> In this example we are using `bool` in the method signature. In C#, the `bool` type is not blittable unless you [disable runtime marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/disabled-marshalling). Otherwise use `byte`.

In the previous example we define a method called `MyLibraryInit` (that is exposed as a C compatible method with the name `my_library_init`). This method will be called by Godot on initializing the GDExtension, we use the `GodotBridge.Initialize` API to setup the GDExtension interface APIs that can be used by the bindings to interop with Godot.

The `GodotBridge.Initialize` allows us to configure the initialization by setting up the minimum initialization level, and callbacks that will be called when the GDExtension is initialized and when it's deinitialized.

The C# project can be compiled using the following command:

```bash
dotnet publish /p:NativeLib=Static /p:SelfContained=true -r [RID] -c [Configuration]
```

The above command will create a static native library for the specified platform according to the `[RID]` runtime identifier (e.g.: win-x64, linux-x64, osx-x64) in the `./bin/{Configuration}/{TargetFramework}/{RID}/publish` directory.

The last step is to create a `.gdextension` file that contains the path to the native library and the name of the initialization method (as specified in the `EntryPoint` property of the `[UnmanagedCallersOnly]` attribute) that acts as the entry point of your GDExtension.

```ini
[configuration]

entry_symbol = "my_library_init"

[libraries]

macos.debug = "bin/libgdexample.macos.debug.dylib"
macos.release = "bin/libgdexample.macos.release.dylib"
windows.debug.x86_64 = "bin/libgdexample.windows.debug.x86_64.dll"
windows.release.x86_64 = "bin/libgdexample.windows.release.x86_64.dll"
linux.debug.x86_64 = "bin/libgdexample.linux.debug.x86_64.so"
linux.release.x86_64 = "bin/libgdexample.linux.release.x86_64.so"
```

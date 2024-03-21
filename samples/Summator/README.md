# Summator

This directory contains an example GDExtension that uses the C# GDExtension bindings.

The example was ported from the [Summator](https://github.com/paddy-exe/GDExtensionSummator) example originally written in C++ with [godot-cpp](https://github.com/godotengine/godot-cpp).

Directory structure:

- **Extension**: This directory contains the GDExtension source code.
- **Game**: This directory contains the Godot project that consumes the GDExtension.

## Usage

Build the .NET project in the _Extension_ directory and copy the output to the `Game/lib` directory:

```bash
dotnet publish Extension -r [RID] -o Game/lib
```

Replace `[RID]` with the runtime identifier of the platform you want to build for (e.g.: win-x64, linux-x64, osx-x64).

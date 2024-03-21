# Godot.BindingsGenerator

CLI tool to generate .NET bindings from a [`extension_api.json`](../../gdextension/extension_api.json) API dump and a [`gdextension_interface.h`](../../gdextension/gdextension_interface.h) header file.

Both files are included in this repository and should be regularly updated but can also be retrieved from a Godot engine build. See the [gdextension](../../gdextension) directory for more details.

The generator uses [Godot.BindingsGenerator.ApiDump](../Godot.BindingsGenerator.ApiDump) to deserialize the API dump and [ClangSharp](https://github.com/dotnet/clangsharp) to generate the GDExtension interface bindings from the header file.

## Usage

```bash
GodotBindingsGenerator --extension-api [PATH_TO_EXTENSION_API_JSON] --extension-interface [PATH_TO_GDEXTENSION_INTERFACE_HEADER] [PATH_TO_OUTPUT_DIRECTORY]
```

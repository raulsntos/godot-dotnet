# Godot.BindingsGenerator.ApiDump

Serialization of the [`extension_api.json`](../../gdextension/extension_api.json) file that contains the API dump. The data in the API dump can be used to generate bindings that can be implemented using the GDExtension APIs.

## Usage

The `GodotApi` type contains static methods to deserialize the `extension_api.json` file.

```csharp
using var stream = File.OpenRead("./extension_api.json");
var api = GodotApi.Deserialize(stream);
```

The deserialized data can be used to implement a bindings generator in C#. The bindings can be implemented by invoking calls to the GDExtension APIs.

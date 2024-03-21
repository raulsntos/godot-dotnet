namespace Godot.BindingsGenerator.ApiDump.Serialization;

[JsonSerializable(typeof(GodotApi))]
[JsonSourceGenerationOptions(
    // Fail loudly if the serializer finds a property in the JSON file that's not defined in the C# classes,
    // this can indicate that we are missing important information or that something changed in the engine
    // and we didn't update the C# classes to match. It's preferable to make this as noticeable as possible.
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,

    Converters =
    [
        typeof(ConstantInfoConverter),
    ]
)]
internal sealed partial class GodotApiJsonSerializerContext : JsonSerializerContext { }

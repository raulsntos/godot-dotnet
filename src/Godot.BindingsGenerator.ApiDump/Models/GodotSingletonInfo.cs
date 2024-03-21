namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot singleton.
/// Contains the name used to access the singleton and the type of the singleton.
/// The type is defined in <see cref="GodotApi.Classes"/>.
/// </summary>
public class GodotSingletonInfo
{
    /// <summary>
    /// Name of the singleton.
    /// This is the name that the singleton can be retrieved with.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the singleton type.
    /// This is the name of the type that the singleton's instance will use.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

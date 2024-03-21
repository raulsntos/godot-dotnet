namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the size information for a Godot class.
/// </summary>
public class GodotClassSize
{
    /// <summary>
    /// Name of the Godot class.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Size of the Godot class in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public required int Size { get; set; }
}

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a collection of size information for Godot classes
/// for a specific build configuration.
/// </summary>
public class GodotClassSizes
{
    /// <summary>
    /// The build configuration that this size information applies to.
    /// </summary>
    [JsonPropertyName("build_configuration")]
    public required GodotBuildConfiguration BuildConfiguration { get; set; }

    /// <summary>
    /// Collection of size information for Godot classes.
    /// </summary>
    [JsonPropertyName("sizes")]
    public required GodotClassSize[] Sizes { get; set; }
}

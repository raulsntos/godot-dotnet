namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a collection of member offset information for Godot classes
/// for a specific build configuration.
/// </summary>
public class GodotClassMemberOffsetsGroup
{
    /// <summary>
    /// The build configuration that this member offset information applies to.
    /// </summary>
    [JsonPropertyName("build_configuration")]
    public required GodotBuildConfiguration BuildConfiguration { get; set; }

    /// <summary>
    /// Collection of member offset information for Godot classes.
    /// </summary>
    [JsonPropertyName("classes")]
    public required GodotClassMemberOffsets[] Classes { get; set; }
}

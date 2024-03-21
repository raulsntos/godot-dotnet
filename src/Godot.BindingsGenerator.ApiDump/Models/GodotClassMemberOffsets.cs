namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the offsets for all the members of a specific Godot class.
/// </summary>
public class GodotClassMemberOffsets
{
    /// <summary>
    /// Name of the Godot class that contains the members.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Collection of member offsets for the Godot class.
    /// </summary>
    [JsonPropertyName("members")]
    public required GodotClassMemberOffset[] Members { get; set; }
}

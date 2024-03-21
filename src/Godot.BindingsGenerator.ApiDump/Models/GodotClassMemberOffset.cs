namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the size information for a member in a Godot class.
/// </summary>
public class GodotClassMemberOffset
{
    /// <summary>
    /// Name of the Godot class member.
    /// </summary>
    [JsonPropertyName("member")]
    public required string Member { get; set; }

    /// <summary>
    /// Byte offset for this member in the struct layout.
    /// </summary>
    [JsonPropertyName("offset")]
    public required int Offset { get; set; }

    /// <summary>
    /// Name of the type of the member (using the metadata name).
    /// </summary>
    [JsonPropertyName("meta")]
    public required string Meta { get; set; }
}

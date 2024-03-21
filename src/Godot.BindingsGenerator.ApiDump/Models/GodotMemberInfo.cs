namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the member of a Godot built-in class.
/// </summary>
public class GodotMemberInfo
{
    /// <summary>
    /// Name of the member.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the type of the member.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{Type} {Name}";
}

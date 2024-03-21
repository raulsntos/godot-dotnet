namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot enum.
/// </summary>
public class GodotEnumInfo
{
    /// <summary>
    /// Name of the enum type.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Indicates if the enum is a bitfield or flags enum.
    /// </summary>
    [JsonPropertyName("is_bitfield")]
    public bool IsBitField { get; set; }

    /// <summary>
    /// Collection of enum members contained in the enum.
    /// </summary>
    [JsonPropertyName("values")]
    public required GodotEnumValueInfo[] Values { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"enum::{Name}";
}

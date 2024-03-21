namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot constant.
/// </summary>
public class GodotConstantInfo
{
    /// <summary>
    /// Name of the constant.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the type of this constant's value.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Value of the constant as a string. The value needs to be parsed and translated to proper C# syntax.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"const {Type} {Name} = {Value}";
}

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the argument of a Godot method.
/// </summary>
public class GodotArgumentInfo
{
    /// <summary>
    /// Name of the method's argument.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the type of the argument.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Metadata for the argument type.
    /// When <see cref="Type"/> is 'int' or 'float' it can specify the size
    /// (e.g.: 'int32', 'uint32', 'int64', 'uint64', 'float', 'double').
    /// </summary>
    [JsonPropertyName("meta")]
    public string? Meta { get; set; }

    /// <summary>
    /// If the argument is optional, determines the default value to use when the
    /// argument is not specified by the caller.
    /// </summary>
    [JsonPropertyName("default_value")]
    public string? DefaultValue { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"{Type} {Name}";
}

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the return of a Godot method.
/// </summary>
public class GodotReturnValueInfo
{
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
}

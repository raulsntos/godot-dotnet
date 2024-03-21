namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot signal for an engine class.
/// </summary>
public class GodotSignalInfo
{
    /// <summary>
    /// Name of the method.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Collection of argument information for the method.
    /// </summary>
    [JsonPropertyName("arguments")]
    public GodotArgumentInfo[] Arguments { get; set; } = [];

    /// <inheritdoc/>
    public override string ToString() =>
        $"signal {Name}({string.Join(", ", (object[])Arguments)})";
}

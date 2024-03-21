namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot operator.
/// </summary>
public class GodotOperatorInfo
{
    /// <summary>
    /// Name of the operator.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the type in the right-hand side of the operator.
    /// For unary operators this is <see langword="null"/>.
    /// </summary>
    [JsonPropertyName("right_type")]
    public string? RightType { get; set; }

    /// <summary>
    /// Name of the type returned by the operator method.
    /// </summary>
    [JsonPropertyName("return_type")]
    public required string ReturnType { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ReturnType} operator {Name}{(RightType is not null ? $"({RightType})" : "")}";
}

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot method for a built-in class.
/// </summary>
public class GodotBuiltInMethodInfo
{
    /// <summary>
    /// Name of the method.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the return type for the method.
    /// </summary>
    [JsonPropertyName("return_type")]
    public string? ReturnType { get; set; }

    /// <summary>
    /// Indicates if the method has a variant number of arguments, this is equivalent
    /// to C#'s <c>params</c> keyword.
    /// </summary>
    [JsonPropertyName("is_vararg")]
    public bool IsVararg { get; set; }

    /// <summary>
    /// Indicates if the method is constant or read-only and doesn't mutate the instance.
    /// </summary>
    [JsonPropertyName("is_const")]
    public bool IsConst { get; set; }

    /// <summary>
    /// Indicates if the method is static and can be called without an instance.
    /// </summary>
    [JsonPropertyName("is_static")]
    public bool IsStatic { get; set; }

    /// <summary>
    /// Method hash that must be used to invoke it.
    /// The hash is computed from the method signature ensuring that breaking changes
    /// result in different hashes. As long as the same hash is used and a compatibility
    /// method is provided, Godot should still be able to find the method.
    /// </summary>
    [JsonPropertyName("hash")]
    public required uint Hash { get; set; }

    /// <summary>
    /// Collection of argument information for the method.
    /// </summary>
    [JsonPropertyName("arguments")]
    public GodotArgumentInfo[] Arguments { get; set; } = [];

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ReturnType ?? "void"} {Name}({string.Join(", ", (object[])Arguments)})";
}

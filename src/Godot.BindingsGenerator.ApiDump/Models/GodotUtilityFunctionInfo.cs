namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot utility function.
/// </summary>
public class GodotUtilityFunctionInfo
{
    /// <summary>
    /// Name of the function.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the return type for the function.
    /// </summary>
    [JsonPropertyName("return_type")]
    public string? ReturnType { get; set; }

    /// <summary>
    /// The category that this utility function belongs to.
    /// Utility functions are grouped by category ('math', 'random', 'general').
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the method has a variant number of arguments, this is equivalent
    /// to C#'s <c>params</c> keyword.
    /// </summary>
    [JsonPropertyName("is_vararg")]
    public bool IsVararg { get; set; }

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
}

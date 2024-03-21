using System.Diagnostics.CodeAnalysis;

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot method for an engine class.
/// </summary>
public class GodotMethodInfo
{
    /// <summary>
    /// Name of the method.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Return information for the method.
    /// </summary>
    [JsonPropertyName("return_value")]
    public GodotReturnValueInfo? ReturnValue { get; set; }

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
    /// Indicates if the method is virtual and can be overridden.
    /// </summary>
    [JsonPropertyName("is_virtual")]
    [MemberNotNullWhen(false, nameof(Hash))]
    public bool IsVirtual { get; set; }

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
    public uint? Hash { get; set; }

    /// <summary>
    /// Collection of hashes of compatibility methods provided for this method.
    /// </summary>
    [JsonPropertyName("hash_compatibility")]
    public uint[]? HashCompatibility { get; set; }

    /// <summary>
    /// Collection of argument information for the method.
    /// </summary>
    [JsonPropertyName("arguments")]
    public GodotArgumentInfo[] Arguments { get; set; } = [];

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ReturnValue?.Type ?? "void"} {Name}({string.Join(", ", (object[])Arguments)})";
}

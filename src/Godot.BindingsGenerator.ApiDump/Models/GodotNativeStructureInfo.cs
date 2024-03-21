namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot native structure.
/// </summary>
public class GodotNativeStructureInfo
{
    /// <summary>
    /// Name of the native structure.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Contains the definition of the native structure using a C-like format.
    /// The format contains the field definitions separated by semi-colons (<c>;</c>).
    /// The field format is: <c>[TYPE] [NAME]( = [DEFAULT_VALUE])</c>
    /// The name of the type refers to a C type name (e.g.: 'uint8_t').
    /// The default value is optional.
    /// </summary>
    [JsonPropertyName("format")]
    public required string Format { get; set; }
}

using Godot.BindingsGenerator.ApiDump.Serialization;

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the type of an API.
/// </summary>
[JsonConverter(typeof(StringEnumConverter<GodotApiType>))]
public enum GodotApiType
{
    /// <summary>
    /// API defined in core types.
    /// </summary>
    [JsonPropertyName("core")]
    Core = 1,

    /// <summary>
    /// API defined in editor types that aren't available in exported projects.
    /// </summary>
    [JsonPropertyName("editor")]
    Editor,

    /// <summary>
    /// API defined in extension types.
    /// </summary>
    [JsonPropertyName("extension")]
    Extension,

    /// <summary>
    /// API defined in extension editor types that aren't available in exported projects.
    /// </summary>
    [JsonPropertyName("editor_extension")]
    EditorExtension,
}

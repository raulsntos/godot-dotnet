namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the type of an API.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GodotApiType>))]
public enum GodotApiType
{
    /// <summary>
    /// API defined in core types.
    /// </summary>
    [JsonStringEnumMemberName("core")]
    Core = 1,

    /// <summary>
    /// API defined in editor types that aren't available in exported projects.
    /// </summary>
    [JsonStringEnumMemberName("editor")]
    Editor,

    /// <summary>
    /// API defined in extension types.
    /// </summary>
    [JsonStringEnumMemberName("extension")]
    Extension,

    /// <summary>
    /// API defined in extension editor types that aren't available in exported projects.
    /// </summary>
    [JsonStringEnumMemberName("editor_extension")]
    EditorExtension,
}

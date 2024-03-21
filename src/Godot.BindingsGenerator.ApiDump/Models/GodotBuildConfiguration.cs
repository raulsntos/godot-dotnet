using System.Diagnostics.CodeAnalysis;
using Godot.BindingsGenerator.ApiDump.Serialization;

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// The configuration used to build the Godot engine used to dump the extension API.
/// </summary>
[JsonConverter(typeof(StringEnumConverter<GodotBuildConfiguration>))]
public enum GodotBuildConfiguration
{
    /// <summary>
    /// Godot was built for a 32-bits architecture, and with 32-bit floating-point precision.
    /// </summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The name of the enum value must match the string used by the API JSON dump.")]
    [JsonPropertyName("float_32")]
    Float32 = 1,

    /// <summary>
    /// Godot was built for a 64-bits architecture, and with 32-bit floating-point precision.
    /// </summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The name of the enum value must match the string used by the API JSON dump.")]
    [JsonPropertyName("float_64")]
    Float64,

    /// <summary>
    /// Godot was built for a 32-bits architecture, and with 64-bit floating-point precision.
    /// </summary>
    [JsonPropertyName("double_32")]
    Double32,

    /// <summary>
    /// Godot was built for a 64-bits architecture, and with 64-bit floating-point precision.
    /// </summary>
    [JsonPropertyName("double_64")]
    Double64,
}

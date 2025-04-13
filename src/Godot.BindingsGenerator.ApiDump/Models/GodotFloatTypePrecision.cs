using System.Diagnostics.CodeAnalysis;

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the precision used by the floating point type.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GodotFloatTypePrecision>))]
public enum GodotFloatTypePrecision
{
    /// <summary>
    /// Floating point type is a 32-bit <see cref="float"/>.
    /// </summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The name of the enum value must match the string used by the API JSON dump.")]
    [JsonStringEnumMemberName("single")]
    Single,

    /// <summary>
    /// Floating point type is a 64-bit <see cref="double"/>.
    /// </summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The name of the enum value must match the string used by the API JSON dump.")]
    [JsonStringEnumMemberName("double")]
    Double,
}

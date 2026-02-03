using System;

namespace Godot.SourceGeneration;

/// <summary>
/// Describes a Godot property group specification.
/// </summary>
internal readonly record struct GodotPropertyGroupSpec : IEquatable<GodotPropertyGroupSpec>
{
    /// <summary>
    /// Name of the property's group.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Prefix of the properties that will be included in the group.
    /// </summary>
    public required string? Prefix { get; init; }
}

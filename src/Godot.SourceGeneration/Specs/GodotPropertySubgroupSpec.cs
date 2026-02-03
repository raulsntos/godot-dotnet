using System;

namespace Godot.SourceGeneration;

/// <summary>
/// Describes a Godot property subgroup specification.
/// </summary>
internal readonly record struct GodotPropertySubgroupSpec : IEquatable<GodotPropertySubgroupSpec>
{
    /// <summary>
    /// Name of the property's subgroup.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Prefix of the properties that will be included in the subgroup.
    /// </summary>
    public required string? Prefix { get; init; }
}

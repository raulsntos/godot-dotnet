using System;

namespace Godot.SourceGenerators;

/// <summary>
/// Describes a GDExtension assembly that may contain Godot classes.
/// </summary>
internal readonly record struct AssemblySpec : IEquatable<AssemblySpec>
{
    /// <summary>
    /// Assembly name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Godot classes registered in the assembly.
    /// </summary>
    public EquatableArray<GodotRegistrationSpec> Types { get; init; }

    /// <summary>
    /// Indicates whether the assembly disables the generation of the GDExtension entry-point.
    /// </summary>
    public bool DisableGodotEntryPointGeneration { get; init; }
}

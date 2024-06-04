using System;

namespace Godot;

/// <summary>
/// Registers the annotated enum within an extension class.
/// </summary>
[AttributeUsage(AttributeTargets.Enum, Inherited = false)]
public sealed class BindEnumAttribute : Attribute
{
    /// <summary>
    /// Specifies the name that will be used to register the enum.
    /// If unspecified it will use the name of the annotated enum.
    /// </summary>
    public string? Name { get; init; }
}

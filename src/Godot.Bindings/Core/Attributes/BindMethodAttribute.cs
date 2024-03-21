using System;

namespace Godot;

/// <summary>
/// Registers the annotated method within an extension class.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class BindMethodAttribute : Attribute
{
    /// <summary>
    /// Specifies the name that will be used to register the method.
    /// If unspecified it will use the name of the annotated method.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Determines whether the method is registered as a virtual method
    /// and if the method must be overridden in user scripts.
    /// </summary>
    public bool Virtual { get; init; }
}

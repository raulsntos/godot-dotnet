using System;

namespace Godot;

/// <summary>
/// Registers the annotated delegate as a signal within an extension class.
/// </summary>
[AttributeUsage(AttributeTargets.Delegate, Inherited = false)]
public sealed class SignalAttribute : Attribute
{
    /// <summary>
    /// Specifies the name that will be used to register the signal.
    /// If unspecified it will use the name of the annotated delegate.
    /// </summary>
    public string? Name { get; init; }
}

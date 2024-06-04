using System;

namespace Godot;

/// <summary>
/// Registers the annotated constant within an extension class.
/// It can also be used to annotate enum members to customize binding.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class BindConstantAttribute : Attribute
{
    /// <summary>
    /// Specifies the name that will be used to register the constant.
    /// If unspecified it will use the name of the annotated constant.
    /// </summary>
    public string? Name { get; init; }
}

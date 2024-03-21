using System.Collections.Generic;

namespace Godot.Bridge;

/// <summary>
/// Defines a method registered for a class.
/// </summary>
public sealed class MethodInfo
{
    /// <summary>
    /// Name of the method.
    /// </summary>
    public StringName Name { get; }

    /// <summary>
    /// Collection of parameter information for the method.
    /// </summary>
    public List<ParameterInfo> Parameters { get; } = [];

    /// <summary>
    /// Return information for the method or <see langword="null"/> if the
    /// method has no return parameter.
    /// </summary>
    public ReturnInfo? Return { get; init; }

    /// <summary>
    /// Indicates whether the method is static.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// The <see cref="MethodBindInvoker"/> that can invoke this method.
    /// </summary>
    public MethodBindInvoker Invoker { get; }

    /// <summary>
    /// Constructs a new <see cref="MethodInfo"/> with the specified name and an invoker.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    /// <param name="invoker">Invoker for the method.</param>
    public MethodInfo(StringName name, MethodBindInvoker invoker)
    {
        Name = name;
        Invoker = invoker;
    }
}

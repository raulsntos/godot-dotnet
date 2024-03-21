using System.Collections.Generic;

namespace Godot.Bridge;

/// <summary>
/// Defines a virtual method registered for a class.
/// A virtual method is a method that can be overridden in user scripts
/// but the extension class must call the script implementation using
/// <see cref="GodotObject.CallVirtualMethod(StringName)"/> or
/// <see cref="GodotObject.TryCallVirtualMethod(StringName)"/>.
/// </summary>
public partial class VirtualMethodInfo
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
    /// Constructs a new <see cref="VirtualMethodInfo"/> with the specified name.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    public VirtualMethodInfo(StringName name)
    {
        Name = name;
    }
}

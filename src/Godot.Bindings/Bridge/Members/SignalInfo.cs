using System.Collections.Generic;

namespace Godot.Bridge;

/// <summary>
/// Defines a signal registered for a class.
/// </summary>
public sealed class SignalInfo
{
    /// <summary>
    /// Name of the signal.
    /// </summary>
    public StringName Name { get; }

    /// <summary>
    /// Collection of parameter information for the signal delegate.
    /// </summary>
    public List<ParameterInfo> Parameters { get; } = [];

    /// <summary>
    /// Constructs a new <see cref="SignalInfo"/> with the specified name.
    /// </summary>
    /// <param name="name"></param>
    public SignalInfo(StringName name)
    {
        Name = name;
    }
}

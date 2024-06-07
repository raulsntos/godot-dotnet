using System;

namespace Godot;

/// <summary>
/// Registers a property group within an extension class.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public sealed class PropertyGroupAttribute : Attribute
{
    /// <summary>
    /// Specifies the name of the property group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies the prefix used by the properties that will be included in the group.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Constructs a <see cref="PropertyGroupAttribute"/>.
    /// </summary>
    public PropertyGroupAttribute(string name, string prefix = "")
    {
        Name = name;
        Prefix = prefix;
    }
}

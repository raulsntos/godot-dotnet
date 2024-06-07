using System;

namespace Godot;

/// <summary>
/// Registers a property subgroup within an extension class.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public sealed class PropertySubgroupAttribute : Attribute
{
    /// <summary>
    /// Specifies the name of the property subgroup.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies the prefix used by the properties that will be included in the subgroup.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Constructs a <see cref="PropertySubgroupAttribute"/>.
    /// </summary>
    public PropertySubgroupAttribute(string name, string prefix = "")
    {
        Name = name;
        Prefix = prefix;
    }
}

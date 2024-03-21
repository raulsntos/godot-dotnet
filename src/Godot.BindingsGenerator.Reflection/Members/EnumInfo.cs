using System.Collections.Generic;
using System.Linq;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# enum type.
/// </summary>
public class EnumInfo : TypeInfo
{
    /// <summary>
    /// Whether the enumeration type is a flags enum.
    /// </summary>
    public bool HasFlagsAttribute { get; set; }

    /// <summary>
    /// The underlying type of the current enumeration type, if specified.
    /// An enum without a specified underlying type uses <see langword="int"/>.
    /// </summary>
    public TypeInfo? UnderlyingType { get; set; }

    /// <summary>
    /// Collection of the values of this enumeration type.
    /// </summary>
    public List<(string Name, long Value)> Values { get; set; } = [];

    /// <summary>
    /// Constructs a new <see cref="EnumInfo"/>.
    /// </summary>
    /// <param name="name">Name of the enum.</param>
    /// <param name="namespace">Namespace that contains the enum.</param>
    public EnumInfo(string name, string? @namespace = null) : base(name, @namespace)
    {
        TypeAttributes = TypeAttributes.ValueType;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Enum: {Name} {{ {string.Join(", ", Values.Select(value => value.Name))} }}";
}

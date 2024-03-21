using System.Collections.Generic;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# member.
/// </summary>
public class MemberInfo
{
    /// <summary>
    /// Member name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates whether the member uses the <c>new</c> modifier to shadow an inherited member.
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// List of attributes applied to this member.
    /// </summary>
    public List<string> Attributes { get; set; } = [];

    /// <summary>
    /// Constructs a new <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="name">Name of the member.</param>
    public MemberInfo(string name)
    {
        Name = name;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Member: {Name}";
}

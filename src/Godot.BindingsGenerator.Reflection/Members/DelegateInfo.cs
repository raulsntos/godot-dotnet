using System.Collections.Generic;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# delegate type.
/// </summary>
public class DelegateInfo : TypeInfo
{
    /// <summary>
    /// The delegate's parameter list.
    /// </summary>
    public List<ParameterInfo> Parameters { get; set; } = [];

    /// <summary>
    /// The delegate's return parameter, or null if it returns void.
    /// </summary>
    /// <seealso cref="ReturnType"/>
    public ParameterInfo? ReturnParameter { get; set; }

    /// <summary>
    /// The delegate's return type.
    /// </summary>
    /// <seealso cref="ReturnParameter"/>
    public TypeInfo? ReturnType => ReturnParameter?.Type;

    /// <summary>
    /// Constructs a new <see cref="DelegateInfo"/>.
    /// </summary>
    /// <param name="name">Name of the delegate.</param>
    /// <param name="namespace">Namespace that contains the delegate.</param>
    public DelegateInfo(string name, string? @namespace = null) : base(name, @namespace) { }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Delegate: {ReturnType?.FullName ?? "void"} {Name}({string.Join(", ", Parameters)})";
}

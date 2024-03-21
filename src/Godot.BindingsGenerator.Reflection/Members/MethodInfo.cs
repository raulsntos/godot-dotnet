namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# method.
/// </summary>
public class MethodInfo : MethodBase
{
    /// <summary>
    /// The method's return parameter, or null if it returns void.
    /// </summary>
    /// <seealso cref="ReturnType"/>
    public ParameterInfo? ReturnParameter { get; set; }

    /// <summary>
    /// The method's return type.
    /// </summary>
    /// <seealso cref="ReturnParameter"/>
    public TypeInfo? ReturnType => ReturnParameter?.Type;

    /// <summary>
    /// Indicates whether the method is read-only.
    /// Currently this is only supported for methods or property accessors
    /// contained in struct types.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Constructs a new <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    public MethodInfo(string name) : base(name) { }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Method: {ReturnType?.FullName ?? "void"} {Name}({string.Join(", ", Parameters)})";
}

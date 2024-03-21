namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# field member.
/// </summary>
public class FieldInfo : VisibleMemberInfo
{
    /// <summary>
    /// Field's type.
    /// </summary>
    public TypeInfo Type { get; set; }

    /// <summary>
    /// Indicates whether the field is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Indicates whether the field is read-only.
    /// </summary>
    public bool IsInitOnly { get; set; }

    /// <summary>
    /// Indicates whether the field is a constant.
    /// The value is written at constant time and cannot be changed.
    /// </summary>
    public bool IsLiteral { get; set; }

    /// <summary>
    /// The default value for this field.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Indicates whether the inline initialization requires unsafe code.
    /// </summary>
    public bool RequiresUnsafeCode { get; set; }

    /// <summary>
    /// Constructs a new <see cref="FieldInfo"/>.
    /// </summary>
    /// <param name="name">Name of the field.</param>
    /// <param name="type">Type of the field.</param>
    public FieldInfo(string name, TypeInfo type) : base(name)
    {
        Type = type;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Field: {Type.FullName} {Name}";
}

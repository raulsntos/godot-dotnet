using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# property member.
/// </summary>
public class PropertyInfo : VisibleMemberInfo
{
    /// <summary>
    /// Property contract attributes.
    /// </summary>
    public ContractAttributes ContractAttributes { get; set; }

    /// <summary>
    /// Indicates whether the property is sealed (cannot be overridden).
    /// </summary>
    /// <seealso cref="IsVirtual"/>
    /// <seealso cref="IsAbstract"/>
    public bool IsFinal => ContractAttributes is ContractAttributes.Final;

    /// <summary>
    /// Indicates whether the property is virtual (can be overridden).
    /// </summary>
    /// <seealso cref="IsFinal"/>
    /// <seealso cref="IsAbstract"/>
    public bool IsVirtual => ContractAttributes is ContractAttributes.Virtual;

    /// <summary>
    /// Indicates whether the property is abstract (must be overridden).
    /// </summary>
    /// <seealso cref="IsFinal"/>
    /// <seealso cref="IsVirtual"/>
    public bool IsAbstract => ContractAttributes is ContractAttributes.Abstract;

    /// <summary>
    /// Indicates whether the property is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Property's type.
    /// </summary>
    public TypeInfo Type { get; set; }

    /// <summary>
    /// Indicates whether the property is read-only.
    /// Currently this is only supported for properties contained in struct types.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Indicates whether the property has a getter.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Getter))]
    public bool CanRead => Getter is not null;

    /// <summary>
    /// Indicates whether the property has a setter.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Setter))]
    public bool CanWrite => Setter is not null;

    /// <summary>
    /// Property's getter.
    /// </summary>
    public MethodInfo? Getter { get; set; }

    /// <summary>
    /// Property's setter.
    /// </summary>
    public MethodInfo? Setter { get; set; }

    /// <summary>
    /// Constructs a new <see cref="PropertyInfo"/>.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="type">Type of the property.</param>
    public PropertyInfo(string name, TypeInfo type) : base(name)
    {
        Type = type;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"Property: {Type.FullName} {Name}");
        if (CanRead && CanWrite)
        {
            sb.Append(" { get; set; }");
        }
        if (CanRead && !CanWrite)
        {
            sb.Append(" { get; }");
        }
        if (!CanRead && CanWrite)
        {
            sb.Append(" { set; }");
        }
        return sb.ToString();
    }
}

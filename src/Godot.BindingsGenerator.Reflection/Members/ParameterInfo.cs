using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# method's parameter.
/// </summary>
public class ParameterInfo : MemberInfo
{
    /// <summary>
    /// Indicates whether the parameter is a ref or a ref readonly.
    /// </summary>
    public RefKind RefKind { get; set; }

    /// <summary>
    /// Indicates whether the parameter is an <c>in</c> param.
    /// </summary>
    public bool IsIn => RefKind is RefKind.In;

    /// <summary>
    /// Indicates whether the parameter is an <c>out</c> param.
    /// </summary>
    public bool IsOut => RefKind is RefKind.Out;

    /// <summary>
    /// Indicates whether the parameter is a <c>ref</c> param.
    /// </summary>
    public bool IsRef => RefKind is RefKind.Ref or RefKind.RefReadOnly;

    /// <summary>
    /// Indicates whether the parameter is a <c>ref readonly</c> param.
    /// </summary>
    public bool IsRefReadOnly => RefKind is RefKind.RefReadOnly;

    /// <summary>
    /// Indicates the scoped kind of the parameter.
    /// </summary>
    public ScopedKind ScopedKind { get; set; }

    /// <summary>
    /// Indicates whether the parameter is a <c>scoped</c> param.
    /// </summary>
    public bool IsScoped => ScopedKind is ScopedKind.ScopedRef or ScopedKind.ScopedValue;

    /// <summary>
    /// Parameter's type.
    /// </summary>
    public TypeInfo Type { get; set; }

    /// <summary>
    /// Indicates whether the parameter allows a variable amount of arguments.
    /// This parameter contains the <c>params</c> keyword.
    /// </summary>
    public bool IsParams { get; set; }

    /// <summary>
    /// Default value serialized as a string if it has one, otherwise null.
    /// </summary>
    /// <seealso cref="IsOptional"/>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Indicates whether the parameter is optional.
    /// When the parameter is optional, it must have a non-null <see cref="DefaultValue"/>.
    /// </summary>
    /// <seealso cref="DefaultValue"/>
    [MemberNotNullWhen(true, nameof(DefaultValue))]
    public bool IsOptional => DefaultValue is not null;

    /// <summary>
    /// Constructs a new <see cref="ParameterInfo"/>.
    /// </summary>
    /// <param name="name">Name of the parameter.</param>
    /// <param name="type">Type of the parameter.</param>
    public ParameterInfo(string name, TypeInfo type) : base(name)
    {
        Type = type;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"{Type.FullName} {Name}");
        if (IsOptional)
        {
            sb.Append(CultureInfo.InvariantCulture, $" = {DefaultValue}");
        }
        return sb.ToString();
    }
}

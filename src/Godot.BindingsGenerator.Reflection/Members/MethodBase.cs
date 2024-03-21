using System.Collections.Generic;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Base to define C# methods.
/// </summary>
public abstract class MethodBase : VisibleMemberInfo
{
    /// <summary>
    /// Method contract attributes.
    /// </summary>
    public ContractAttributes ContractAttributes { get; set; }

    /// <summary>
    /// Indicates whether the method is sealed (cannot be overridden).
    /// </summary>
    /// <seealso cref="IsVirtual"/>
    /// <seealso cref="IsAbstract"/>
    public bool IsFinal => ContractAttributes is ContractAttributes.Final;

    /// <summary>
    /// Indicates whether the method is virtual (can be overridden).
    /// </summary>
    /// <seealso cref="IsFinal"/>
    /// <seealso cref="IsAbstract"/>
    public bool IsVirtual => ContractAttributes is ContractAttributes.Virtual;

    /// <summary>
    /// Indicates whether the method is abstract (must be overridden).
    /// </summary>
    /// <seealso cref="IsFinal"/>
    /// <seealso cref="IsVirtual"/>
    public bool IsAbstract => ContractAttributes is ContractAttributes.Abstract;

    /// <summary>
    /// Indicates whether the method is an override.
    /// </summary>
    public bool IsOverridden { get; set; }

    /// <summary>
    /// Indicates whether the method is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Indicates whether the method is a partial declaration.
    /// </summary>
    public bool IsPartial { get; set; }

    /// <summary>
    /// Indicates whether the method is a constructor.
    /// </summary>
    public bool IsConstructor => this is ConstructorInfo;

    /// <summary>
    /// The method's parameter list.
    /// </summary>
    public List<ParameterInfo> Parameters { get; set; } = [];

    /// <summary>
    /// The method's type parameter list.
    /// </summary>
    public List<TypeParameterInfo> TypeParameters { get; set; } = [];

    /// <summary>
    /// Indicates whether the method is generic (i.e.: contains type parameters).
    /// </summary>
    public bool IsGenericMethod => TypeParameters.Count > 0;

    /// <summary>
    /// The method's body.
    /// </summary>
    public MethodBody Body { get; set; } = MethodBody.Empty;

    /// <summary>
    /// Constructs a new <see cref="MethodBase"/>.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    internal MethodBase(string name) : base(name) { }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Method: {Name}({string.Join(", ", Parameters)})";
}

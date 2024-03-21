using System.Collections.Generic;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# type parameter.
/// </summary>
public class TypeParameterInfo : TypeInfo
{
    /// <summary>
    /// The types that are directly specified as constraints on the type parameter.
    /// </summary>
    public List<TypeInfo> ConstraintTypes { get; set; } = [];

    /// <summary>
    /// The special constraint that is specified on the type parameter.
    /// </summary>
    public TypeParameterConstraintKind ConstraintKind { get; set; }

    /// <summary>
    /// Whether the parameterless constructor constraint (<c>new()</c>) is specified for the type parameter.
    /// </summary>
    public bool HasConstructorConstraint => ConstraintKind is TypeParameterConstraintKind.Constructor;

    /// <summary>
    /// Whether the notnull constraint (<c>notnull</c>) is specified for the type parameter.
    /// </summary>
    public bool HasNotNullConstraint => ConstraintKind is TypeParameterConstraintKind.NotNull;

    /// <summary>
    /// Whether the reference type constraint (<c>class</c>) is specified for the type parameter.
    /// </summary>
    public bool HasReferenceTypeConstraint => ConstraintKind is TypeParameterConstraintKind.ReferenceType;

    /// <summary>
    /// Whether the value type constraint (<c>struct</c>) is specified for the type parameter.
    /// </summary>
    public bool HasValueTypeTypeConstraint => ConstraintKind is TypeParameterConstraintKind.ValueType;

    /// <summary>
    /// Whether the value type constraint (<c>unmanaged</c>) is specified for the type parameter.
    /// </summary>
    public bool HasUnmanagedTypeConstraint => ConstraintKind is TypeParameterConstraintKind.Unmanaged;

    /// <summary>
    /// Constructs a new <see cref="TypeParameterInfo"/>.
    /// </summary>
    /// <param name="name">Name of the type parameter.</param>
    public TypeParameterInfo(string name) : base(name) { }
}

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Special constraint kinds that can be specified for a type parameter.
/// </summary>
public enum TypeParameterConstraintKind
{
    /// <summary>
    /// The type parameter has no special constraint.
    /// </summary>
    None,

    /// <summary>
    /// The type parameter has the parameterless constructor constraint (<c>new()</c>).
    /// </summary>
    Constructor,

    /// <summary>
    /// The type parameter has the notnull constraint (<c>notnull</c>).
    /// </summary>
    NotNull,

    /// <summary>
    /// The type parameter has the reference type constraint (<c>class</c>).
    /// </summary>
    ReferenceType,

    /// <summary>
    /// The type parameter has the value type constraint (<c>struct</c>).
    /// </summary>
    ValueType,

    /// <summary>
    /// The type parameter has the value type constraint (<c>unmanaged</c>).
    /// </summary>
    Unmanaged,
}

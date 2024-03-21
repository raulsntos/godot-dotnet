namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Attributes that define a type.
/// </summary>
public enum TypeAttributes
{
    /// <summary>
    /// Type not defined.
    /// This is an invalid value, every type must de defined.
    /// </summary>
    None,

    /// <summary>
    /// The type is a reference type (e.g.: class).
    /// </summary>
    ReferenceType,

    /// <summary>
    /// The type is a value type (e.g.: struct).
    /// </summary>
    ValueType,

    /// <summary>
    /// The type is a ref struct.
    /// </summary>
    ByRefLikeType,
}

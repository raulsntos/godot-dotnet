namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Attributes that define the parameter ref kind.
/// </summary>
public enum RefKind
{
    /// <summary>
    /// The parameter is passed by reference if it's a reference type
    /// or by copy if it's a value type.
    /// </summary>
    None,

    /// <summary>
    /// The parameter is passed by reference as a read-only parameter.
    /// If a value type is passed this way and tries to be modified
    /// it will be copied.
    /// </summary>
    /// <remarks>
    /// Calling any method not marked as <c>readonly</c> counts as modifying
    /// the value type. That's why it's recommended to use <c>readonly struct</c>
    /// with <c>in</c> parameters.
    /// </remarks>
    In,

    /// <summary>
    /// The parameter is not passed but returned.
    /// </summary>
    Out,

    /// <summary>
    /// The parameter is passed by reference and will be returned
    /// modified since any modification done inside the method
    /// is done to the reference passed and not a copy.
    /// </summary>
    Ref,

    /// <summary>
    /// The parameter is passed by reference but does not allow
    /// modifying it.
    /// </summary>
    RefReadOnly,
}

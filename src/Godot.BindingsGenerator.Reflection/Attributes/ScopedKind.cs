namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Attributes that define the parameter scoped kind.
/// </summary>
public enum ScopedKind
{
    /// <summary>
    /// The parameter is not scoped.
    /// </summary>
    None,

    /// <summary>
    /// The parameter is ref scoped to the enclosing block or method.
    /// </summary>
    ScopedRef,

    /// <summary>
    /// The parameter is value scoped to the enclosing block or method.
    /// </summary>
    ScopedValue,
}

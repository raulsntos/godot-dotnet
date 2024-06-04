namespace Godot.Bridge;

/// <summary>
/// Defines a constant registered for a class.
/// </summary>
public class ConstantInfo
{
    /// <summary>
    /// Name of the constant.
    /// </summary>
    public StringName Name { get; }

    /// <summary>
    /// Value of the constant.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Name of the enum that contains the constant, or <see langword="null"/>
    /// if the constant is not contained in an enum.
    /// </summary>
    public StringName? EnumName { get; init; }

    /// <summary>
    /// Indicates whether the enum that contains the constant can be treated as
    /// a bit field; that is, set of flags.
    /// </summary>
    public bool IsFlagsEnum { get; init; }

    /// <summary>
    /// Constructs a new <see cref="ConstantInfo"/> with the specified name and value.
    /// </summary>
    /// <param name="name">Name of the constant.</param>
    /// <param name="value">Value of the constant.</param>
    public ConstantInfo(StringName name, long value)
    {
        Name = name;
        Value = value;
    }
}

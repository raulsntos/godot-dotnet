namespace Godot.Bridge;

/// <summary>
/// Defines a member registered for a class.
/// </summary>
public class PropertyInfo
{
    /// <summary>
    /// The member's type.
    /// </summary>
    public VariantType Type { get; }

    /// <summary>
    /// The member's type metadata.
    /// </summary>
    public VariantTypeMetadata TypeMetadata { get; }

    /// <summary>
    /// Name of the member.
    /// </summary>
    public StringName Name { get; }

    /// <summary>
    /// Hint that determines how the member should be handled by the editor.
    /// </summary>
    public PropertyHint Hint { get; init; }

    /// <summary>
    /// Additional metadata for <see cref="Hint"/>.
    /// The contents and format of the string depend on the type of hint.
    /// </summary>
    public string? HintString { get; init; }

    /// <summary>
    /// Name of the member's type when <see cref="Type"/> is <see cref="VariantType.Object"/>
    /// and the type is a registered class. Otherwise, it should be <see langword="null"/>.
    /// </summary>
    public StringName? ClassName { get; init; }

    /// <summary>
    /// Flags that determine how the member should be handled by the editor.
    /// </summary>
    public PropertyUsageFlags Usage { get; init; }

    /// <summary>
    /// Constructs a new <see cref="PropertyInfo"/> with the specified name, type, and type metadata.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="type">Type of the property.</param>
    /// <param name="metadata">Type metadata of the property.</param>
    public PropertyInfo(StringName name, VariantType type, VariantTypeMetadata metadata = VariantTypeMetadata.None)
    {
        Type = type;
        TypeMetadata = metadata;
        Name = name;
    }
}

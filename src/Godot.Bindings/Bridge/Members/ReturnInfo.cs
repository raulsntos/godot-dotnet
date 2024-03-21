namespace Godot.Bridge;

/// <summary>
/// Defines the return parameter of a <see cref="MethodInfo"/>.
/// </summary>
public sealed class ReturnInfo : PropertyInfo
{
    /// <summary>
    /// Constructs a new <see cref="ReturnInfo"/> with the specified type.
    /// </summary>
    /// <param name="type">Type of the return parameter.</param>
    /// <param name="metadata">Type metadata of the property.</param>
    public ReturnInfo(VariantType type, VariantTypeMetadata metadata = VariantTypeMetadata.None) : base(StringName.Empty, type, metadata) { }
}

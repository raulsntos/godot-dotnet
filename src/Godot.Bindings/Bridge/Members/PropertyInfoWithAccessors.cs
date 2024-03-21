namespace Godot.Bridge;

/// <summary>
/// Defines a property registered for a class.
/// </summary>
public sealed class PropertyInfoWithAccessors : PropertyInfo
{
    /// <summary>
    /// Name of a registered method to be used as the getter of this property.
    /// </summary>
    public StringName GetterName { get; }

    /// <summary>
    /// Name of a registered method to be used as the setter of this property.
    /// </summary>
    public StringName SetterName { get; }

    /// <summary>
    /// Constructs a new <see cref="PropertyInfo"/> with the specified name, type, getter, and setter.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="type">Type of the property.</param>
    /// <param name="getterName">Name of the registered method to be used as the getter of the property.</param>
    /// <param name="setterName">Name of the registered method to be used as the setter of the property.</param>
    public PropertyInfoWithAccessors(StringName name, VariantType type, StringName getterName, StringName setterName)
        : base(name, type)
    {
        GetterName = getterName;
        SetterName = setterName;
    }
}

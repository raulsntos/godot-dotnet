using System;

namespace Godot;

/// <summary>
/// Registers the constants of the specified enum type within the annotated extension class.
/// This allows a namespace-level enum to be shared across multiple extension classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class BindSharedEnumAttribute : Attribute
{
    /// <summary>
    /// Constructs a new <see cref="BindSharedEnumAttribute"/> with the specified enum type.
    /// </summary>
    /// <param name="enumType">The enum type whose constants will be registered for this class.</param>
    public BindSharedEnumAttribute(Type enumType)
    {
        EnumType = enumType;
    }

    /// <summary>
    /// The enum type whose constants are registered for this class.
    /// </summary>
    public Type EnumType { get; }

    /// <summary>
    /// Specifies the name that will be used to register the enum.
    /// If unspecified it will use the name of the enum type.
    /// </summary>
    public string? Name { get; init; }
}

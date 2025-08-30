using System;

namespace Godot;

/// <summary>
/// Registers the annotated property within an extension class.
/// It can also be used to annotate method parameters to customize marshalling.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
public sealed class BindPropertyAttribute : Attribute
{
    /// <summary>
    /// Specifies the name that will be used to register the property.
    /// If unspecified it will use the name of the annotated property.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Specifies the hint that will be used to register the property.
    /// If unspecified it will use the hint calculated from the property type.
    /// </summary>
    public PropertyHint Hint { get; init; } = PropertyHint.None;

    /// <summary>
    /// Specifies the hint string that will be used to register the property.
    /// If unspecified it will use the hint string calculated from the property type.
    /// </summary>
    public string? HintString { get; init; }

    /// <summary>
    /// Specifies the marshaller type that will be used to marshal this property.
    /// If unspecified the default marshaller for the annotated property's type
    /// will be used instead. If there is no default marshaller, a marshaller must
    /// be specified.
    /// </summary>
    public Type? Marshaller { get; init; }

    /// <summary>
    /// Specifies the Variant type that this property will be marshalled as.
    /// The marshaller will use this information to determine how to marshal the
    /// property.
    /// If unspecified (<see cref="VariantType.Nil"/>) the Variant type will be
    /// determined from the annotated property's type.
    /// </summary>
    public VariantType MarshalAs { get; init; } = VariantType.Nil;
}

using System;

namespace Godot.SourceGenerators;

/// <summary>
/// Describes a Godot property (or parameter) specification.
/// </summary>
internal readonly record struct GodotPropertySpec : IEquatable<GodotPropertySpec>
{
    /// <summary>
    /// Name of the property's symbol.
    /// This is the real name of the property in the source code.
    /// </summary>
    public required string SymbolName { get; init; }

    /// <summary>
    /// Fully qualified name of the property's type symbol, including the global namespace.
    /// This is the real type that the property is defined with in the source code,
    /// the type that needs to be used when marshalling may be different (use
    /// <see cref="MarshalInfo.FullyQualifiedTypeName"/> when marshalling).
    /// </summary>
    public required string FullyQualifiedTypeName { get; init; }

    /// <summary>
    /// Determines whether the property has an specified default value.
    /// This is only used for parameters.
    /// </summary>
    public bool HasExplicitDefaultValue { get; init; }

    /// <summary>
    /// The default value specified for the property.
    /// This is only used for parameters.
    /// </summary>
    public string? ExplicitDefaultValue { get; init; }

    /// <summary>
    /// Indicates how to marshal the property.
    /// </summary>
    public required MarshalInfo MarshalInfo { get; init; }

    /// <summary>
    /// Name specified in the <c>[BindProperty]</c> attribute for this property,
    /// or <see langword="null"/> if a name was not specified.
    /// If unspecified the name of the property will be <see cref="SymbolName"/>.
    /// </summary>
    public string? NameOverride { get; init; }

    /// <summary>
    /// Group information specified in the <c>[PropertyGroup]</c> attribute,
    /// if the property is annotated to define a group from this property.
    /// </summary>
    public GodotPropertyGroupSpec? GroupDefinition { get; init; }

    /// <summary>
    /// Subgroup information specified in the <c>[PropertySubgroup]</c> attribute,
    /// if the property is annotated to define a subgroup from this property.
    /// </summary>
    public GodotPropertySubgroupSpec? SubgroupDefinition { get; init; }
}

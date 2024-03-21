using System;

namespace Godot.SourceGenerators;

internal readonly record struct MarshalInfo : IEquatable<MarshalInfo>
{
    /// <summary>
    /// The Variant type that will be used to marshal the type.
    /// </summary>
    public required VariantType VariantType { get; init; }

    /// <summary>
    /// The metadata that narrows the <see cref="VariantType"/>.
    /// </summary>
    public required VariantTypeMetadata VariantTypeMetadata { get; init; }

    /// <summary>
    /// The fully-qualified name of the managed type.
    /// </summary>
    public required string FullyQualifiedTypeName { get; init; }

    public PropertyHint Hint { get; init; }

    public string? HintString { get; init; }

    public string? ClassName { get; init; }

    public PropertyUsageFlags Usage { get; init; }
}

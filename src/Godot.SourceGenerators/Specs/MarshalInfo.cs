using System;
using System.Diagnostics.CodeAnalysis;

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

    private readonly string? _fullyQualifiedMarshalAsTypeName;

    /// <summary>
    /// The fully-qualified name of the type to use for marshalling.
    /// </summary>
    /// <remarks>
    /// May return <see cref="FullyQualifiedTypeName"/> if it can be marshalled directly.
    /// </remarks>
    [AllowNull]
    public string FullyQualifiedMarshalAsTypeName
    {
        get => string.IsNullOrEmpty(_fullyQualifiedMarshalAsTypeName)
                ? FullyQualifiedTypeName
                : _fullyQualifiedMarshalAsTypeName!;

        init => _fullyQualifiedMarshalAsTypeName = value;
    }

    /// <summary>
    /// The fully-qualified name of the marshaller type that converts between
    /// <see cref="FullyQualifiedTypeName"/> and <see cref="FullyQualifiedMarshalAsTypeName"/>.
    /// </summary>
    public string? FullyQualifiedMarshallerTypeName { get; init; }

    /// <summary>
    /// Indicates whether the type is a specially-recognized type that can be marshalled
    /// with hardcoded rules.
    /// </summary>
    public bool TypeIsSpeciallyRecognized =>
        !string.IsNullOrEmpty(_fullyQualifiedMarshalAsTypeName) && !UsesCustomMarshaller;

    /// <summary>
    /// Indicates whether the type should use a custom marshaller to convert the type.
    /// If so, the marshaller type is specified by <see cref="FullyQualifiedMarshallerTypeName"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(FullyQualifiedMarshallerTypeName))]
    public bool UsesCustomMarshaller =>
        !string.IsNullOrEmpty(FullyQualifiedMarshallerTypeName);

    public PropertyHint Hint { get; init; }

    public string? HintString { get; init; }

    public string? ClassName { get; init; }

    public PropertyUsageFlags Usage { get; init; }
}

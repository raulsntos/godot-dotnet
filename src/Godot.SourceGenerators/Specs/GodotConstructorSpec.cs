using System;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

/// <summary>
/// Describes a Godot constructor specification.
/// </summary>
internal readonly record struct GodotConstructorSpec : IEquatable<GodotConstructorSpec>
{
    /// <summary>
    /// Fully qualified name of the builder type, including the global namespace.
    /// </summary>
    public required string FullyQualifiedBuilderTypeName { get; init; }

    /// <summary>
    /// The name of the method used to construct the type.
    /// </summary>
    public required string MethodSymbolName { get; init; }

    /// <summary>
    /// Indicates that the method to use to construct the type should be the
    /// parameterless constructor of the type specified by
    /// <see cref="FullyQualifiedBuilderTypeName"/>.
    /// </summary>
    public bool IsConstructor { get; private init; }

    public static GodotConstructorSpec CreateForConstructor(ITypeSymbol typeSymbol)
    {
        return new GodotConstructorSpec()
        {
            FullyQualifiedBuilderTypeName = typeSymbol.FullQualifiedNameWithGlobal(),
            MethodSymbolName = ".ctor",
            IsConstructor = true,
        };
    }
}

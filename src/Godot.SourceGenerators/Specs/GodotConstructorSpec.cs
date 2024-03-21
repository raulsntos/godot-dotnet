using System;

namespace Godot.SourceGenerators;

/// <summary>
/// Describes a Godot constructor specification.
/// </summary>
internal readonly record struct GodotConstructorSpec : IEquatable<GodotConstructorSpec>
{
    /// <summary>
    /// The name of the static method that will be used to construct the type.
    /// This is the method in the type that has the <c>[BindConstructor]</c> attribute.
    /// If this is <see langword="null"/>, the type will be constructed using the
    /// parameter-less constructor.
    /// </summary>
    public string? MethodSymbolName { get; init; }
}

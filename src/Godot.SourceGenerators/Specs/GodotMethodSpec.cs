using System;

namespace Godot.SourceGenerators;

/// <summary>
/// Describes a Godot method specification.
/// </summary>
internal readonly record struct GodotMethodSpec : IEquatable<GodotMethodSpec>
{
    /// <summary>
    /// Name of the method's symbol.
    /// This is the real name of the method in the source code.
    /// </summary>
    public required string SymbolName { get; init; }

    /// <summary>
    /// Indicates whether the method should be registered as a static method.
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Indicates whether the method should be registered as a virtual method
    /// and if the method must be overridden in user scripts.
    /// </summary>
    public bool IsVirtual { get; init; }

    /// <summary>
    /// Describes the parameters of this method.
    /// </summary>
    public EquatableArray<GodotPropertySpec> Parameters { get; init; }

    /// <summary>
    /// Describes the return parameter of this method.
    /// </summary>
    public GodotPropertySpec? ReturnParameter { get; init; }

    /// <summary>
    /// Name specified in the <c>[BindMethod]</c> attribute for this property,
    /// or <see langword="null"/> if a name was not specified.
    /// If unspecified the name of the method will be <see cref="SymbolName"/>.
    /// </summary>
    public string? NameOverride { get; init; }
}

using System;

namespace Godot.SourceGenerators;

/// <summary>
/// Describes a Godot signal specification.
/// </summary>
internal readonly record struct GodotSignalSpec : IEquatable<GodotSignalSpec>
{
    /// <summary>
    /// Name of the delegate's symbol.
    /// This is the real name of the delegate in the source code.
    /// </summary>
    public required string SymbolName { get; init; }

    /// <summary>
    /// Describes the parameters of the delegate that defines the signal.
    /// </summary>
    public EquatableArray<GodotPropertySpec> Parameters { get; init; }

    /// <summary>
    /// Name specified in the <c>[Signal]</c> attribute for this signal,
    /// or <see langword="null"/> if a name was not specified.
    /// If unspecified the name of the signal will be <see cref="SymbolName"/>.
    /// </summary>
    public string? NameOverride { get; init; }
}

using System;

namespace Godot.SourceGeneration;

/// <summary>
/// Describes a Godot constant specification.
/// </summary>
internal readonly record struct GodotConstantSpec : IEquatable<GodotConstantSpec>
{
    /// <summary>
    /// Name of the constant's symbol.
    /// This is the real name of the constant in the source code.
    /// </summary>
    public required string SymbolName { get; init; }

    /// <summary>
    /// Name of the enum that contains this constant.
    /// This is only included if the constant is contained by an enum with the
    /// <c>[BindEnum]</c> attribute.
    /// This is the real name of the enum type in the source code.
    /// </summary>
    public string? EnumSymbolName { get; init; }

    /// <summary>
    /// Name specified in the <c>[BindEnum]</c> attribute for the enum
    /// that contains this constant, or <see langword="null"/> if a name
    /// was not specified or this constant is not contained by an enum.
    /// If unspecified the name of the enum will be <see cref="EnumSymbolName"/>.
    /// </summary>
    public string? EnumNameOverride { get; init; }

    /// <summary>
    /// Indicates whether the enum that contains this constant is a flags enum;
    /// that is, the enum type is annotated with the <c>[Flags]</c> attribute.
    /// </summary>
    public bool IsFlagsEnum { get; init; }

    /// <summary>
    /// Name specified in the <c>[BindConstant]</c> attribute for this constant,
    /// or <see langword="null"/> if a name was not specified.
    /// If unspecified the name of the constant will be <see cref="SymbolName"/>.
    /// </summary>
    public string? NameOverride { get; init; }
}

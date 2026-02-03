using System;

namespace Godot.SourceGeneration;

/// <summary>
/// Describes a type that contains other nested types.
/// </summary>
internal readonly record struct ContainingSymbol : IEquatable<ContainingSymbol>
{
    /// <summary>
    /// Enumerates the kinds of types that exist.
    /// </summary>
    public enum Kind
    {
        Unknown,
        Interface,
        Class,
        Struct,
        RecordClass,
        RecordStruct,
    }

    /// <summary>
    /// Indicates the kind of type symbol that this symbol represents.
    /// </summary>
    public required Kind SymbolKind { get; init; }

    /// <summary>
    /// Name of the type's symbol.
    /// </summary>
    public required string SymbolName { get; init; }
}

using System;

namespace Godot.SourceGenerators;

internal readonly record struct ContainingSymbol : IEquatable<ContainingSymbol>
{
    public enum Kind
    {
        Unknown,
        Interface,
        Class,
        Struct,
        RecordClass,
        RecordStruct,
    }

    public required Kind SymbolKind { get; init; }

    public required string SymbolName { get; init; }
}

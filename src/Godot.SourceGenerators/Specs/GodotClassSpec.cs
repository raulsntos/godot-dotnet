using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

/// <summary>
/// Describes a Godot class specification.
/// </summary>
internal readonly record struct GodotClassSpec : IEquatable<GodotClassSpec>
{
    /// <summary>
    /// Describes the state of the class specification: it is either perfectly
    /// valid or it isn't. If it is invalid, this indicates the reason.
    /// </summary>
    internal enum StateType
    {
        Valid,
        InvalidBaseType
    }

    /// <summary>
    /// Name of the type's symbol.
    /// This is the real name of the type in the source code.
    /// </summary>
    public required string SymbolName { get; init; }

    /// <summary>
    /// Fully qualified name of the type's symbol, including the global namespace.
    /// </summary>
    public required string FullyQualifiedSymbolName { get; init; }

    /// <summary>
    /// Fully qualified name of the type's containing namespace.
    /// Or <see langword="null"/> if the type is a top-level type.
    /// </summary>
    public string? FullyQualifiedNamespace { get; init; }

    /// <summary>
    /// Type symbols that contain the type's symbol, if any.
    /// The collection is ordered from lowest-level to highest-level,
    /// for example for a type <c>C</c> nested in type <c>B</c> that is
    /// in turn nested in type <c>A</c> it will contain <c>[B, A]</c>.
    /// </summary>
    public EquatableArray<ContainingSymbol> ContainingTypeSymbols { get; init; }

    /// <summary>
    /// Fully qualified name of the base type, including the global namespace.
    /// This can never be <see langword="null"/> because every registered class
    /// must ultimately derive from <c>GodotObject</c>.
    /// </summary>
    public required string FullyQualifiedBaseTypeName { get; init; }

    /// <summary>
    /// Describes how the type should be constructed.
    /// Types that can't be instantiated (e.g.: generic type definitions or abstract types)
    /// should not have a <see cref="GodotConstructorSpec"/>.
    /// </summary>
    public GodotConstructorSpec? Constructor { get; init; }

    /// <summary>
    /// Describes the constants that should be registered for the type.
    /// </summary>
    public EquatableArray<GodotConstantSpec> Constants { get; init; }

    /// <summary>
    /// Describes the properties that should be registered for the type.
    /// </summary>
    public EquatableArray<GodotPropertySpec> Properties { get; init; }

    /// <summary>
    /// Describes the methods that should be registered for the type.
    /// </summary>
    public EquatableArray<GodotMethodSpec> Methods { get; init; }

    /// <summary>
    /// Describes the signals that should be registered for the type.
    /// </summary>
    public EquatableArray<GodotSignalSpec> Signals { get; init; }

    /// <summary>
    /// Location of the class declaration in the source code.
    /// </summary>
    public Location LocationInSource { get; init; }

    /// <summary>
    /// Indicates whether the type derives from <c>Godot.GodotObject</c>. Types
    /// that do not derive from <c>Godot.GodotObject</c> cannot be automatically
    /// registered with the engine.
    /// </summary>
    public bool IsGodotObject { get; init; }

    /// <summary>
    /// State of the class specification.
    /// </summary>
    public StateType State
    {
        get
        {
            if (!IsGodotObject)
            {
                return StateType.InvalidBaseType;
            }

            return StateType.Valid;
        }
    }

    public Diagnostic? GetDiagnostic()
    {
        return State switch
        {
            StateType.InvalidBaseType => Diagnostics.ClassDoesNotHaveCorrectBaseType(LocationInSource, this),
            _ => null
        };
    }

    /// <summary>
    /// Generate a unique hint name for the generated source code
    /// from the type name and the containing symbols.
    /// </summary>
    /// <returns>A unique hint name.</returns>
    public string GetHintName()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(FullyQualifiedNamespace))
        {
            sb.Append(FullyQualifiedNamespace);
            sb.Append('.');
        }

        for (int i = ContainingTypeSymbols.Count - 1; i >= 0; i--)
        {
            var containingSymbol = ContainingTypeSymbols[i];
            sb.Append(containingSymbol.SymbolName);
            sb.Append('.');
        }

        sb.Append(SymbolName);

        return sb.ToString();
    }
}

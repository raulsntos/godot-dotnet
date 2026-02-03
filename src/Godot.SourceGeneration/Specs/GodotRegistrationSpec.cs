using System;

namespace Godot.SourceGeneration;

/// <summary>
/// Describes a registered Godot class specification.
/// </summary>
internal readonly record struct GodotRegistrationSpec : IEquatable<GodotRegistrationSpec>
{
    /// <summary>
    /// Enumerates the kinds of GDExtension classes that can be registered.
    /// </summary>
    public enum Kind
    {
        /// <summary>
        /// A normal GDExtension class, runs in the editor (i.e.: a tool class).
        /// </summary>
        Class,

        /// <summary>
        /// A runtime GDExtension class, does not run in the editor.
        /// </summary>
        RuntimeClass,

        /// <summary>
        /// A virtual GDExtension class, it can be instantiated by the engine but not by the user.
        /// </summary>
        VirtualClass,

        /// <summary>
        /// An abstract GDExtension class, it can't be instantiated.
        /// </summary>
        AbstractClass,

        /// <summary>
        /// An internal GDExtension class, hidden in the editor.
        /// </summary>
        InternalClass,
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
    /// Fully qualified name of the type's base type's symbol, including the global namespace.
    /// </summary>
    public required string FullyQualifiedBaseSymbolName { get; init; }

    /// <summary>
    /// Indicates the kind of registration that this specification represents.
    /// </summary>
    public required Kind RegistrationKind { get; init; }
}

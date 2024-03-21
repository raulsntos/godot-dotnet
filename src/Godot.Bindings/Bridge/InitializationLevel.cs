using Godot.NativeInterop;

namespace Godot.Bridge;

/// <summary>
/// Indicates the level reached by the Godot initialization.
/// </summary>
public enum InitializationLevel : uint
{
    /// <summary>
    /// First initialization level, only the core built-in types are available.
    /// </summary>
    Core = GDExtensionInitializationLevel.GDEXTENSION_INITIALIZATION_CORE,

    /// <summary>
    /// Second initialization level, the server classes are available.
    /// All types initialized in previous levels are also available.
    /// </summary>
    Servers = GDExtensionInitializationLevel.GDEXTENSION_INITIALIZATION_SERVERS,

    /// <summary>
    /// Third initialization level, the scene classes are available.
    /// All types initialized in previous levels are also available.
    /// </summary>
    Scene = GDExtensionInitializationLevel.GDEXTENSION_INITIALIZATION_SCENE,

    /// <summary>
    /// Fourth initialization level, the editor classes are available.
    /// All types initialized in previous levels are also available.
    /// </summary>
    Editor = GDExtensionInitializationLevel.GDEXTENSION_INITIALIZATION_EDITOR,
}

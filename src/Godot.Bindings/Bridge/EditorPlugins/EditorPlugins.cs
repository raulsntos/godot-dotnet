using System;
using System.Collections.Generic;
using Godot.NativeInterop;

namespace Godot.Bridge;

/// <summary>
/// Utility to register editor plugins within the Godot engine.
/// </summary>
public static class EditorPlugins
{
    private static readonly HashSet<StringName> _registeredPlugins = [];

    /// <summary>
    /// Register <typeparamref name="T"/> as an <see cref="EditorPlugin"/>
    /// in the engine.
    /// </summary>
    /// <typeparam name="T">Type of the editor plugin.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// A type has already been registered
    /// </exception>
    public static unsafe void AddByType<T>() where T : EditorPlugin
    {
        StringName className = new StringName(typeof(T).Name);

        if (_registeredPlugins.Contains(className))
        {
            throw new InvalidOperationException($"Type '{typeof(T)}' has already been registered as an editor plugin.");
        }

        NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;
        GodotBridge.GDExtensionInterface.editor_add_plugin(classNameNative.GetUnsafeAddress());
    }
}

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
    private static readonly Stack<StringName> _pluginRegisterStack = [];

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

        _registeredPlugins.Add(className);
        _pluginRegisterStack.Push(className);

        NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;
        GodotBridge.GDExtensionInterface.editor_add_plugin(classNameNative.GetUnsafeAddress());
    }

    internal unsafe static void RemoveAllPlugins()
    {
        while (_pluginRegisterStack.TryPop(out StringName? className))
        {
            NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;
            GodotBridge.GDExtensionInterface.editor_remove_plugin(classNameNative.GetUnsafeAddress());

            _registeredPlugins.Remove(className);
        }
    }
}

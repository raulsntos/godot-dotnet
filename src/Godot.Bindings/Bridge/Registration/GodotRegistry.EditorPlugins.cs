using System;
using System.Collections.Generic;
using Godot.NativeInterop;

namespace Godot.Bridge;

partial class GodotRegistry
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
    public static unsafe void AddEditorPluginByType<T>() where T : EditorPlugin
    {
        StringName className = new StringName(typeof(T).Name);

        if (_registeredPlugins.Contains(className))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_EditorPluginAlreadyRegistered(typeof(T)));
        }

        _registeredPlugins.Add(className);
        _pluginRegisterStack.Push(className);

        NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;
        GodotBridge.GDExtensionInterface.editor_add_plugin(classNameNative.GetUnsafeAddress());
    }

    internal unsafe static void RemoveAllEditorPlugins()
    {
        while (_pluginRegisterStack.TryPop(out StringName? className))
        {
            NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;
            GodotBridge.GDExtensionInterface.editor_remove_plugin(classNameNative.GetUnsafeAddress());

            _registeredPlugins.Remove(className);
        }
    }
}

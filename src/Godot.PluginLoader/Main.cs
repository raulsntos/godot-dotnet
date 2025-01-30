using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: DisableRuntimeMarshalling]

namespace Godot.PluginLoader;

internal static unsafe class Main
{
    private static readonly Dictionary<string, WeakGodotLoadContext> _loadContexts = [];

    [UnmanagedCallersOnly]
    public static void LoadAssembly(nint assemblyPathNative, nint fullyQualifiedTypeNameNative, nint methodNameNative, nint outGDExtensionInitializationFunction)
    {
        ArgumentNullException.ThrowIfNull((void*)outGDExtensionInitializationFunction);

        string assemblyPath = MarshalToString(assemblyPathNative);
        string fullyQualifiedTypeName = MarshalToString(fullyQualifiedTypeNameNative);
        string methodName = MarshalToString(methodNameNative);

        if (_loadContexts.ContainsKey(assemblyPath))
        {
            throw new InvalidOperationException($"Trying to load assembly '{assemblyPath}' that is already loaded.");
        }

        var alc = new GodotLoadContext(assemblyPath);
        try
        {
            _loadContexts.Add(assemblyPath, new WeakGodotLoadContext(alc));

            var assembly = alc.LoadFromAssemblyPath(assemblyPath);

            var type = Type.GetType(fullyQualifiedTypeName, alc.LoadFromAssemblyName, null, throwOnError: true)!;

            // Match semantics of hostfxr's load_assembly_and_get_function_pointer.
            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var methodInfo = type.GetMethod(methodName, bindingFlags);
            if (methodInfo is null)
            {
                throw new MissingMethodException(fullyQualifiedTypeName, methodName);
            }

            if (!methodInfo.IsDefined(typeof(UnmanagedCallersOnlyAttribute)))
            {
                throw new InvalidOperationException("Entry-point method must have the UnmanagedCallersOnly attribute.");
            }

            *(nint*)outGDExtensionInitializationFunction = methodInfo.MethodHandle.GetFunctionPointer();
        }
        catch
        {
            // Loading assembly failed, make sure everything is unloaded before returning.
            alc.Unload();
            _loadContexts.Remove(assemblyPath);
            throw;
        }
    }

    [UnmanagedCallersOnly]
    public static void UnloadAssembly(nint assemblyPathNative)
    {
        string assemblyPath = MarshalToString(assemblyPathNative);

        if (!_loadContexts.TryGetValue(assemblyPath, out var alc))
        {
            throw new InvalidOperationException($"Trying to unload assembly '{assemblyPath}' that is not loaded.");
        }

        alc.Unload();
        _loadContexts.Remove(assemblyPath);

        for (int i = 0; alc.IsAlive && (i < 10); i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        if (alc.IsAlive)
        {
            // If the ALC is still alive, then something is keeping it alive and can't be unloaded.
            throw new InvalidOperationException($"Failed to unload assembly '{assemblyPath}'. Possible causes: Strong GC handles, running threads, etc.");
        }
    }

    private static string MarshalToString(nint stringNative, [CallerArgumentExpression(nameof(stringNative))] string? paramName = null)
    {
        string? result = Marshal.PtrToStringAuto(stringNative);
        ArgumentNullException.ThrowIfNull(result, paramName);
        return result;
    }
}

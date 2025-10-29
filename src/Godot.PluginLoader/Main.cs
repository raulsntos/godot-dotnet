using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: DisableRuntimeMarshalling]

namespace Godot.PluginLoader;

internal static unsafe class Main
{
    private static readonly Dictionary<string, WeakGodotLoadContext> _loadContexts = [];

    [UnmanagedCallersOnly]
    public static bool LoadAssembly(nint assemblyPathNative, nint fullyQualifiedTypeNameNative, nint methodNameNative, nint outGDExtensionInitializationFunction)
    {
        try
        {
            LoadAssemblyCore(assemblyPathNative, fullyQualifiedTypeNameNative, methodNameNative, outGDExtensionInitializationFunction);
            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to load assembly. Exception: {e}");
            return false;
        }
    }

    [UnmanagedCallersOnly]
    public static bool UnloadAssembly(nint assemblyPathNative)
    {
        try
        {
            UnloadAssemblyCore(assemblyPathNative);
            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to unload assembly. Exception: {e}");
            return false;
        }
    }

    private static void LoadAssemblyCore(nint assemblyPathNative, nint fullyQualifiedTypeNameNative, nint methodNameNative, nint outGDExtensionInitializationFunction)
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

            // Load the assembly in memory to prevent locking the file on Windows.
            using var assemblyStream = File.OpenRead(assemblyPath);
            var assembly = alc.LoadFromStream(assemblyStream);

            if (IsGodotSharpAssembly(assembly))
            {
                throw new InvalidOperationException("Assembly uses old GodotSharp bindings.");
            }

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

    private static void UnloadAssemblyCore(nint assemblyPathNative)
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

    private static bool IsGodotSharpAssembly(Assembly assembly)
    {
        // If the assembly contains a reference to the 'GodotSharp' assembly,
        // it must be an assembly using the old C# bindings; otherwise, assume
        // it's using the new Godot .NET bindings or custom bindings.
        foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
        {
            if (referencedAssembly.Name == "GodotSharp")
            {
                return true;
            }
        }

        return false;
    }

    private static string MarshalToString(nint stringNative, [CallerArgumentExpression(nameof(stringNative))] string? paramName = null)
    {
        string? result = Marshal.PtrToStringAuto(stringNative);
        ArgumentNullException.ThrowIfNull(result, paramName);
        return result;
    }
}

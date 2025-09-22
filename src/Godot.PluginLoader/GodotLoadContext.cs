using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Godot.PluginLoader;

/// <summary>
/// Implements an ALC to be used by Godot when loading .NET assemblies using hostfxr
/// which allows unloading the .NET assemblies when the GDExtension needs to be reloaded.
/// </summary>
internal sealed class GodotLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public GodotLoadContext(string mainAssemblyPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (!string.IsNullOrEmpty(assemblyPath))
        {
            // Load in memory to prevent locking the file.

            using var assemblyFile = File.Open(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            string pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");

            if (File.Exists(pdbPath))
            {
                using var pdbFile = File.Open(pdbPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return LoadFromStream(assemblyFile, pdbFile);
            }

            return LoadFromStream(assemblyFile);
        }

        return null;
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        string? unmanagedDllPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (!string.IsNullOrEmpty(unmanagedDllPath))
        {
            return LoadUnmanagedDllFromPath(unmanagedDllPath);
        }

        return 0;
    }
}

using System;

namespace Godot.PluginLoader;

/// <summary>
/// Weak reference to a <see cref="GodotLoadContext"/> so it can be unloaded.
/// Keeping a reference to the ALC would prevent it from unloading, so using
/// a weak reference like this is needed.
/// This type keeps a strong reference to the ALC until the <see cref="Unload"/>
/// method is called, to prevent the ALC from being unferenced early.
/// </summary>
internal sealed class WeakGodotLoadContext
{
    private GodotLoadContext? _alc;
    private readonly WeakReference _weakReference;

    public bool IsAlive => _weakReference.IsAlive;

    public WeakGodotLoadContext(GodotLoadContext alc)
    {
        _alc = alc;
        _weakReference = new WeakReference(alc, trackResurrection: true);
    }

    public void Unload()
    {
        _alc?.Unload();
        _alc = null;
    }
}

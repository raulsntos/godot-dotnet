using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Godot.NativeInterop;

internal static class DisposablesTracker
{
    private static readonly ConcurrentDictionary<WeakReference<GodotObject>, byte> _godotObjectInstances =
        new();

    private static readonly ConcurrentDictionary<WeakReference<IDisposable>, byte> _otherInstances =
        new();

    internal static void DisposeAll()
    {
        bool isStdOutVerbose = OS.Singleton.IsStdOutVerbose();

        if (isStdOutVerbose)
        {
            GD.Print("Disposing tracked instances...");
        }

        // Dispose GodotObjects first, and only then dispose other disposables
        // like StringName, NodePath, GodotArray/GodotDictionary, etc.
        // The GodotObject Dispose() method may need any of the later instances.

        foreach (WeakReference<GodotObject> item in _godotObjectInstances.Keys)
        {
            if (item.TryGetTarget(out GodotObject? self))
            {
                self.Dispose();
            }
        }

        foreach (WeakReference<IDisposable> item in _otherInstances.Keys)
        {
            if (item.TryGetTarget(out IDisposable? self))
            {
                self.Dispose();
            }
        }

        if (isStdOutVerbose)
        {
            GD.Print("Finished disposing tracked instances.");
        }
    }

    public static WeakReference<GodotObject> RegisterGodotObject(GodotObject godotObject)
    {
        var weakReferenceToSelf = new WeakReference<GodotObject>(godotObject);
        _godotObjectInstances.TryAdd(weakReferenceToSelf, 0);
        return weakReferenceToSelf;
    }

    public static WeakReference<IDisposable> RegisterDisposable(IDisposable disposable)
    {
        Debug.Assert(disposable is not GodotObject, $"GodotObjects must be registered with {nameof(RegisterGodotObject)}.");

        var weakReferenceToSelf = new WeakReference<IDisposable>(disposable);
        _otherInstances.TryAdd(weakReferenceToSelf, 0);
        return weakReferenceToSelf;
    }

    public static void UnregisterGodotObject(GodotObject godotObject, WeakReference<GodotObject> weakReferenceToSelf)
    {
        if (!_godotObjectInstances.TryRemove(weakReferenceToSelf, out _))
        {
            throw new ArgumentException("Godot Object not registered.", nameof(weakReferenceToSelf));
        }
    }

    public static void UnregisterDisposable(WeakReference<IDisposable> weakReference)
    {
        if (!_otherInstances.TryRemove(weakReference, out _))
        {
            throw new ArgumentException("Disposable not registered.", nameof(weakReference));
        }
    }
}

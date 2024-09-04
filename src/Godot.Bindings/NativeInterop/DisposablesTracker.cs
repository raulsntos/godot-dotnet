using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Godot.NativeInterop;

internal static class DisposablesTracker
{
    private static readonly ConcurrentDictionary<GodotObject, byte> _godotObjectInstances =
        new();

    private static readonly ConcurrentDictionary<WeakReference<RefCounted>, byte> _refCountedInstances =
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

        foreach (GodotObject self in _godotObjectInstances.Keys)
        {
            self.Dispose();
        }

        foreach (WeakReference<RefCounted> item in _refCountedInstances.Keys)
        {
            if (item.TryGetTarget(out RefCounted? self))
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

    public static WeakReference<RefCounted>? RegisterGodotObject(GodotObject godotObject)
    {
        if (godotObject is RefCounted rc)
        {
            var weakReferenceToSelf = new WeakReference<RefCounted>(rc);
            _refCountedInstances.TryAdd(weakReferenceToSelf, 0);
            return weakReferenceToSelf;
        }
        _godotObjectInstances.TryAdd(godotObject, 0);
        return null;
    }

    public static WeakReference<IDisposable> RegisterDisposable(IDisposable disposable)
    {
        Debug.Assert(disposable is not GodotObject, $"GodotObjects must be registered with {nameof(RegisterGodotObject)}.");

        var weakReferenceToSelf = new WeakReference<IDisposable>(disposable);
        _otherInstances.TryAdd(weakReferenceToSelf, 0);
        return weakReferenceToSelf;
    }

    public static void UnregisterGodotObject(GodotObject godotObject, WeakReference<RefCounted>? weakReferenceToSelf)
    {
        if (godotObject is RefCounted rc ? !_refCountedInstances.TryRemove(weakReferenceToSelf!, out _) : !_godotObjectInstances.TryRemove(godotObject, out _))
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

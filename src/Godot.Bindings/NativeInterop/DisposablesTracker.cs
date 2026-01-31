using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Godot.NativeInterop;

internal static class DisposablesTracker
{
    private static readonly ConcurrentDictionary<WeakReference<GodotObject>, byte> _godotObjectInstances = [];

    private static readonly ConcurrentDictionary<WeakReference<IDisposable>, byte> _otherInstances = [];

    private static bool? _isStdOutVerbose;

    internal static void DisposeAll()
    {
        // We cache the result of IsStdOutVerbose() because the next time we call it,
        // the OS singleton would already be disposed. And stdout verbosity won't change.
        _isStdOutVerbose ??= OS.Singleton.IsStdOutVerbose();

        if (_isStdOutVerbose.Value)
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

        if (_isStdOutVerbose.Value)
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

    public static void UnregisterGodotObject(WeakReference<GodotObject> weakReferenceToSelf)
    {
        if (!_godotObjectInstances.TryRemove(weakReferenceToSelf, out _))
        {
            throw new ArgumentException(SR.Argument_DisposableGodotObjectNotRegistered, nameof(weakReferenceToSelf));
        }
    }

    public static void UnregisterDisposable(WeakReference<IDisposable> weakReference)
    {
        if (!_otherInstances.TryRemove(weakReference, out _))
        {
            throw new ArgumentException(SR.Argument_DisposableNotRegistered, nameof(weakReference));
        }
    }
}

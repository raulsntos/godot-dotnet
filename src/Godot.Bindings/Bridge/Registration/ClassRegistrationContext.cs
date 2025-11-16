using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Godot.Bridge;

/// <summary>
/// Context for registering classes and their members within the Godot engine.
/// </summary>
public sealed partial class ClassRegistrationContext : IDisposable
{
    private bool _disposed;

    private GCHandle _gcHandle;

    internal GCHandle GCHandle => _gcHandle;

    internal StringName ClassName { get; }

    private readonly ConcurrentQueue<Action> _registerBindingActions = [];

    internal ClassRegistrationContext(StringName className)
    {
        _gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
        ClassName = className;
    }

    internal void RegisterBindings()
    {
        while (_registerBindingActions.TryDequeue(out var register))
        {
            register();
        }
    }

    /// <summary>
    /// Disposes of this <see cref="ClassRegistrationContext"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _gcHandle.Free();
    }
}

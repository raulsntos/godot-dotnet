using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Godot.Bridge;

/// <summary>
/// Context for registering classes and their members within the Godot engine.
/// </summary>
public partial class ClassRegistrationContext
{
    internal GCHandle GCHandle { get; }

    internal StringName ClassName { get; }

    private readonly ConcurrentQueue<Action> _registerBindingActions = [];

    internal ClassRegistrationContext(StringName className)
    {
        GCHandle = GCHandle.Alloc(this, GCHandleType.Weak);
        ClassName = className;
    }

    internal void RegisterBindings()
    {
        while (_registerBindingActions.TryDequeue(out var register))
        {
            register();
        }
    }
}

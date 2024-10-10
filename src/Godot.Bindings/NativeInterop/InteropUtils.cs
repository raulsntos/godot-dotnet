using System;
using System.Collections.Frozen;
using Godot.Bridge;

namespace Godot.NativeInterop;

internal static partial class InteropUtils
{
    internal static FrozenDictionary<StringName, Func<nint, GodotObject>> CreateHelpers { get; private set; }

    internal static FrozenDictionary<StringName, Action<ClassRegistrationContext>> RegisterVirtualOverridesHelpers { get; private set; }

    static InteropUtils()
    {
        EnsureHelpersInitialized();
    }
}

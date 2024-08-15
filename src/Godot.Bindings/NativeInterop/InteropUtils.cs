using System;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Godot.Bridge;

namespace Godot.NativeInterop;

public static partial class InteropUtils
{
    internal static FrozenDictionary<StringName, Func<nint, GodotObject>> CreateHelpers { get; private set; }

    internal delegate void RegisterVirtualOverrideHelper([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)] Type type, ClassDBRegistrationContext context);
    internal static FrozenDictionary<StringName, RegisterVirtualOverrideHelper> RegisterVirtualOverridesHelpers { get; private set; }

    static InteropUtils()
    {
        EnsureHelpersInitialized();
    }
}

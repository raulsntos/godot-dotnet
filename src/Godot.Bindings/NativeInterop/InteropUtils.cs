using System;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Godot.Bridge;

namespace Godot.NativeInterop;

internal static partial class InteropUtils
{
    internal static FrozenDictionary<StringName, Func<nint, GodotObject>> CreateHelpers { get; private set; }

    internal delegate void RegisterVirtualOverrideHelper([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] Type type, ClassRegistrationContext context);
    internal static FrozenDictionary<StringName, RegisterVirtualOverrideHelper> RegisterVirtualOverridesHelpers { get; private set; }

    static InteropUtils()
    {
        EnsureHelpersInitialized();
    }
}

using System.Runtime.InteropServices;

namespace Godot.Bridge;

/// <summary>
/// Context for registering classes and their members within the Godot engine.
/// </summary>
public partial class ClassDBRegistrationContext
{
    internal GCHandle GCHandle { get; }

    internal StringName ClassName { get; }

    internal ClassDBRegistrationContext(StringName className)
    {
        GCHandle = GCHandle.Alloc(this, GCHandleType.Weak);
        ClassName = className;
    }
}

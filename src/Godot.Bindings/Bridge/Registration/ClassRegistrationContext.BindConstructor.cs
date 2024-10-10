using System;

namespace Godot.Bridge;

partial class ClassRegistrationContext
{
    internal Func<GodotObject>? RegisteredConstructor { get; private set; }

    /// <summary>
    /// Register a function to construct a new instance of the class.
    /// </summary>
    /// <param name="constructor">Function that constructs the class.</param>
    public unsafe void BindConstructor(Func<GodotObject> constructor)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        RegisteredConstructor = constructor;
    }
}

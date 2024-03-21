using System;

namespace Godot.Bridge;

partial class ClassDBRegistrationContext
{
    private Func<GodotObject>? _registeredConstructor;

    internal Func<GodotObject>? RegisteredConstructor => _registeredConstructor;

    /// <summary>
    /// Register a function to construct a new instance of the class.
    /// </summary>
    /// <param name="constructor">Function that constructs the class.</param>
    public unsafe void BindConstructor(Func<GodotObject> constructor)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        _registeredConstructor = constructor;
    }
}

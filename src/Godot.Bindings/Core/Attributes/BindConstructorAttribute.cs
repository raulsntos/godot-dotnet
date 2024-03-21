using System;

namespace Godot;

/// <summary>
/// Registers the annotated static method as the constructor of an extension class.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class BindConstructorAttribute : Attribute { }

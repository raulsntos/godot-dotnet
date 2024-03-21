using System;

namespace Godot;

/// <summary>
/// Internal attribute used to specify the name of a built-in Godot class
/// as is registered in ClassDB.
/// This attribute is only added to classes that have a different name in the
/// generated bindings than the one they use in ClassDB.
/// The source generators use this attribute to determine the name of built-in
/// classes
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class GodotNativeClassNameAttribute : Attribute
{
    public string Name { get; init; }

    public GodotNativeClassNameAttribute(string name)
    {
        Name = name;
    }
}

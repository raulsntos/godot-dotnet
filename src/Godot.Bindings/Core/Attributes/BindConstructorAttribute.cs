using System;

namespace Godot;

/// <summary>
/// Registers a static method as the constructor of the annotated extension class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BindConstructorAttribute : Attribute
{
    /// <summary>
    /// The type of the builder to use to construct the extension class.
    /// </summary>
    public Type BuilderType { get; }

    /// <summary>
    /// The name of the method on the builder to use to construct the extension class.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BindConstructorAttribute"/> class.
    /// </summary>
    /// <param name="builderType">The type of the builder to use to construct the extension class.</param>
    /// <param name="methodName">The name of the method on the builder to use to construct the extension class.</param>
    public BindConstructorAttribute(Type builderType, string methodName)
    {
        BuilderType = builderType;
        MethodName = methodName;
    }
}

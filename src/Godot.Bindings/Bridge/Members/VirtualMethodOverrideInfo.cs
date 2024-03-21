namespace Godot.Bridge;

/// <summary>
/// Defines a virtual method override registered for a class.
/// A virtual method override is the method in an extension class that is bound
/// as the override of a virtual method in a built-in class.
/// </summary>
public partial class VirtualMethodOverrideInfo
{
    /// <summary>
    /// Name of the method.
    /// </summary>
    public StringName Name { get; }

    /// <summary>
    /// The <see cref="MethodBindInvoker"/> that can invoke this method.
    /// </summary>
    public MethodBindInvoker Invoker { get; }

    /// <summary>
    /// Constructs a new <see cref="VirtualMethodOverrideInfo"/> with the specified
    /// name and an invoker.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    /// <param name="invoker">Invoker for the method.</param>
    public VirtualMethodOverrideInfo(StringName name, MethodBindInvoker invoker)
    {
        Name = name;
        Invoker = invoker;
    }
}

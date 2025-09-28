#nullable enable

namespace NamespaceB;

partial class ClassTwo
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    {
        context.BindConstructor(() => new global::NamespaceB.ClassTwo());
    }
}

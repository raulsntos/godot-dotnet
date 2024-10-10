#nullable enable

namespace NamespaceA;

partial class ClassOne
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
    internal static void BindMethods(global::Godot.Bridge.ClassRegistrationContext context)
    {
        context.BindConstructor(() => new ClassOne());
    }
}

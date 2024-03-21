#nullable enable

namespace NamespaceA.NamespaceB;

partial class MyNestedNamespacesNode
{
    internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
    {
        context.BindConstructor(() => new MyNestedNamespacesNode());
    }
}

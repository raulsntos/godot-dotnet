#nullable enable

namespace NamespaceB;

partial class ClassTwo
{
    internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
    {
        context.BindConstructor(() => new ClassTwo());
    }
}

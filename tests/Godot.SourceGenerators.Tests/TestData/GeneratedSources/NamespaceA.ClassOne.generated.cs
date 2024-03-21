#nullable enable

namespace NamespaceA;

partial class ClassOne
{
    internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
    {
        context.BindConstructor(() => new ClassOne());
    }
}

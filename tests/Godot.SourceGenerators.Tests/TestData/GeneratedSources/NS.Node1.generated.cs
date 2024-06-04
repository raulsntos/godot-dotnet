#nullable enable

namespace NS;

partial class Node1
{
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @Node1Property { get; } = global::Godot.StringName.CreateStaticFromAscii("Node1Property"u8);
    }
    internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
    {
        context.BindConstructor(() => new Node1());
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@Node1Property, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (Node1 __instance) =>
            {
                return __instance.@Node1Property;
            },
            static (Node1 __instance, int value) =>
            {
                __instance.@Node1Property = value;
            });
    }
}

#nullable enable

namespace NS;

partial class Node1
{
    partial class Node2
    {
        public new class PropertyName : global::Godot.Node.PropertyName
        {
            public static global::Godot.StringName @Node2Property { get; } = global::Godot.StringName.CreateStaticFromAscii("Node2Property"u8);
        }
        internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
        {
            context.BindConstructor(() => new Node2());
            context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@Node2Property, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
                static (Node2 __instance) =>
                {
                    return __instance.@Node2Property;
                },
                static (Node2 __instance, int value) =>
                {
                    __instance.@Node2Property = value;
                });
        }
    }
}

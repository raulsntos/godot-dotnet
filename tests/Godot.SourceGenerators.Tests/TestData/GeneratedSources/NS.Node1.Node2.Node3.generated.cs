#nullable enable

namespace NS;

partial class Node1
{
    partial class Node2
    {
        partial class Node3
        {
            public new class PropertyName : global::Godot.Node.PropertyName
            {
                public static global::Godot.StringName @Node3Property { get; } = global::Godot.StringName.CreateStaticFromAscii("Node3Property"u8);
            }
            internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
            {
                context.BindConstructor(() => new Node3());
                context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@Node3Property, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
                    {
                        Usage = global::Godot.PropertyUsageFlags.Default,
                    },
                    static (Node3 __instance) =>
                    {
                        return __instance.@Node3Property;
                    },
                    static (Node3 __instance, int value) =>
                    {
                        __instance.@Node3Property = value;
                    });
            }
        }
    }
}

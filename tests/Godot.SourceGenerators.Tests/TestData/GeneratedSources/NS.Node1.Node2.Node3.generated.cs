#nullable enable

namespace NS;

partial class Node1
{
    partial class Node2
    {
        partial class Node3
        {
            public new partial class MethodName : global::Godot.Node.MethodName
            {
            }
            public new partial class ConstantName : global::Godot.Node.ConstantName
            {
            }
            public new partial class PropertyName : global::Godot.Node.PropertyName
            {
                public static global::Godot.StringName @Node3Property { get; } = global::Godot.StringName.CreateStaticFromAscii("Node3Property"u8);
            }
            public new partial class SignalName : global::Godot.Node.SignalName
            {
            }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
            internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
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

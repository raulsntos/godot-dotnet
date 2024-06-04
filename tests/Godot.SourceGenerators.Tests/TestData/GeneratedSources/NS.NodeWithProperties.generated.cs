#nullable enable

namespace NS;

partial class NodeWithProperties
{
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @MyProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("MyProperty"u8);
        public static global::Godot.StringName @MyNamedProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("my_named_property"u8);
        public static global::Godot.StringName @MyPropertyWithDefaultValue { get; } = global::Godot.StringName.CreateStaticFromAscii("MyPropertyWithDefaultValue"u8);
    }
    internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
    {
        context.BindConstructor(() => new NodeWithProperties());
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@MyProperty, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithProperties __instance) =>
            {
                return __instance.@MyProperty;
            },
            static (NodeWithProperties __instance, int value) =>
            {
                __instance.@MyProperty = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@MyNamedProperty, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithProperties __instance) =>
            {
                return __instance.@MyNamedProperty;
            },
            static (NodeWithProperties __instance, int value) =>
            {
                __instance.@MyNamedProperty = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@MyPropertyWithDefaultValue, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithProperties __instance) =>
            {
                return __instance.@MyPropertyWithDefaultValue;
            },
            static (NodeWithProperties __instance, int value) =>
            {
                __instance.@MyPropertyWithDefaultValue = value;
            });
    }
}

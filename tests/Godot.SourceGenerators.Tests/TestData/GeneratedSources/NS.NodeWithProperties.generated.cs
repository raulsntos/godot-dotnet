#nullable enable

namespace NS;

partial class NodeWithProperties
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @MyProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("MyProperty"u8);
        public static global::Godot.StringName @MyNamedProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("my_named_property"u8);
        public static global::Godot.StringName @MyPropertyWithDefaultValue { get; } = global::Godot.StringName.CreateStaticFromAscii("MyPropertyWithDefaultValue"u8);
        public static global::Godot.StringName @myField { get; } = global::Godot.StringName.CreateStaticFromAscii("myField"u8);
        public static global::Godot.StringName @myNamedField { get; } = global::Godot.StringName.CreateStaticFromAscii("my_named_field"u8);
        public static global::Godot.StringName @myFieldWithDefaultValue { get; } = global::Godot.StringName.CreateStaticFromAscii("myFieldWithDefaultValue"u8);
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
    internal static void BindMethods(global::Godot.Bridge.ClassRegistrationContext context)
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
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@myField, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
        {
            Usage = global::Godot.PropertyUsageFlags.Default,
        },
            static (NodeWithProperties __instance) =>
            {
                return __instance.@myField;
            },
            static (NodeWithProperties __instance, int value) =>
            {
                __instance.@myField = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@myNamedField, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
        {
            Usage = global::Godot.PropertyUsageFlags.Default,
        },
            static (NodeWithProperties __instance) =>
            {
                return __instance.@myNamedField;
            },
            static (NodeWithProperties __instance, int value) =>
            {
                __instance.@myNamedField = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@myFieldWithDefaultValue, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
        {
            Usage = global::Godot.PropertyUsageFlags.Default,
        },
            static (NodeWithProperties __instance) =>
            {
                return __instance.@myFieldWithDefaultValue;
            },
            static (NodeWithProperties __instance, int value) =>
            {
                __instance.@myFieldWithDefaultValue = value;
            });
    }
}

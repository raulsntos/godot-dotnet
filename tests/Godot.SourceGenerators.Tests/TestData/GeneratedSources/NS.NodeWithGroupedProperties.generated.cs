#nullable enable

namespace NS;

partial class NodeWithGroupedProperties
{
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @UngroupedProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("UngroupedProperty"u8);
        public static global::Godot.StringName @TextSize { get; } = global::Godot.StringName.CreateStaticFromAscii("TextSize"u8);
        public static global::Godot.StringName @TextOutlineWidth { get; } = global::Godot.StringName.CreateStaticFromAscii("TextOutlineWidth"u8);
        public static global::Godot.StringName @TextOutlineColor { get; } = global::Godot.StringName.CreateStaticFromAscii("TextOutlineColor"u8);
        public static global::Godot.StringName @Speed { get; } = global::Godot.StringName.CreateStaticFromAscii("Speed"u8);
        public static global::Godot.StringName @Gravity { get; } = global::Godot.StringName.CreateStaticFromAscii("Gravity"u8);
        public static global::Godot.StringName @CanCollideWithEnemies { get; } = global::Godot.StringName.CreateStaticFromAscii("CanCollideWithEnemies"u8);
        public static global::Godot.StringName @CanCollideWithWalls { get; } = global::Godot.StringName.CreateStaticFromAscii("CanCollideWithWalls"u8);
    }
    internal static void BindMethods(global::Godot.Bridge.ClassDBRegistrationContext context)
    {
        context.BindConstructor(() => new NodeWithGroupedProperties());
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@UngroupedProperty, global::Godot.VariantType.String)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@UngroupedProperty;
            },
            static (NodeWithGroupedProperties __instance, string value) =>
            {
                __instance.@UngroupedProperty = value;
            });
        context.AddPropertyGroup("Text", "Text");
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@TextSize, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@TextSize;
            },
            static (NodeWithGroupedProperties __instance, int value) =>
            {
                __instance.@TextSize = value;
            });
        context.AddPropertySubgroup("Text Outline", "TextOutline");
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@TextOutlineWidth, global::Godot.VariantType.Float, global::Godot.Bridge.VariantTypeMetadata.Single)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@TextOutlineWidth;
            },
            static (NodeWithGroupedProperties __instance, float value) =>
            {
                __instance.@TextOutlineWidth = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@TextOutlineColor, global::Godot.VariantType.Float, global::Godot.Bridge.VariantTypeMetadata.Single)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@TextOutlineColor;
            },
            static (NodeWithGroupedProperties __instance, float value) =>
            {
                __instance.@TextOutlineColor = value;
            });
        context.AddPropertyGroup("Physics");
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@Speed, global::Godot.VariantType.Float, global::Godot.Bridge.VariantTypeMetadata.Single)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@Speed;
            },
            static (NodeWithGroupedProperties __instance, float value) =>
            {
                __instance.@Speed = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@Gravity, global::Godot.VariantType.Float, global::Godot.Bridge.VariantTypeMetadata.Single)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@Gravity;
            },
            static (NodeWithGroupedProperties __instance, float value) =>
            {
                __instance.@Gravity = value;
            });
        context.AddPropertySubgroup("Collisions");
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@CanCollideWithEnemies, global::Godot.VariantType.Bool)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@CanCollideWithEnemies;
            },
            static (NodeWithGroupedProperties __instance, bool value) =>
            {
                __instance.@CanCollideWithEnemies = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@CanCollideWithWalls, global::Godot.VariantType.Bool)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithGroupedProperties __instance) =>
            {
                return __instance.@CanCollideWithWalls;
            },
            static (NodeWithGroupedProperties __instance, bool value) =>
            {
                __instance.@CanCollideWithWalls = value;
            });
    }
}

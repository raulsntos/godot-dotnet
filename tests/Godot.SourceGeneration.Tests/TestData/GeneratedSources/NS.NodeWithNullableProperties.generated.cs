#nullable enable

namespace NS;

partial class @NodeWithNullableProperties
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @MyNullableString { get; } = global::Godot.StringName.CreateStaticFromAscii("MyNullableString"u8);
        public static global::Godot.StringName @MyNonNullableString { get; } = global::Godot.StringName.CreateStaticFromAscii("MyNonNullableString"u8);
        public static global::Godot.StringName @MyNullableObject { get; } = global::Godot.StringName.CreateStaticFromAscii("MyNullableObject"u8);
        public static global::Godot.StringName @MyNonNullableObject { get; } = global::Godot.StringName.CreateStaticFromAscii("MyNonNullableObject"u8);
        public static global::Godot.StringName @MyGodotArrayOfNullableObject { get; } = global::Godot.StringName.CreateStaticFromAscii("MyGodotArrayOfNullableObject"u8);
        public static global::Godot.StringName @MyGodotArrayOfNonNullableObject { get; } = global::Godot.StringName.CreateStaticFromAscii("MyGodotArrayOfNonNullableObject"u8);
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    {
        global::System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(global::Godot.Collections.GodotArray<global::Godot.GodotObject>).TypeHandle);
        context.BindConstructor(() => new global::NS.NodeWithNullableProperties());
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@MyNullableString, global::Godot.VariantType.String)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithNullableProperties __instance) =>
            {
                return __instance.@MyNullableString;
            },
            static (NodeWithNullableProperties __instance, string? value) =>
            {
                __instance.@MyNullableString = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@MyNonNullableString, global::Godot.VariantType.String)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithNullableProperties __instance) =>
            {
                return __instance.@MyNonNullableString;
            },
            static (NodeWithNullableProperties __instance, string value) =>
            {
                __instance.@MyNonNullableString = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@MyNullableObject, global::Godot.VariantType.Object)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
                ClassName = global::Godot.StringName.CreateStaticFromAscii("Object"u8),
            },
            static (NodeWithNullableProperties __instance) =>
            {
                return __instance.@MyNullableObject;
            },
            static (NodeWithNullableProperties __instance, global::Godot.GodotObject? value) =>
            {
                __instance.@MyNullableObject = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@MyNonNullableObject, global::Godot.VariantType.Object)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
                ClassName = global::Godot.StringName.CreateStaticFromAscii("Object"u8),
            },
            static (NodeWithNullableProperties __instance) =>
            {
                return __instance.@MyNonNullableObject;
            },
            static (NodeWithNullableProperties __instance, global::Godot.GodotObject value) =>
            {
                __instance.@MyNonNullableObject = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@MyGodotArrayOfNullableObject, global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "24/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithNullableProperties __instance) =>
            {
                return __instance.@MyGodotArrayOfNullableObject;
            },
            static (NodeWithNullableProperties __instance, global::Godot.Collections.GodotArray<global::Godot.GodotObject?> value) =>
            {
                __instance.@MyGodotArrayOfNullableObject = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@MyGodotArrayOfNonNullableObject, global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "24/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithNullableProperties __instance) =>
            {
                return __instance.@MyGodotArrayOfNonNullableObject;
            },
            static (NodeWithNullableProperties __instance, global::Godot.Collections.GodotArray<global::Godot.GodotObject> value) =>
            {
                __instance.@MyGodotArrayOfNonNullableObject = value;
            });
    }
}

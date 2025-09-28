#nullable enable

namespace NS;

partial class NodeWithExplicitInterfaceImplementations
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
        public static global::Godot.StringName @MyMethod { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethod"u8);
        public static global::Godot.StringName @MyMethodWithReturn { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethodWithReturn"u8);
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @MyProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("MyProperty"u8);
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    {
        context.BindConstructor(() => new global::NS.NodeWithExplicitInterfaceImplementations());
        context.BindMethod(MethodName.@MyMethod,
            static (NodeWithExplicitInterfaceImplementations __instance) =>
            {
                ((global::NS.IMyInterface)__instance).@MyMethod();
            });
        context.BindMethod(MethodName.@MyMethodWithReturn,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithExplicitInterfaceImplementations __instance) =>
            {
                return ((global::NS.IMyInterface)__instance).@MyMethodWithReturn();
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@MyProperty, global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithExplicitInterfaceImplementations __instance) =>
            {
                return ((global::NS.IMyInterface)__instance).@MyProperty;
            },
            static (NodeWithExplicitInterfaceImplementations __instance, int value) =>
            {
                ((global::NS.IMyInterface)__instance).@MyProperty = value;
            });
    }
}

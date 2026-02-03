#nullable enable

namespace NS;

partial class NodeWithSignals
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
        public static global::Godot.StringName @MySignal { get; } = global::Godot.StringName.CreateStaticFromAscii("MySignal"u8);
        public static global::Godot.StringName @MyNamedSignal { get; } = global::Godot.StringName.CreateStaticFromAscii("my_named_signal"u8);
        public static global::Godot.StringName @MySignalWithParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MySignalWithParameters"u8);
        public static global::Godot.StringName @MySignalWithNamedParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MySignalWithNamedParameters"u8);
        public static global::Godot.StringName @MySignalWithOptionalParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MySignalWithOptionalParameters"u8);
    }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    {
        context.BindConstructor(() => new global::NS.NodeWithSignals());
        context.BindSignal(new global::Godot.Bridge.SignalInfo(SignalName.@MySignal));
        context.BindSignal(new global::Godot.Bridge.SignalInfo(SignalName.@MyNamedSignal));
        context.BindSignal(new global::Godot.Bridge.SignalInfo(SignalName.@MySignalWithParameters)
        {
            Parameters =
            {
                new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("a"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
                new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("b"u8), global::Godot.VariantType.Float, global::Godot.Bridge.VariantTypeMetadata.Single)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
                new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("c"u8), global::Godot.VariantType.String)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
            },
        });
        context.BindSignal(new global::Godot.Bridge.SignalInfo(SignalName.@MySignalWithNamedParameters)
        {
            Parameters =
            {
                new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("my_number"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
                new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("my_string"u8), global::Godot.VariantType.String)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
            },
        });
        context.BindSignal(new global::Godot.Bridge.SignalInfo(SignalName.@MySignalWithOptionalParameters)
        {
            Parameters =
            {
                new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("requiredParameter"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
                new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("optionalParameter"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32, 42)
                {
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
            },
        });
    }
}

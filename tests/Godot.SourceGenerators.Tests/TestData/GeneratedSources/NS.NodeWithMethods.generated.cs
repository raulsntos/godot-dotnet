#nullable enable

namespace NS;

partial class NodeWithMethods
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
        public static global::Godot.StringName @MyMethod { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethod"u8);
        public static global::Godot.StringName @MyNamedMethod { get; } = global::Godot.StringName.CreateStaticFromAscii("my_named_method"u8);
        public static global::Godot.StringName @MyMethodWithReturn { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethodWithReturn"u8);
        public static global::Godot.StringName @MyMethodWithParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethodWithParameters"u8);
        public static global::Godot.StringName @MyMethodWithReturnAndParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethodWithReturnAndParameters"u8);
        public static global::Godot.StringName @MyMethodWithNamedParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethodWithNamedParameters"u8);
        public static global::Godot.StringName @MyMethodWithOptionalParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethodWithOptionalParameters"u8);
        public static global::Godot.StringName @MyMethodWithReservedKeyword { get; } = global::Godot.StringName.CreateStaticFromAscii("MyMethodWithReservedKeyword"u8);
        public static global::Godot.StringName @MyStaticMethod { get; } = global::Godot.StringName.CreateStaticFromAscii("MyStaticMethod"u8);
        public static global::Godot.StringName @MyStaticMethodWithParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyStaticMethodWithParameters"u8);
        public static global::Godot.StringName @MyStaticMethodWithReturn { get; } = global::Godot.StringName.CreateStaticFromAscii("MyStaticMethodWithReturn"u8);
        public static global::Godot.StringName @MyStaticMethodWithReturnAndParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyStaticMethodWithReturnAndParameters"u8);
        public static global::Godot.StringName @MyVirtualMethod { get; } = global::Godot.StringName.CreateStaticFromAscii("MyVirtualMethod"u8);
        public static global::Godot.StringName @MyVirtualMethodWithParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyVirtualMethodWithParameters"u8);
        public static global::Godot.StringName @MyVirtualMethodWithReturn { get; } = global::Godot.StringName.CreateStaticFromAscii("MyVirtualMethodWithReturn"u8);
        public static global::Godot.StringName @MyVirtualMethodWithReturnAndParameters { get; } = global::Godot.StringName.CreateStaticFromAscii("MyVirtualMethodWithReturnAndParameters"u8);
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    {
        context.BindConstructor(() => new NodeWithMethods());
        context.BindMethod(MethodName.@MyMethod,
            static (NodeWithMethods __instance) =>
            {
                __instance.@MyMethod();
            });
        context.BindMethod(MethodName.@MyNamedMethod,
            static (NodeWithMethods __instance) =>
            {
                __instance.@MyNamedMethod();
            });
        context.BindMethod(MethodName.@MyMethodWithReturn,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithMethods __instance) =>
            {
                return __instance.@MyMethodWithReturn();
            });
        context.BindMethod(MethodName.@MyMethodWithParameters,
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
            static (NodeWithMethods __instance, int @a, float @b, string @c) =>
            {
                __instance.@MyMethodWithParameters(@a, @b, @c);
            });
        context.BindMethod(MethodName.@MyMethodWithReturnAndParameters,
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
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithMethods __instance, int @a, float @b, string @c) =>
            {
                return __instance.@MyMethodWithReturnAndParameters(@a, @b, @c);
            });
        context.BindMethod(MethodName.@MyMethodWithNamedParameters,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("my_number"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("my_string"u8), global::Godot.VariantType.String)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithMethods __instance, int @myNumber, string @myString) =>
            {
                __instance.@MyMethodWithNamedParameters(@myNumber, @myString);
            });
        context.BindMethod(MethodName.@MyMethodWithOptionalParameters,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("requiredParameter"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("optionalParameter"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32, 42)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithMethods __instance, int @requiredParameter, int @optionalParameter) =>
            {
                __instance.@MyMethodWithOptionalParameters(@requiredParameter, @optionalParameter);
            });
        context.BindMethod(MethodName.@MyMethodWithReservedKeyword,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("int"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithMethods __instance, int @int) =>
            {
                __instance.@MyMethodWithReservedKeyword(@int);
            });
        context.BindStaticMethod(MethodName.@MyStaticMethod,
            static () =>
            {
                NodeWithMethods.@MyStaticMethod();
            });
        context.BindStaticMethod(MethodName.@MyStaticMethodWithParameters,
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
            static (int @a, float @b, string @c) =>
            {
                NodeWithMethods.@MyStaticMethodWithParameters(@a, @b, @c);
            });
        context.BindStaticMethod(MethodName.@MyStaticMethodWithReturn,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static () =>
            {
                return NodeWithMethods.@MyStaticMethodWithReturn();
            });
        context.BindStaticMethod(MethodName.@MyStaticMethodWithReturnAndParameters,
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
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (int @a, float @b, string @c) =>
            {
                return NodeWithMethods.@MyStaticMethodWithReturnAndParameters(@a, @b, @c);
            });
        context.BindVirtualMethod(MethodName.@MyVirtualMethod);
        context.BindVirtualMethod<int, float, string>(MethodName.@MyVirtualMethodWithParameters,
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
            });
        context.BindVirtualMethod<int>(MethodName.@MyVirtualMethodWithReturn,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            });
        context.BindVirtualMethod<int, float, string, int>(MethodName.@MyVirtualMethodWithReturnAndParameters,
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
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            });
    }
}

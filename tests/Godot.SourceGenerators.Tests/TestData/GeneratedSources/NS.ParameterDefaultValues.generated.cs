#nullable enable

namespace NS;

partial class ParameterDefaultValues
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
        public static global::Godot.StringName @Method1 { get; } = global::Godot.StringName.CreateStaticFromAscii("Method1"u8);
        public static global::Godot.StringName @Method2 { get; } = global::Godot.StringName.CreateStaticFromAscii("Method2"u8);
        public static global::Godot.StringName @Method3 { get; } = global::Godot.StringName.CreateStaticFromAscii("Method3"u8);
        public static global::Godot.StringName @Method4 { get; } = global::Godot.StringName.CreateStaticFromAscii("Method4"u8);
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
    internal static void BindMethods(global::Godot.Bridge.ClassRegistrationContext context)
    {
        context.BindConstructor(() => new ParameterDefaultValues());
        context.BindMethod(MethodName.@Method1,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterByte"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Byte, 42)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterBoolean"u8), global::Godot.VariantType.Bool, global::Godot.Bridge.VariantTypeMetadata.None, true)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterSByte"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.SByte, 42)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (ParameterDefaultValues __instance, byte @parameterByte, bool @parameterBoolean, sbyte @parameterSByte) =>
            {
                __instance.@Method1(@parameterByte, @parameterBoolean, @parameterSByte);
            });
        context.BindMethod(MethodName.@Method2,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterInt16"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int16, -42)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterInt32"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int32, -42)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterInt64"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Int64, -42L)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterUInt16"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.UInt16, 42)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterUInt32"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.UInt32, 42U)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterUInt64"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.UInt64, 42UL)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (ParameterDefaultValues __instance, short @parameterInt16, int @parameterInt32, long @parameterInt64, ushort @parameterUInt16, uint @parameterUInt32, ulong @parameterUInt64) =>
            {
                __instance.@Method2(@parameterInt16, @parameterInt32, @parameterInt64, @parameterUInt16, @parameterUInt32, @parameterUInt64);
            });
        context.BindMethod(MethodName.@Method3,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterSingle"u8), global::Godot.VariantType.Float, global::Godot.Bridge.VariantTypeMetadata.Single, 4.2f)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterDouble"u8), global::Godot.VariantType.Float, global::Godot.Bridge.VariantTypeMetadata.Double, 4.2)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (ParameterDefaultValues __instance, float @parameterSingle, double @parameterDouble) =>
            {
                __instance.@Method3(@parameterSingle, @parameterDouble);
            });
        context.BindMethod(MethodName.@Method4,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterChar"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.Char16, 'a')
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterString"u8), global::Godot.VariantType.String, global::Godot.Bridge.VariantTypeMetadata.None, "hello")
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterEnum"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.None, (long)(MyEnum.A))
            {
                Hint = global::Godot.PropertyHint.Enum,
                HintString = "A,B,C",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterFlagsEnum"u8), global::Godot.VariantType.Int, global::Godot.Bridge.VariantTypeMetadata.None, (long)(MyFlagsEnum.A | MyFlagsEnum.B))
            {
                Hint = global::Godot.PropertyHint.Flags,
                HintString = "A:0,B:1,C:2",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterAabb"u8), global::Godot.VariantType.Aabb, global::Godot.Bridge.VariantTypeMetadata.None, default)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterBasis"u8), global::Godot.VariantType.Basis, global::Godot.Bridge.VariantTypeMetadata.None, default)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("parameterNodePath"u8), global::Godot.VariantType.NodePath, global::Godot.Bridge.VariantTypeMetadata.None, default)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (ParameterDefaultValues __instance, char @parameterChar, string @parameterString, global::NS.ParameterDefaultValues.MyEnum @parameterEnum, global::NS.ParameterDefaultValues.MyFlagsEnum @parameterFlagsEnum, global::Godot.Aabb @parameterAabb, global::Godot.Basis @parameterBasis, global::Godot.NodePath @parameterNodePath) =>
            {
                __instance.@Method4(@parameterChar, @parameterString, @parameterEnum, @parameterFlagsEnum, @parameterAabb, @parameterBasis, @parameterNodePath);
            });
    }
}

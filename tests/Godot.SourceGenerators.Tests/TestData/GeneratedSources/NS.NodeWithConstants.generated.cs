#nullable enable

namespace NS;

partial class NodeWithConstants
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
        public static global::Godot.StringName @MyConstant { get; } = global::Godot.StringName.CreateStaticFromAscii("MyConstant"u8);
        public static global::Godot.StringName @MyNamedConstant { get; } = global::Godot.StringName.CreateStaticFromAscii("my_named_constant"u8);
        public static global::Godot.StringName @MyEnum { get; } = global::Godot.StringName.CreateStaticFromAscii("MyEnum"u8);
        public static global::Godot.StringName @MyEnumRed { get; } = global::Godot.StringName.CreateStaticFromAscii("Red"u8);
        public static global::Godot.StringName @MyEnumGreen { get; } = global::Godot.StringName.CreateStaticFromAscii("Green"u8);
        public static global::Godot.StringName @MyEnumBlue { get; } = global::Godot.StringName.CreateStaticFromAscii("Blue"u8);
        public static global::Godot.StringName @MyNamedEnum { get; } = global::Godot.StringName.CreateStaticFromAscii("MY_NAMED_ENUM"u8);
        public static global::Godot.StringName @MyNamedEnumRed { get; } = global::Godot.StringName.CreateStaticFromAscii("Red"u8);
        public static global::Godot.StringName @MyNamedEnumGreen { get; } = global::Godot.StringName.CreateStaticFromAscii("Green"u8);
        public static global::Godot.StringName @MyNamedEnumBlue { get; } = global::Godot.StringName.CreateStaticFromAscii("Blue"u8);
        public static global::Godot.StringName @MyEnumWithNamedConstants { get; } = global::Godot.StringName.CreateStaticFromAscii("MyEnumWithNamedConstants"u8);
        public static global::Godot.StringName @MyEnumWithNamedConstantsRed { get; } = global::Godot.StringName.CreateStaticFromAscii("RED"u8);
        public static global::Godot.StringName @MyEnumWithNamedConstantsGreen { get; } = global::Godot.StringName.CreateStaticFromAscii("GREEN"u8);
        public static global::Godot.StringName @MyEnumWithNamedConstantsBlue { get; } = global::Godot.StringName.CreateStaticFromAscii("BLUE"u8);
        public static global::Godot.StringName @MyFlagsEnum { get; } = global::Godot.StringName.CreateStaticFromAscii("MyFlagsEnum"u8);
        public static global::Godot.StringName @MyFlagsEnumFire { get; } = global::Godot.StringName.CreateStaticFromAscii("Fire"u8);
        public static global::Godot.StringName @MyFlagsEnumWater { get; } = global::Godot.StringName.CreateStaticFromAscii("Water"u8);
        public static global::Godot.StringName @MyFlagsEnumEarth { get; } = global::Godot.StringName.CreateStaticFromAscii("Earth"u8);
        public static global::Godot.StringName @MyFlagsEnumWind { get; } = global::Godot.StringName.CreateStaticFromAscii("Wind"u8);
        public static global::Godot.StringName @MyFlagsEnumFireAndWater { get; } = global::Godot.StringName.CreateStaticFromAscii("FireAndWater"u8);
        public static global::Godot.StringName @MyNamedFlagsEnum { get; } = global::Godot.StringName.CreateStaticFromAscii("MY_NAMED_FLAGS_ENUM"u8);
        public static global::Godot.StringName @MyNamedFlagsEnumFire { get; } = global::Godot.StringName.CreateStaticFromAscii("Fire"u8);
        public static global::Godot.StringName @MyNamedFlagsEnumWater { get; } = global::Godot.StringName.CreateStaticFromAscii("Water"u8);
        public static global::Godot.StringName @MyNamedFlagsEnumEarth { get; } = global::Godot.StringName.CreateStaticFromAscii("Earth"u8);
        public static global::Godot.StringName @MyNamedFlagsEnumWind { get; } = global::Godot.StringName.CreateStaticFromAscii("Wind"u8);
        public static global::Godot.StringName @MyNamedFlagsEnumFireAndWater { get; } = global::Godot.StringName.CreateStaticFromAscii("FireAndWater"u8);
        public static global::Godot.StringName @MyFlagsEnumWithNamedConstants { get; } = global::Godot.StringName.CreateStaticFromAscii("MyFlagsEnumWithNamedConstants"u8);
        public static global::Godot.StringName @MyFlagsEnumWithNamedConstantsFire { get; } = global::Godot.StringName.CreateStaticFromAscii("FIRE"u8);
        public static global::Godot.StringName @MyFlagsEnumWithNamedConstantsWater { get; } = global::Godot.StringName.CreateStaticFromAscii("WATER"u8);
        public static global::Godot.StringName @MyFlagsEnumWithNamedConstantsEarth { get; } = global::Godot.StringName.CreateStaticFromAscii("EARTH"u8);
        public static global::Godot.StringName @MyFlagsEnumWithNamedConstantsWind { get; } = global::Godot.StringName.CreateStaticFromAscii("WIND"u8);
        public static global::Godot.StringName @MyFlagsEnumWithNamedConstantsFireAndWater { get; } = global::Godot.StringName.CreateStaticFromAscii("FIRE_AND_WATER"u8);
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
    internal static void BindMethods(global::Godot.Bridge.ClassRegistrationContext context)
    {
        context.BindConstructor(() => new NodeWithConstants());
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyConstant, (long)(@MyConstant)));
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedConstant, (long)(@MyNamedConstant)));
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyEnumRed, (long)(@MyEnum.@Red))
        {
            EnumName = ConstantName.@MyEnum,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyEnumGreen, (long)(@MyEnum.@Green))
        {
            EnumName = ConstantName.@MyEnum,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyEnumBlue, (long)(@MyEnum.@Blue))
        {
            EnumName = ConstantName.@MyEnum,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedEnumRed, (long)(@MyNamedEnum.@Red))
        {
            EnumName = ConstantName.@MyNamedEnum,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedEnumGreen, (long)(@MyNamedEnum.@Green))
        {
            EnumName = ConstantName.@MyNamedEnum,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedEnumBlue, (long)(@MyNamedEnum.@Blue))
        {
            EnumName = ConstantName.@MyNamedEnum,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyEnumWithNamedConstantsRed, (long)(@MyEnumWithNamedConstants.@Red))
        {
            EnumName = ConstantName.@MyEnumWithNamedConstants,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyEnumWithNamedConstantsGreen, (long)(@MyEnumWithNamedConstants.@Green))
        {
            EnumName = ConstantName.@MyEnumWithNamedConstants,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyEnumWithNamedConstantsBlue, (long)(@MyEnumWithNamedConstants.@Blue))
        {
            EnumName = ConstantName.@MyEnumWithNamedConstants,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumFire, (long)(@MyFlagsEnum.@Fire))
        {
            EnumName = ConstantName.@MyFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumWater, (long)(@MyFlagsEnum.@Water))
        {
            EnumName = ConstantName.@MyFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumEarth, (long)(@MyFlagsEnum.@Earth))
        {
            EnumName = ConstantName.@MyFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumWind, (long)(@MyFlagsEnum.@Wind))
        {
            EnumName = ConstantName.@MyFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumFireAndWater, (long)(@MyFlagsEnum.@FireAndWater))
        {
            EnumName = ConstantName.@MyFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedFlagsEnumFire, (long)(@MyNamedFlagsEnum.@Fire))
        {
            EnumName = ConstantName.@MyNamedFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedFlagsEnumWater, (long)(@MyNamedFlagsEnum.@Water))
        {
            EnumName = ConstantName.@MyNamedFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedFlagsEnumEarth, (long)(@MyNamedFlagsEnum.@Earth))
        {
            EnumName = ConstantName.@MyNamedFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedFlagsEnumWind, (long)(@MyNamedFlagsEnum.@Wind))
        {
            EnumName = ConstantName.@MyNamedFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyNamedFlagsEnumFireAndWater, (long)(@MyNamedFlagsEnum.@FireAndWater))
        {
            EnumName = ConstantName.@MyNamedFlagsEnum,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumWithNamedConstantsFire, (long)(@MyFlagsEnumWithNamedConstants.@Fire))
        {
            EnumName = ConstantName.@MyFlagsEnumWithNamedConstants,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumWithNamedConstantsWater, (long)(@MyFlagsEnumWithNamedConstants.@Water))
        {
            EnumName = ConstantName.@MyFlagsEnumWithNamedConstants,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumWithNamedConstantsEarth, (long)(@MyFlagsEnumWithNamedConstants.@Earth))
        {
            EnumName = ConstantName.@MyFlagsEnumWithNamedConstants,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumWithNamedConstantsWind, (long)(@MyFlagsEnumWithNamedConstants.@Wind))
        {
            EnumName = ConstantName.@MyFlagsEnumWithNamedConstants,
            IsFlagsEnum = true,
        });
        context.BindConstant(new global::Godot.Bridge.ConstantInfo(ConstantName.@MyFlagsEnumWithNamedConstantsFireAndWater, (long)(@MyFlagsEnumWithNamedConstants.@FireAndWater))
        {
            EnumName = ConstantName.@MyFlagsEnumWithNamedConstants,
            IsFlagsEnum = true,
        });
    }
}

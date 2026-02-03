using System;
using Godot;

namespace NS;

[GodotClass]
public partial class NodeWithConstants : Node
{
    public const int UnexposedConstant = 42;

    [BindConstant]
    public const int MyConstant = 42;

    [BindConstant(Name = "my_named_constant")]
    public const int MyNamedConstant = 42;

    [BindEnum]
    public enum MyEnum
    {
        Red,
        Green,
        Blue,
    }

    [BindEnum(Name = "MY_NAMED_ENUM")]
    public enum MyNamedEnum
    {
        Red,
        Green,
        Blue,
    }

    [BindEnum]
    public enum MyEnumWithNamedConstants
    {
        [BindConstant(Name = "RED")]
        Red,

        [BindConstant(Name = "GREEN")]
        Green,

        [BindConstant(Name = "BLUE")]
        Blue,
    }

    [BindEnum, Flags]
    public enum MyFlagsEnum
    {
        Fire = 1 << 1,
        Water = 1 << 2,
        Earth = 1 << 3,
        Wind = 1 << 4,

        FireAndWater = Fire | Water,
    }

    [BindEnum(Name = "MY_NAMED_FLAGS_ENUM"), Flags]
    public enum MyNamedFlagsEnum
    {
        Fire = 1 << 1,
        Water = 1 << 2,
        Earth = 1 << 3,
        Wind = 1 << 4,

        FireAndWater = Fire | Water,
    }

    [BindEnum, Flags]
    public enum MyFlagsEnumWithNamedConstants
    {
        [BindConstant(Name = "FIRE")]
        Fire = 1 << 1,
        [BindConstant(Name = "WATER")]
        Water = 1 << 2,
        [BindConstant(Name = "EARTH")]
        Earth = 1 << 3,
        [BindConstant(Name = "WIND")]
        Wind = 1 << 4,

        [BindConstant(Name = "FIRE_AND_WATER")]
        FireAndWater = Fire | Water,
    }
}

using System;
using Godot;

namespace NS;

[GodotClass]
public partial class ParameterDefaultValues : Node
{
    public enum MyEnum { A, B, C }
    [Flags] public enum MyFlagsEnum { A, B, C }

    [BindMethod]
    public void Method1(
        byte parameterByte = 42,
        bool parameterBoolean = true,
        sbyte parameterSByte = 42
    )
    { }

    [BindMethod]
    public void Method2(
        short parameterInt16 = -42,
        int parameterInt32 = -42,
        long parameterInt64 = -42L,
        ushort parameterUInt16 = 42,
        uint parameterUInt32 = 42U,
        ulong parameterUInt64 = 42UL
    )
    { }

    [BindMethod]
    public void Method3(
        float parameterSingle = 4.2f,
        double parameterDouble = 4.2
    )
    { }

    [BindMethod]
    public void Method4(
        char parameterChar = 'a',
        string parameterString = "hello",
        MyEnum parameterEnum = MyEnum.A,
        MyFlagsEnum parameterFlagsEnum = MyFlagsEnum.A | MyFlagsEnum.B,
        Aabb parameterAabb = new Aabb(),
        Basis parameterBasis = default,
        NodePath parameterNodePath = null!
    )
    { }
}

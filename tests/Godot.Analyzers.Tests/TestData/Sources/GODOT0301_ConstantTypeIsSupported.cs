using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindConstant]
    public const byte MyByteConst = 42;

    [BindConstant]
    public const sbyte MySByteConst = 42;

    [BindConstant]
    public const short MyShortConst = 42;

    [BindConstant]
    public const ushort MyUShortConst = 42;

    [BindConstant]
    public const int MyIntConst = 42;

    [BindConstant]
    public const uint MyUIntConst = 42;

    [BindConstant]
    public const long MyLongConst = 42;

    [BindConstant]
    public const ulong MyULongConst = 42;

    [BindConstant]
    public const {|GODOT0301:float|} MyFloatConst = 42.0f;

    [BindConstant]
    public const {|GODOT0301:double|} MyDoubleConst = 42.0;

    [BindConstant]
    public const {|GODOT0301:char|} MyCharConst = 'A';

    [BindConstant]
    public const {|GODOT0301:string|} MyStringConst = "Hello, World!";
}

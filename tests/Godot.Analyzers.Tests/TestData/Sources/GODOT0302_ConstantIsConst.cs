using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindConstant]
    public const int MyConst = 42;

    [BindConstant]
    public readonly int {|GODOT0302:MyReadOnlyField|} = 42;

    [BindConstant]
    public int {|GODOT0302:MyField|} = 42;

    [BindConstant]
    public static readonly int {|GODOT0302:MyStaticReadOnlyField|} = 42;

    [BindConstant]
    public static int {|GODOT0302:MyStaticField|} = 42;
}

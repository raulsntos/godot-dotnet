using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindProperty]
    public int MyProperty { get; set; }

    [BindProperty]
    public static int {|GODOT0502:MyStaticProperty|} { get; set; }
}

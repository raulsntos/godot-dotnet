using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindProperty]
    public int MyField;

    [BindProperty]
    public readonly int {|GODOT0504:MyConstField|} = 42;
}

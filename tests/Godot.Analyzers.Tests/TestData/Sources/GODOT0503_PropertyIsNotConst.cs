using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindProperty]
    public int MyField;

    [BindProperty]
    public const int {|GODOT0503:MyConstField|} = 42;
}

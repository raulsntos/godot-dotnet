using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindProperty]
    public int {|GODOT0506:this|}[int index] { get => default; set { } }
}

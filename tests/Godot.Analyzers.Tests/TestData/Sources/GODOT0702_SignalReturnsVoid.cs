using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [Signal]
    public delegate void MySignalEventHandler();

    [Signal]
    public delegate int {|GODOT0702:MySignalReturningIntEventHandler|}();
}

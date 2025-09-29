using Godot;

public partial class MyNode : Node
{
    public new void _Process(double delta) { }

    public override void {|CS0507:_PhysicsProcess|}(double delta) { }

    public override void {|CS0507:_UnhandledInput|}(InputEvent @event) { }
}

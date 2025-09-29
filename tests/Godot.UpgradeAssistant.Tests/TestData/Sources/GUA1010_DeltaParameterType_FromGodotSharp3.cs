using Godot;

public partial class MyNode : Node
{
    public new void _Process(float delta) { }

    public override void {|GUA1010:_PhysicsProcess|}(float delta) { }

    public override void _UnhandledInput(InputEvent @event) { }
}

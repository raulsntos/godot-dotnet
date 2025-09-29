using Godot;

public partial class MyNode : Node
{
    protected new void _Process(double delta) { }

    protected override void _PhysicsProcess(double delta) { }

    protected override void _UnhandledInput(InputEvent @event) { }
}

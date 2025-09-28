using Godot;

namespace NS;

[GodotClass]
[BindConstructor(typeof(MyNodeBuilder), nameof(MyNodeBuilder.CreateMyNode))]
public partial class MyNodeWithBuilder : Node { }

public static class MyNodeBuilder
{
    public static MyNodeWithBuilder CreateMyNode() => new();
}

using Godot;

[GodotClass]
[{|#0:BindConstructor(typeof(MyNode1), nameof(Create))|}]
public partial class MyNode1 : Node
{
    public const int Create = 42;
}

[GodotClass]
[{|#1:BindConstructor(typeof(MyNode2Builder), "Create")|}]
public partial class MyNode2 : Node { }

public static class MyNode2Builder { }

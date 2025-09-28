using Godot;

[GodotClass]
[{|#0:BindConstructor(typeof(MyNode1), nameof(Create))|}]
public partial class MyNode1 : Node
{
    public static MyNode1 Create(int value) => new MyNode1();
}

[GodotClass]
[{|#1:BindConstructor(typeof(MyNode2Builder), nameof(MyNode2Builder.Create))|}]
public partial class MyNode2 : Node { }

public static class MyNode2Builder
{
    public static MyNode2 Create(int value) => new MyNode2();
}

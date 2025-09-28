using Godot;

[GodotClass]
[{|#0:BindConstructor(typeof(MyNode1), nameof(Create))|}]
public partial class MyNode1 : Node
{
    public MyNode1 Create() => new MyNode1();
}

[GodotClass]
[{|#1:BindConstructor(typeof(MyNode2Builder), nameof(MyNode2Builder.Create))|}]
public partial class MyNode2 : Node { }

public class MyNode2Builder
{
    public MyNode2 Create() => new MyNode2();
}

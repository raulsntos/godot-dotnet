using Godot;

[GodotClass]
[{|#0:BindConstructor(typeof(MyNode1), nameof(Create))|}]
public partial class MyNode1 : Node
{
    public static MyNode1 Create<T>() => new MyNode1();
}

[GodotClass]
[{|#1:BindConstructor(typeof(MyNode2Builder), nameof(MyNode2Builder.Create))|}]
public partial class MyNode2 : Node { }

public static class MyNode2Builder
{
    public static MyNode2 Create<T>() => new MyNode2();
}

[GodotClass]
[{|#2:BindConstructor(typeof(MyNode3Builder<>), nameof(MyNode3Builder<int>.Create))|}]
public partial class MyNode3 : Node { }

public static class MyNode3Builder<T>
{
    public static MyNode3 Create() => new MyNode3();
}

[GodotClass]
[BindConstructor(typeof(MyNode4Builder<int>), nameof(MyNode4Builder<int>.Create))]
public partial class MyNode4 : Node { }

public static class MyNode4Builder<T>
{
    public static MyNode4 Create() => new MyNode4();
}

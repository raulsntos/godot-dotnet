using Godot;

[GodotClass]
[BindConstructor(typeof(MyNode1), nameof(Create))]
public partial class MyNode1 : Node
{
    private static MyNode1 Create() => new MyNode1();
}

[GodotClass]
[BindConstructor(typeof(MyNode2Builder), nameof(MyNode2Builder.Create))]
public partial class MyNode2 : Node { }

public static class MyNode2Builder
{
    internal static MyNode2 Create() => new MyNode2();
}

[GodotClass]
[{|#0:BindConstructor(typeof(MyNode3Builder), "Create")|}]
public partial class MyNode3 : Node { }

public static class MyNode3Builder
{
    private static MyNode3 Create() => new MyNode3();
}

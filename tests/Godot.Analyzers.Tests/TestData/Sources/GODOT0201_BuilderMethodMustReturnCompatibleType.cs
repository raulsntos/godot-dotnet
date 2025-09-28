using Godot;

[GodotClass]
[{|#0:BindConstructor(typeof(MyNode1), nameof(Create))|}]
public partial class MyNode1 : Node
{
    public static object Create() => new MyNode1();
}

[GodotClass]
[{|#1:BindConstructor(typeof(MyNode2Builder), nameof(MyNode2Builder.Create))|}]
public partial class MyNode2 : Node { }

public static class MyNode2Builder
{
    public static object Create() => new MyNode2();
}

[GodotClass]
[{|#2:BindConstructor(typeof(MyNode3), nameof(Create))|}]
public partial class MyNode3 : Node
{
    public static Node Create() => new MyNode3();
}

[GodotClass]
[{|#3:BindConstructor(typeof(MyNode4Builder), nameof(MyNode4Builder.Create))|}]
public partial class MyNode4 : Node { }

public static class MyNode4Builder
{
    public static Node Create() => new MyNode4();
}

[GodotClass]
[BindConstructor(typeof(BaseNode1), nameof(Create))]
public partial class BaseNode1 : Node
{
    public static DerivedNode1 Create() => new DerivedNode1();
}

public class DerivedNode1 : BaseNode1 { }

[GodotClass]
[BindConstructor(typeof(BaseNode2Builder), nameof(BaseNode2Builder.Create))]
public partial class BaseNode2 : Node { }

public static class BaseNode2Builder
{
    public static DerivedNode2 Create() => new DerivedNode2();
}

public class DerivedNode2 : BaseNode2 { }

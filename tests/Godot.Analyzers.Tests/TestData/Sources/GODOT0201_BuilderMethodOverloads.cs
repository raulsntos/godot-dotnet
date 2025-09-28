using Godot;

[GodotClass]
[BindConstructor(typeof(MyNode1), nameof(Create))]
public partial class MyNode1 : Node
{
    // Overload 1: Has generic type parameter.
    public static MyNode1 Create<T>() => new MyNode1();
    // Overload 2: Has parameters.
    public static MyNode1 Create(int value) => new MyNode1();
    // Overload 3: Valid.
    public static MyNode1 Create() => new MyNode1();
}

[GodotClass]
[BindConstructor(typeof(MyNode2Builder), nameof(MyNode2Builder.Create))]
public partial class MyNode2 : Node { }

public class MyNode2Builder
{
    // Overload 1: Has generic type parameter.
    public static MyNode2 Create<T>() => new MyNode2();
    // Overload 2: Has parameters.
    public static MyNode2 Create(int value) => new MyNode2();
    // Overload 3: Valid.
    public static MyNode2 Create() => new MyNode2();
}

[GodotClass]
[{|#0:BindConstructor(typeof(MyNode3), nameof(Create))|}]
public partial class MyNode3 : Node
{
    // Overload 1: Has generic type parameter.
    public static MyNode3 Create<T>() => new MyNode3();
    // Overload 2: Has parameters.
    public static MyNode3 Create(int value) => new MyNode3();
    // Overload 3: Not static.
    public MyNode3 Create() => new MyNode3();
}

[GodotClass]
[{|#1:BindConstructor(typeof(MyNode4Builder), nameof(MyNode4Builder.Create))|}]
public partial class MyNode4 : Node { }

public class MyNode4Builder
{
    // Overload 1: Has generic type parameter.
    public static MyNode4 Create<T>() => new MyNode4();
    // Overload 2: Has parameters.
    public static MyNode4 Create(int value) => new MyNode4();
    // Overload 3: Not static.
    public MyNode4 Create() => new MyNode4();
}

[GodotClass]
[{|#2:BindConstructor(typeof(MyNode5), nameof(Create))|}]
public partial class MyNode5 : Node
{
    // Overload 1: Has generic type parameter.
    public static MyNode5 Create<T>() => new MyNode5();
    // Overload 2: Has parameters.
    public static MyNode5 Create(int value) => new MyNode5();
    // Overload 3: Invalid return type.
    public static object Create() => new MyNode5();
}

[GodotClass]
[{|#3:BindConstructor(typeof(MyNode6Builder), nameof(MyNode6Builder.Create))|}]
public partial class MyNode6 : Node { }

public class MyNode6Builder
{
    // Overload 1: Has generic type parameter.
    public static MyNode6 Create<T>() => new MyNode6();
    // Overload 2: Has parameters.
    public static MyNode6 Create(int value) => new MyNode6();
    // Overload 3: Invalid return type.
    public static object Create() => new MyNode6();
}

[GodotClass]
[{|#4:BindConstructor(typeof(MyNode7), nameof(Create))|}]
public partial class MyNode7 : Node
{
    // Overload 1: Has generic type parameter.
    public static MyNode7 Create<T>() => new MyNode7();
    // Overload 2: Has parameters.
    public static MyNode7 Create(int value) => new MyNode7();
}

[GodotClass]
[{|#5:BindConstructor(typeof(MyNode8Builder), nameof(MyNode8Builder.Create))|}]
public partial class MyNode8 : Node { }

public class MyNode8Builder
{
    // Overload 1: Has generic type parameter.
    public static MyNode8 Create<T>() => new MyNode8();
    // Overload 2: Has parameters.
    public static MyNode8 Create(int value) => new MyNode8();
}

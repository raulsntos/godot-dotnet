using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindConstant]
    public const int MyConst = 42;

    [BindEnum]
    public enum MyEnum { A, B, C }

    [BindProperty]
    public int MyField;

    [PropertyGroup("My Group")]
    [PropertySubgroup("My Subgroup")]
    [BindProperty]
    public int MyProperty { get; set; }

    [BindMethod]
    public void MyMethod() { }

    [Signal]
    public delegate void MySignalEventHandler();
}

public partial class MyNonGodotObject
{
    [{|GODOT0001:BindConstant|}]
    public const int MyConstant = 42;

    [{|GODOT0001:BindEnum|}]
    public enum MyEnum { A, B, C }

    [{|GODOT0001:PropertyGroup("My Group")|}]
    [{|GODOT0001:PropertySubgroup("My Subgroup")|}]
    [{|GODOT0001:BindProperty|}]
    public int MyField;

    [{|GODOT0001:BindProperty|}]
    public int MyProperty { get; set; }

    [{|GODOT0001:BindMethod|}]
    public void MyMethod() { }

    [{|GODOT0001:Signal|}]
    public delegate void MySignalEventHandler();
}

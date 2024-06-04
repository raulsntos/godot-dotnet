using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindConstructor]
    public static MyNode MyConstructorMethod() => new();

    [Signal]
    public delegate void MySignalEventHandler();

    [BindProperty]
    public int MyProperty { get; set; }

    [BindMethod]
    public void MyMethod() { }
}

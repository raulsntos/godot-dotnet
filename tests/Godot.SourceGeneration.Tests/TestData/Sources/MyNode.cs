using Godot;

namespace NS;

[GodotClass]
[BindConstructor(typeof(MyNode), nameof(MyConstructorMethod))]
public partial class MyNode : Node
{
    public static MyNode MyConstructorMethod() => new();

    [Signal]
    public delegate void MySignalEventHandler();

    [BindProperty]
    public int MyProperty { get; set; }

    [BindMethod]
    public void MyMethod() { }
}

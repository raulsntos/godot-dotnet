using Godot;

namespace NS;

public interface IMyInterface
{
    public int MyProperty { get; set; }
    public void MyMethod();
    public int MyMethodWithReturn();
}

[GodotClass]
public partial class NodeWithExplicitInterfaceImplementations : Node, IMyInterface
{
    [BindProperty]
    int IMyInterface.MyProperty { get; set; }

    [BindMethod]
    void IMyInterface.MyMethod() { }

    [BindMethod]
    int IMyInterface.MyMethodWithReturn()
    {
        return 42;
    }
}

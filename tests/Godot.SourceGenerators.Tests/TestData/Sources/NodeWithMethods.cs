using Godot;

namespace NS;

[GodotClass]
public partial class NodeWithMethods : Node
{
    public void UnexposedMethod() { }

    [BindMethod]
    public void MyMethod() { }

    [BindMethod(Name = "my_named_method")]
    public void MyNamedMethod() { }

    [BindMethod]
    public int MyMethodWithReturn()
    {
        return 42;
    }

    [BindMethod]
    public void MyMethodWithParameters(int a, float b, string c) { }

    [BindMethod]
    public int MyMethodWithReturnAndParameters(int a, float b, string c)
    {
        return 42;
    }

    [BindMethod]
    public void MyMethodWithNamedParameters([BindProperty(Name = "my_number")] int myNumber, [BindProperty(Name = "my_string")] string myString) { }

    [BindMethod]
    public void MyMethodWithOptionalParameters(int requiredParameter, int optionalParameter = 42) { }

    [BindMethod]
    public void MyMethodWithReservedKeyword(int @int) { }

    [BindMethod]
    public static void MyStaticMethod() { }

    [BindMethod]
    public static void MyStaticMethodWithParameters(int a, float b, string c) { }

    [BindMethod]
    public static int MyStaticMethodWithReturn()
    {
        return 42;
    }

    [BindMethod]
    public static int MyStaticMethodWithReturnAndParameters(int a, float b, string c)
    {
        return 42;
    }

    [BindMethod(Virtual = true)]
    public void MyVirtualMethod() { }

    [BindMethod(Virtual = true)]
    public void MyVirtualMethodWithParameters(int a, float b, string c) { }

    [BindMethod(Virtual = true)]
    public int MyVirtualMethodWithReturn()
    {
        return 42;
    }

    [BindMethod(Virtual = true)]
    public int MyVirtualMethodWithReturnAndParameters(int a, float b, string c)
    {
        return 42;
    }
}

using Godot;

public partial class MyNode : Node
{
    [BindMethod]
    public void MyMethod(int value) { }

    public void MyMethod_NonVariantParameter(object obj) { }

    public object MyMethod_NonVariantReturn() => null;

    [BindMethod]
    public void MyMethod_WithOverloads() { }

    [BindMethod]
    public void MyMethod_WithOverloads(int value) { }

    public void MyMethod_WithOverloads(Vector3 value) { }

    public void MyMethod_Generic<T>(T parameter) { }

    public void MyMethod_RefLikeParameter(ref int x) { }

    public override void {|CS0507:_Ready|}() { }

    public class Nested
    {
        public void NestedMethod(int value) { }
    }
}

public partial class MyBaseNode : Node
{
    [BindMethod]
    public virtual void MyMethod() { }
}

public partial class MyDerivedNode : MyBaseNode
{
    [BindMethod]
    public override void MyMethod() { }
}

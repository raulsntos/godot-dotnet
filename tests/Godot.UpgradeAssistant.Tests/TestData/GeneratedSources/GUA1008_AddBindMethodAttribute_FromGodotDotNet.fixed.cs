using Godot;

public partial class MyNode : Node
{
    public void MyMethod(int value) { }

    [BindMethod]
    public void MyMethod_Bound(int value) { }

    public void MyMethod_NonVariantParameter(object obj) { }

    public object MyMethod_NonVariantReturn() => null;

    [BindMethod]
    public void MyMethod_WithOverloads() { }

    public void MyMethod_WithOverloads(int value) { }

    public void MyMethod_WithOverloads(Vector3 value) { }

    public void MyMethod_Generic<T>(T parameter) { }

    public void MyMethod_GenericMustBeVariant<[MustBeVariant] T>(T parameter) { }

    public void MyMethod_RefLikeParameter(ref int x) { }

    protected override void _Ready() { }

    public class Nested
    {
        public void NestedMethod(int value) { }
    }
}

public partial class MyBaseNode : Node
{
    public virtual void MyMethod() { }
}

public partial class MyDerivedNode : MyBaseNode
{
    public override void MyMethod() { }
}

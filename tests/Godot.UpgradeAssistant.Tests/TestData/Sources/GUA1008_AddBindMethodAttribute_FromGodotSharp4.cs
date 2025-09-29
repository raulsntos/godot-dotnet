using Godot;

public partial class MyNode : Node
{
    public void {|GUA1008:MyMethod|}(int value) { }

    public void MyMethod_NonVariantParameter(object obj) { }

    public object MyMethod_NonVariantReturn() => null;

    public void {|GUA1008:MyMethod_WithOverloads|}() { }

    public void {|GUA1008:MyMethod_WithOverloads|}(int value) { }

    public void MyMethod_WithOverloads(Vector3 value) { }

    public void MyMethod_Generic<T>(T parameter) { }

    public void MyMethod_GenericMustBeVariant<[MustBeVariant] T>(T parameter) { }

    public void MyMethod_RefLikeParameter(ref int x) { }

    public override void _Ready() { }

    public class Nested
    {
        public void NestedMethod(int value) { }
    }
}

public partial class MyBaseNode : Node
{
    public virtual void {|GUA1008:MyMethod|}() { }
}

public partial class MyDerivedNode : MyBaseNode
{
    public override void {|GUA1008:MyMethod|}() { }
}

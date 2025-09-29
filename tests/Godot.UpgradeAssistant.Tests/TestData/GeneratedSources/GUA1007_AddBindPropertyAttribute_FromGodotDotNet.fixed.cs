using Godot;

public partial class MyNode : Node
{
    public int MyField;

    public int MyProperty { get; set; }

    [BindProperty]
    public int MyField_Bound;

    [BindProperty]
    public int MyProperty_Bound { get; set; }

    public object MyField_NonVariant;

    public object MyProperty_NonVariant { get; set; }

    public const int MyField_Constant = 42;

    public static int MyField_Static;

    public static int MyProperty_Static { get; set; }

    public readonly int MyField_ReadOnly;

    public int MyProperty_ReadOnly { get; }

    public int MyProperty_WriteOnly { set { } }

    public int MyProperty_InitOnly { init { } }

    public int MyProperty_GetAndInitOnly { get; init; }

    public Node MyProperty_Node { get; set; }
}

public partial class MyResource : Resource
{
    public Node MyProperty_Node { get; set; }
}

public interface IMyInterface
{
    public int MyProperty { get; set; }
}

public partial class MyNode2 : Node, IMyInterface
{
    int IMyInterface.MyProperty { get; set; }
}

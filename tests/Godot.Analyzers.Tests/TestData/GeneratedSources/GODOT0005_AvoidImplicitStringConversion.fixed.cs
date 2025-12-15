using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    private static readonly StringName DuplicateValue = new StringName("DuplicateValue");

    private static readonly StringName AssignedValue = new StringName("AssignedValue");

    private static readonly StringName ReturnValue = new StringName("ReturnValue");

    private static readonly NodePath ParameterPath = new NodePath("Parameter/Path");

    private static readonly StringName ParameterName = new StringName("ParameterName");

    private static readonly NodePath AnotherPath = new NodePath("Another/Path");

    private static readonly NodePath MyNodePath = new NodePath("My/Node/Path");

    private static readonly StringName AnotherName = new StringName("AnotherName");

    private static readonly StringName MyName = new StringName("MyName");

    private static readonly StringName ExistingCachedField = new StringName("ExistingCachedValue");

    public void TestImplicitStringNameConversion()
    {
        StringName name1 = MyName;
        StringName name2 = AnotherName;

        StringName name3 = (StringName)"ExplicitName";

        StringName name4 = new StringName("ConstructorName");
    }

    public void TestImplicitNodePathConversion()
    {
        NodePath path1 = MyNodePath;
        NodePath path2 = AnotherPath;

        NodePath path3 = (NodePath)"Explicit/Path";

        NodePath path4 = new NodePath("Constructor/Path");
    }

    public void TestMethodParameters()
    {
        TakesStringName(ParameterName);
        TakesNodePath(ParameterPath);
    }

    public void TakesStringName(StringName name) { }
    public void TakesNodePath(NodePath path) { }

    public StringName TestReturnValue()
    {
        return ReturnValue;
    }

    public void TestAssignment()
    {
        StringName name = new StringName("Initial");

        name = AssignedValue;
    }

    public void TestExistingCachedValue()
    {
        StringName name1 = ExistingCachedField;
        StringName name2 = ExistingCachedField;
    }

    public void TestDuplicateValues()
    {
        StringName name1 = DuplicateValue;
        StringName name2 = DuplicateValue;
    }
}

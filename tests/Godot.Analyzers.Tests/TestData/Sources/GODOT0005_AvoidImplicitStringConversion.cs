using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    private static readonly StringName ExistingCachedField = new StringName("ExistingCachedValue");

    public void TestImplicitStringNameConversion()
    {
        StringName name1 = {|GODOT0005:"MyName"|};
        StringName name2 = {|GODOT0005:"AnotherName"|};

        StringName name3 = (StringName)"ExplicitName";

        StringName name4 = new StringName("ConstructorName");
    }

    public void TestImplicitNodePathConversion()
    {
        NodePath path1 = {|GODOT0005:"My/Node/Path"|};
        NodePath path2 = {|GODOT0005:"Another/Path"|};

        NodePath path3 = (NodePath)"Explicit/Path";

        NodePath path4 = new NodePath("Constructor/Path");
    }

    public void TestMethodParameters()
    {
        TakesStringName({|GODOT0005:"ParameterName"|});
        TakesNodePath({|GODOT0005:"Parameter/Path"|});
    }

    public void TakesStringName(StringName name) { }
    public void TakesNodePath(NodePath path) { }

    public StringName TestReturnValue()
    {
        return {|GODOT0005:"ReturnValue"|};
    }

    public void TestAssignment()
    {
        StringName name = new StringName("Initial");

        name = {|GODOT0005:"AssignedValue"|};
    }

    public void TestExistingCachedValue()
    {
        StringName name1 = {|GODOT0005:"ExistingCachedValue"|};
        StringName name2 = {|GODOT0005:"ExistingCachedValue"|};
    }

    public void TestDuplicateValues()
    {
        StringName name1 = {|GODOT0005:"DuplicateValue"|};
        StringName name2 = {|GODOT0005:"DuplicateValue"|};
    }
}

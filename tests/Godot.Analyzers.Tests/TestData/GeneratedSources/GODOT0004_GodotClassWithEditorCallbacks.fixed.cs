using System.Collections.Generic;
using Godot;
using Godot.Bridge;

namespace NS;

[GodotClass(Tool = true)]
public partial class MyNode : Node
{
    private void SomeUserDefinedMethod() { }

    protected override void _EnterTree() { }

    protected override void _ExitTree() { }

    protected override bool _Set(StringName property, Variant value)
    {
        return false;
    }

    protected override bool _Get(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    protected override void _GetPropertyList(IList<PropertyInfo> properties) { }

    protected override bool _PropertyCanRevert(StringName property)
    {
        return false;
    }

    protected override bool _PropertyGetRevert(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    protected override void _ValidateProperty(PropertyInfo property) { }
}

[GodotClass(Tool = true)]
public partial class MyNodeAlreadyTool : Node
{
    protected override void _EnterTree() { }

    protected override void _ExitTree() { }

    protected override bool _Set(StringName property, Variant value)
    {
        return false;
    }

    protected override bool _Get(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    protected override void _GetPropertyList(IList<PropertyInfo> properties) { }

    protected override bool _PropertyCanRevert(StringName property)
    {
        return false;
    }

    protected override bool _PropertyGetRevert(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    protected override void _ValidateProperty(PropertyInfo property) { }
}

[GodotClass(Tool = true)]
public partial class MyNodeExplicitlyNotTool : Node
{
    protected override void _EnterTree() { }

    protected override void _ExitTree() { }

    protected override bool _Set(StringName property, Variant value)
    {
        return false;
    }

    protected override bool _Get(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    protected override void _GetPropertyList(IList<PropertyInfo> properties) { }

    protected override bool _PropertyCanRevert(StringName property)
    {
        return false;
    }

    protected override bool _PropertyGetRevert(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    protected override void _ValidateProperty(PropertyInfo property) { }
}

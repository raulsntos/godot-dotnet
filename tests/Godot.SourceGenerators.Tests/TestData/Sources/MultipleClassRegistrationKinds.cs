using Godot;
using Godot.Bridge;

namespace NS;

public partial class MyUnregisteredNode : Node { }

[GodotClass]
public partial class MyNode : Node
{
    public static void BindMembers(ClassRegistrationContext context) { }
}

[GodotClass(Tool = true)]
public partial class MyToolNode : Node
{
    public static void BindMembers(ClassRegistrationContext context) { }
}

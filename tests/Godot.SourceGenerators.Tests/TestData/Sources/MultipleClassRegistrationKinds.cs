using Godot;
using Godot.Bridge;

namespace NS;

public partial class MyUnregisteredNode : Node { }

[GodotClass]
public partial class MyNode : Node
{
    public static void BindMethods(ClassRegistrationContext context) { }
}

[GodotClass(Tool = true)]
public partial class MyToolNode : Node
{
    public static void BindMethods(ClassRegistrationContext context) { }
}

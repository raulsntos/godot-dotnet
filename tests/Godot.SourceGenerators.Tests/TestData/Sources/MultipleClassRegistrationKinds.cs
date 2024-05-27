using Godot;
using Godot.Bridge;

namespace NS;

public partial class MyUnregisteredNode : Node { }

[GodotClass]
public partial class MyNode : Node
{
    public static void BindMethods(ClassDBRegistrationContext context) { }
}

[GodotClass(Tool = true)]
public partial class MyToolNode : Node
{
    public static void BindMethods(ClassDBRegistrationContext context) { }
}

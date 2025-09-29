using Godot;

[GodotClass]
public partial class MyNode : Node { }

[GodotClass(Tool = true)]
public partial class MyToolNode : Node { }

public class SomeClass : object { }

[GodotClass]
public partial class OuterClass : Node
{
    public partial class NestedClass : Node { }
}

[GodotClass]
public partial class MyGlobalClass : Node { }

[GodotClass(Tool = true)]
public partial class MyToolGlobalClass1 : Node { }

[GodotClass(Tool = true)]
public partial class MyToolGlobalClass2 : Node { }

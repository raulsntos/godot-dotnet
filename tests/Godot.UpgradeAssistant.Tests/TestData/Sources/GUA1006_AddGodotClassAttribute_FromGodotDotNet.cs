using Godot;

public partial class MyNode : Node { }

[GodotClass(Tool = true)]
public partial class MyToolNode : Node { }

public class SomeClass : object { }

public partial class OuterClass : Node
{
    public partial class NestedClass : Node { }
}

[GodotClass]
public partial class MyGodotClass : Node { }

public partial class GenericClass<T> : Node { }

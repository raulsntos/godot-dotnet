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

public partial class GenericClass<T> : Node { }

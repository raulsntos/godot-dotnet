using Godot;

public partial class {|GUA1006:MyNode|} : Node { }

[Tool]
public partial class {|GUA1006:MyToolNode|} : Node { }

public class SomeClass : object { }

public partial class {|GUA1006:OuterClass|} : Node
{
    public partial class NestedClass : Node { }
}

public partial class GenericClass<T> : Node { }

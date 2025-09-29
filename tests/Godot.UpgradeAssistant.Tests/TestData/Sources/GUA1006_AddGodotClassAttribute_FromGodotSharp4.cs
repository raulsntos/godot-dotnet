using Godot;

public partial class {|GUA1006:MyNode|} : Node { }

[Tool]
public partial class {|GUA1006:MyToolNode|} : Node { }

public class SomeClass : object { }

public partial class {|GUA1006:OuterClass|} : Node
{
    public partial class NestedClass : Node { }
}

[GlobalClass]
public partial class {|GUA1006:MyGlobalClass|} : Node { }

[GlobalClass, Tool]
public partial class {|GUA1006:MyToolGlobalClass1|} : Node { }

[Tool]
[GlobalClass]
public partial class {|GUA1006:MyToolGlobalClass2|} : Node { }

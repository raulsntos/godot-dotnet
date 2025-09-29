using Godot;

public class {|GUA1001:MyNode|} : Node
{
}

public partial class AlreadyPartial : Node
{
}

public class NotGodot : object
{
}

public partial class Outer : Node
{
    public class {|GUA1001:Nested|} : Node { }
}

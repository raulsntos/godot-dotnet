using Godot;

public partial class MyNode : Node
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
    public partial class Nested : Node { }
}

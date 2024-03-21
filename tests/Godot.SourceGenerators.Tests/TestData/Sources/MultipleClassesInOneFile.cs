using Godot;

namespace NamespaceA
{
    [GodotClass]
    public partial class ClassOne : Node { }
}

namespace NamespaceB
{
    [GodotClass]
    public partial class ClassTwo : Node { }
}

namespace NamespaceC
{
    public partial class UnrelatedClass : Node { }
}

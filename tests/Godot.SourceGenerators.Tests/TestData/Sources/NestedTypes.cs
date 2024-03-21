using Godot;

namespace NS;

[GodotClass]
public partial class Node1 : Node
{
    [BindProperty]
    public int Node1Property { get; set; }

    [GodotClass]
    public partial class Node2 : Node
    {
        [BindProperty]
        public int Node2Property { get; set; }

        [GodotClass]
        public partial class Node3 : Node
        {
            [BindProperty]
            public int Node3Property { get; set; }
        }
    }
}

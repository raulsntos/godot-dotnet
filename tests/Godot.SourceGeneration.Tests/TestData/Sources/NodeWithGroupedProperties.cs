using Godot;

namespace NS;

[GodotClass]
public partial class NodeWithGroupedProperties : Node
{
    [BindProperty]
    public string UngroupedProperty { get; set; }

    [PropertyGroup("Text", "Text")]

    [BindProperty]
    public int TextSize { get; set; }

    [PropertySubgroup("Text Outline", "TextOutline")]

    [BindProperty]
    public float TextOutlineWidth { get; set; }

    [BindProperty]
    public float TextOutlineColor { get; set; }

    [PropertyGroup("Physics")]

    [BindProperty]
    public float Speed { get; set; }

    [BindProperty]
    public float Gravity { get; set; }

    [PropertySubgroup("Collisions")]

    [BindProperty]
    public bool CanCollideWithEnemies { get; set; }

    [BindProperty]
    public bool CanCollideWithWalls { get; set; }
}

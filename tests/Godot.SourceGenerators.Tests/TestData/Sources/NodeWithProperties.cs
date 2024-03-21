using Godot;

namespace NS;

[GodotClass]
public partial class NodeWithProperties : Node
{
    public int UnexposedProperty { get; set; }

    [BindProperty]
    public int MyProperty { get; set; }

    [BindProperty(Name = "my_named_property")]
    public int MyNamedProperty { get; set; }

    [BindProperty]
    public int MyPropertyWithDefaultValue { get; set; } = 42;
}

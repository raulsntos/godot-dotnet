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

    public int unexposedField;

    [BindProperty]
    public int myField;

    [BindProperty(Name = "my_named_field")]
    public int myNamedField;

    [BindProperty]
    public int myFieldWithDefaultValue = 42;

    [BindProperty(Hint = PropertyHint.Layers3DPhysics)]
    public int myFieldWithHintOverride;

    [BindProperty(HintString = "my_hint_string")]
    public int myFieldWithHintStringOverride;
}

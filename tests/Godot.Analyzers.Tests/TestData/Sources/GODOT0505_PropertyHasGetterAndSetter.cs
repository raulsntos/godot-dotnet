using Godot;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindProperty]
    public int MyProperty { get; set; }

    [BindProperty]
    public int {|#0:MyGetterOnlyProperty|} { get; }

    [BindProperty]
    public int {|#1:MySetterOnlyProperty|} { set { } }

    [BindProperty]
    public int {|#2:MyInitOnlyProperty|} { init { } }

    [BindProperty]
    public int {|#3:MyGetterInitOnlyProperty|} { get; init; }
}

using Godot;

namespace NS;

[GodotClass]
public partial class BaseNode : Node
{
    [BindProperty]
    public int MyValue { get; set; }

    [BindProperty(Name = "MyAlias")]
    public int MyOtherValue { get; set; }

    [BindMethod]
    public void MyMethod() { }

    [BindMethod(Name = "MyAliasedMethod")]
    public void MyOtherMethod() { }

    [BindMethod]
    public void ShadowedMethod() { }

    [Signal]
    public delegate void MySignalEventHandler();
}

[GodotClass]
public partial class DerivedNode : BaseNode
{
    [BindProperty]
    public int UniqueProperty { get; set; }

    [BindProperty(Name = "MyValue")]
    public int {|GODOT0002:ConflictingProperty|} { get; set; }

    [BindProperty(Name = "MyAlias")]
    public int {|GODOT0002:AnotherConflictingProperty|} { get; set; }

    [BindProperty(Name = "UniqueProperty")]
    public int {|GODOT0002:DuplicateUniqueProperty|} { get; set; }

    [BindMethod]
    public new void {|GODOT0002:ShadowedMethod|}() { }

    [BindMethod(Name = "MyMethod")]
    public void {|GODOT0002:ConflictingMethod|}() { }

    [BindMethod]
    public void UniqueMethod() { }

    [BindMethod]
    public void {|GODOT0002:UniqueMethod|}(int x) { }

    [BindMethod]
    public void {|GODOT0002:UniqueMethod|}(int x, int y) { }

    [BindMethod(Name = "MyAliasedMethod")]
    public void {|GODOT0002:AnotherConflictingMethod|}() { }

    [BindMethod]
    public new void MyOtherValue() { }

    [Signal]
    public delegate void UniqueSignalEventHandler();

    [Signal(Name = "MySignal")]
    public delegate void {|GODOT0002:ConflictingSignalEventHandler|}();

    [Signal(Name = "UniqueSignal")]
    public delegate void {|GODOT0002:DuplicateUniqueSignalEventHandler|}();
}

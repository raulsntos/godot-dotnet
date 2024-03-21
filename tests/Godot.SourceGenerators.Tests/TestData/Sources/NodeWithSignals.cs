using Godot;

namespace NS;

[GodotClass]
public partial class NodeWithSignals : Node
{
    public delegate void UnexposedDelegate();

    [Signal]
    public delegate void MySignal();

    [Signal(Name = "my_named_signal")]
    public delegate void MyNamedSignal();

    [Signal]
    public delegate void MySignalWithParameters(int a, float b, string c);

    [Signal]
    public delegate void MySignalWithNamedParameters([BindProperty(Name = "my_number")] int myNumber, [BindProperty(Name = "my_string")] string myString);

    [Signal]
    public delegate void MySignalWithOptionalParameters(int requiredParameter, int optionalParameter = 42);
}

using Godot;

namespace NS;

[GodotClass]
public partial class NodeWithSignals : Node
{
    public delegate void UnexposedDelegate();

    [Signal]
    public delegate void MySignalEventHandler();

    [Signal(Name = "my_named_signal")]
    public delegate void MyNamedSignalEventHandler();

    [Signal]
    public delegate void MySignalWithParametersEventHandler(int a, float b, string c);

    [Signal]
    public delegate void MySignalWithNamedParametersEventHandler([BindProperty(Name = "my_number")] int myNumber, [BindProperty(Name = "my_string")] string myString);

    [Signal]
    public delegate void MySignalWithOptionalParametersEventHandler(int requiredParameter, int optionalParameter = 42);
}

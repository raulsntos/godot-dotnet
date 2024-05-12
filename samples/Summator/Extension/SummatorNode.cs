using Godot;

namespace GDExtensionSummator;
[GodotClass]
public partial class SummatorNode: Node
{

    private int _count;

    [BindMethod]
    public void Add(int value = 1)
    {
        _count += value;
    }
    [BindMethod]
    public void Reset()
    {
        _count = 0;
    }
    [BindMethod]
    public int GetTotal()
    {
        return _count;
    }
}

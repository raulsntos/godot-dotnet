using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    public List<int> MyUnboundField;

    [BindProperty]
    public PackedInt32Array MyField1;

    [BindProperty]
    public PackedInt32Array MyField2;

    [BindProperty]
    public PackedInt32Array MyField3;

    [BindProperty]
    public GodotArray<int> MyField4;

    public List<int> MyUnboundProperty { get; set; }

    [BindProperty]
    public PackedInt32Array MyProperty { get; set; }

    public void MyUnboundMethod(List<int> parameter) { }

    [BindMethod]
    public void MyMethod(PackedInt32Array parameter) { }

    [BindMethod]
    public PackedInt32Array MyMethodWithReturn() => [];

    [Signal]
    public delegate void MySignalEventHandler(PackedInt32Array parameter);

    [BindMethod]
    public void MethodWithManyParameters(int a, string b, PackedInt32Array c, float d) { }

    [BindMethod]
    public int MethodWithManyParametersAndReturn1(int a, string b, PackedStringArray c, float d) => 0;

    [BindMethod]
    public PackedVector2Array MethodWithManyParametersAndReturn2(int a, string b, GodotArray<Node> c, float d) => [];
}

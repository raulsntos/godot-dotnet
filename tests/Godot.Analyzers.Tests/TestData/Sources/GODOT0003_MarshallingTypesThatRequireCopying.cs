using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    public List<int> MyUnboundField;

    [BindProperty]
    public {|GODOT0003:List<int>|} MyField1;

    [BindProperty]
    public {|GODOT0003:int[]|} MyField2;

    [BindProperty]
    public PackedInt32Array MyField3;

    [BindProperty]
    public GodotArray<int> MyField4;

    public List<int> MyUnboundProperty { get; set; }

    [BindProperty]
    public {|GODOT0003:List<int>|} MyProperty { get; set; }

    public void MyUnboundMethod(List<int> parameter) { }

    [BindMethod]
    public void MyMethod({|GODOT0003:List<int>|} parameter) { }

    [BindMethod]
    public {|GODOT0003:List<int>|} MyMethodWithReturn() => [];

    [Signal]
    public delegate void MySignalEventHandler({|GODOT0003:List<int>|} parameter);

    [BindMethod]
    public void MethodWithManyParameters(int a, string b, {|GODOT0003:int[]|} c, float d) { }

    [BindMethod]
    public int MethodWithManyParametersAndReturn1(int a, string b, {|GODOT0003:string[]|} c, float d) => 0;

    [BindMethod]
    public {|GODOT0003:Vector2[]|} MethodWithManyParametersAndReturn2(int a, string b, {|GODOT0003:Node[]|} c, float d) => [];
}

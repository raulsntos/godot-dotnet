using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class MyNode : Node
{
    public void CallWithPackedArray()
    {
        var navPolygon = new NavigationPolygon();

        List<int> list = new List<int>() { 1, 2, 3 };
        int[] arr = new int[] { 1, 2, 3 };
        navPolygon.AddPolygon(new Godot.Collections.PackedInt32Array(arr));
        navPolygon.AddPolygon(new Godot.Collections.PackedInt32Array(list));
        navPolygon.AddPolygon([1, 2, 3]);
        navPolygon.AddPolygon(new Godot.Collections.PackedInt32Array { 1, 2, 3 });
        navPolygon.AddPolygon(new Godot.Collections.PackedInt32Array { 1, 2, 3 });
        navPolygon.AddPolygon(new Godot.Collections.PackedInt32Array(ApiThatReturnsArray()));
        navPolygon.AddPolygon(navPolygon.GetPolygon(0));

        arr = navPolygon.GetPolygon(0).ToArray();
        var arr1 = navPolygon.GetPolygon(0).ToArray();
        int[] arr2 = navPolygon.GetPolygon(0).ToArray();
        Span<int> arr3 = navPolygon.GetPolygon(0).ToArray();
        ReadOnlySpan<int> arr4 = navPolygon.GetPolygon(0).ToArray();

        arr = [.. navPolygon.GetPolygon(0)];
    }

    private int[] ApiThatReturnsArray() => [];

    public override Godot.Collections.PackedStringArray {|CS0507:_GetConfigurationWarnings|}()
    {
        return [];
    }
}

public partial class MyBaseNode : MyNode
{
    public virtual string[] _GetConfigurationWarnings() => [];
}

public partial class MyDerivedNode : MyBaseNode
{
    public override string[] _GetConfigurationWarnings() => ["warning"];
}

public partial class MyExportPlugin : EditorExportPlugin
{
    public override void {|CS0507:_ExportFile|}(string path, string type, Godot.Collections.PackedStringArray features) { }
}

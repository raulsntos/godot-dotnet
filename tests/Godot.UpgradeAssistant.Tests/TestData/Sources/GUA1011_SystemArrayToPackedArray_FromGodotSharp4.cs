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
        navPolygon.AddPolygon({|GUA1011:arr|});
        navPolygon.AddPolygon({|GUA1011:list.ToArray()|});
        navPolygon.AddPolygon([1, 2, 3]);
        navPolygon.AddPolygon({|GUA1011:new int[] { 1, 2, 3 }|});
        navPolygon.AddPolygon({|GUA1011:stackalloc int[] { 1, 2, 3 }|});
        navPolygon.AddPolygon({|GUA1011:ApiThatReturnsArray()|});
        navPolygon.AddPolygon(navPolygon.GetPolygon(0));

        arr = {|GUA1011:navPolygon.GetPolygon(0)|};
        var arr1 = {|GUA1011:navPolygon.GetPolygon(0)|};
        int[] arr2 = {|GUA1011:navPolygon.GetPolygon(0)|};
        Span<int> arr3 = {|GUA1011:navPolygon.GetPolygon(0)|};
        ReadOnlySpan<int> arr4 = {|GUA1011:navPolygon.GetPolygon(0)|};

        arr = [.. navPolygon.GetPolygon(0)];
    }

    private int[] ApiThatReturnsArray() => [];

    {|GUA1011:public override string[] _GetConfigurationWarnings()
    {
        return [];
    }|}
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
    public override void _ExportFile(string path, string type, {|GUA1011:string[] features|}) { }
}

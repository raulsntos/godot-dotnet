using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class MyNode : Node
{
    public void CallWithPackedArray()
    {
        var navPolygon = new NavigationPolygon();

        navPolygon.AddPolygon([1, 2, 3]);
        navPolygon.AddPolygon(new PackedInt32Array { 1, 2, 3 });
        navPolygon.AddPolygon(ApiThatReturnsPackedInt32Array());
        navPolygon.AddPolygon(navPolygon.GetPolygon(0));

        int[] arr = [.. navPolygon.GetPolygon(0)];
    }

    private PackedInt32Array ApiThatReturnsPackedInt32Array() => [];

    protected override PackedStringArray _GetConfigurationWarnings()
    {
        return [];
    }
}

public partial class MyBaseNode : MyNode
{
    protected virtual string[] _GetConfigurationWarnings() => [];
}

public partial class MyDerivedNode : MyBaseNode
{
    protected override string[] _GetConfigurationWarnings() => ["warning"];
}

public partial class MyExportPlugin : EditorExportPlugin
{
    protected override void _ExportFile(string path, string type, PackedStringArray features) { }
}

using System;
using Godot;
using Godot.Collections;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindProperty]
    public bool PropertyBoolean { get; set; }

    [BindProperty]
    public char PropertyChar { get; set; }

    [BindProperty]
    public sbyte PropertySByte { get; set; }

    [BindProperty]
    public short PropertyInt16 { get; set; }

    [BindProperty]
    public int PropertyInt32 { get; set; }

    [BindProperty]
    public long PropertyInt64 { get; set; }

    [BindProperty]
    public byte PropertyByte { get; set; }

    [BindProperty]
    public ushort PropertyUInt16 { get; set; }

    [BindProperty]
    public uint PropertyUInt32 { get; set; }

    [BindProperty]
    public ulong PropertyUInt64 { get; set; }

    [BindProperty]
    public float PropertySingle { get; set; }

    [BindProperty]
    public double PropertyDouble { get; set; }

    [BindProperty]
    public string PropertyString { get; set; }

    [BindProperty]
    public Aabb PropertyAabb { get; set; }

    [BindProperty]
    public Basis PropertyBasis { get; set; }

    [BindProperty]
    public Callable PropertyCallable { get; set; }

    [BindProperty]
    public Color PropertyColor { get; set; }

    [BindProperty]
    public NodePath PropertyNodePath { get; set; }

    [BindProperty]
    public Plane PropertyPlane { get; set; }

    [BindProperty]
    public Projection PropertyProjection { get; set; }

    [BindProperty]
    public Quaternion PropertyQuaternion { get; set; }

    [BindProperty]
    public Rect2 PropertyRect2 { get; set; }

    [BindProperty]
    public Rect2I PropertyRect2I { get; set; }

    [BindProperty]
    public Rid PropertyRid { get; set; }

    [BindProperty]
    public Signal PropertySignal { get; set; }

    [BindProperty]
    public StringName PropertyStringName { get; set; }

    [BindProperty]
    public Transform2D PropertyTransform2D { get; set; }

    [BindProperty]
    public Transform3D PropertyTransform3D { get; set; }

    [BindProperty]
    public Vector2 PropertyVector2 { get; set; }

    [BindProperty]
    public Vector2I PropertyVector2I { get; set; }

    [BindProperty]
    public Vector3 PropertyVector3 { get; set; }

    [BindProperty]
    public Vector3I PropertyVector3I { get; set; }

    [BindProperty]
    public Vector4 PropertyVector4 { get; set; }

    [BindProperty]
    public Vector4I PropertyVector4I { get; set; }

    [BindProperty]
    public Variant PropertyVariant { get; set; }

    [BindProperty]
    public GodotObject PropertyGodotObject { get; set; }

    [BindProperty]
    public Node PropertyNode { get; set; }

    [BindProperty]
    public Resource PropertyResource { get; set; }

    [BindProperty]
    public PackedByteArray PropertyPackedByteArray { get; set; }

    [BindProperty]
    public PackedInt32Array PropertyPackedInt32Array { get; set; }

    [BindProperty]
    public PackedInt64Array PropertyPackedInt64Array { get; set; }

    [BindProperty]
    public PackedFloat32Array PropertyPackedFloat32Array { get; set; }

    [BindProperty]
    public PackedFloat64Array PropertyPackedFloat64Array { get; set; }

    [BindProperty]
    public PackedStringArray PropertyPackedStringArray { get; set; }

    [BindProperty]
    public PackedVector2Array PropertyPackedVector2Array { get; set; }

    [BindProperty]
    public PackedVector3Array PropertyPackedVector3Array { get; set; }

    [BindProperty]
    public PackedColorArray PropertyPackedColorArray { get; set; }

    [BindProperty]
    public PackedVector4Array PropertyPackedVector4Array { get; set; }

    [BindProperty]
    public GodotArray PropertyGodotArray { get; set; }

    [BindProperty]
    public GodotDictionary PropertyGodotDictionary { get; set; }

    [BindProperty]
    public GodotArray<int> PropertyGodotGenericArray { get; set; }

    [BindProperty]
    public GodotDictionary<string, bool> PropertyGodotGenericDictionary { get; set; }

    public enum MyEnum { A, B, C }

    [BindProperty]
    public MyEnum PropertyEnum { get; set; }

    [Flags]
    public enum MyFlagsEnum { A, B, C }

    [BindProperty]
    public MyFlagsEnum PropertyFlagsEnum { get; set; }

    [BindProperty]
    public {|GODOT0501:object|} PropertySystemObject { get; set; }
}

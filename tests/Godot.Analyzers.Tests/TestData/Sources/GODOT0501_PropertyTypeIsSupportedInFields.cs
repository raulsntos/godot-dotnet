using System;
using Godot;
using Godot.Collections;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindProperty]
    private bool _fieldBoolean;

    [BindProperty]
    private char _fieldChar;

    [BindProperty]
    private sbyte _fieldSByte;

    [BindProperty]
    private short _fieldInt16;

    [BindProperty]
    private int _fieldInt32;

    [BindProperty]
    private long _fieldInt64;

    [BindProperty]
    private byte _fieldByte;

    [BindProperty]
    private ushort _fieldUInt16;

    [BindProperty]
    private uint _fieldUInt32;

    [BindProperty]
    private ulong _fieldUInt64;

    [BindProperty]
    private float _fieldSingle;

    [BindProperty]
    private double _fieldDouble;

    [BindProperty]
    private string _fieldString;

    [BindProperty]
    private Aabb _fieldAabb;

    [BindProperty]
    private Basis _fieldBasis;

    [BindProperty]
    private Callable _fieldCallable;

    [BindProperty]
    private Color _fieldColor;

    [BindProperty]
    private NodePath _fieldNodePath;

    [BindProperty]
    private Plane _fieldPlane;

    [BindProperty]
    private Projection _fieldProjection;

    [BindProperty]
    private Quaternion _fieldQuaternion;

    [BindProperty]
    private Rect2 _fieldRect2;

    [BindProperty]
    private Rect2I _fieldRect2I;

    [BindProperty]
    private Rid _fieldRid;

    [BindProperty]
    private Signal _fieldSignal;

    [BindProperty]
    private StringName _fieldStringName;

    [BindProperty]
    private Transform2D _fieldTransform2D;

    [BindProperty]
    private Transform3D _fieldTransform3D;

    [BindProperty]
    private Vector2 _fieldVector2;

    [BindProperty]
    private Vector2I _fieldVector2I;

    [BindProperty]
    private Vector3 _fieldVector3;

    [BindProperty]
    private Vector3I _fieldVector3I;

    [BindProperty]
    private Vector4 _fieldVector4;

    [BindProperty]
    private Vector4I _fieldVector4I;

    [BindProperty]
    private Variant _fieldVariant;

    [BindProperty]
    private GodotObject _fieldGodotObject;

    [BindProperty]
    private Node _fieldNode;

    [BindProperty]
    private Resource _fieldResource;

    [BindProperty]
    private PackedByteArray _fieldPackedByteArray;

    [BindProperty]
    private PackedInt32Array _fieldPackedInt32Array;

    [BindProperty]
    private PackedInt64Array _fieldPackedInt64Array;

    [BindProperty]
    private PackedFloat32Array _fieldPackedFloat32Array;

    [BindProperty]
    private PackedFloat64Array _fieldPackedFloat64Array;

    [BindProperty]
    private PackedStringArray _fieldPackedStringArray;

    [BindProperty]
    private PackedVector2Array _fieldPackedVector2Array;

    [BindProperty]
    private PackedVector3Array _fieldPackedVector3Array;

    [BindProperty]
    private PackedColorArray _fieldPackedColorArray;

    [BindProperty]
    private PackedVector4Array _fieldPackedVector4Array;

    [BindProperty]
    private GodotArray _fieldGodotArray;

    [BindProperty]
    private GodotDictionary _fieldGodotDictionary;

    [BindProperty]
    private GodotArray<int> _fieldGodotGenericArray;

    [BindProperty]
    private GodotDictionary<string, bool> _fieldGodotGenericDictionary;

    public enum MyEnum { A, B, C }

    [BindProperty]
    private MyEnum _fieldEnum;

    [Flags]
    public enum MyFlagsEnum { A, B, C }

    [BindProperty]
    private MyFlagsEnum _fieldFlagsEnum;

    [BindProperty]
    private {|GODOT0501:object|} _fieldSystemObject;
}

using Godot;
using Godot.Collections;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [Signal]
    public delegate void SignalBoolEventHandler(bool parameter);

    [Signal]
    public delegate void SignalCharEventHandler(char parameter);

    [Signal]
    public delegate void SignalSByteEventHandler(sbyte parameter);

    [Signal]
    public delegate void SignalByteEventHandler(byte parameter);

    [Signal]
    public delegate void SignalInt16EventHandler(short parameter);

    [Signal]
    public delegate void SignalUInt16EventHandler(ushort parameter);

    [Signal]
    public delegate void SignalInt32EventHandler(int parameter);

    [Signal]
    public delegate void SignalUInt32EventHandler(uint parameter);

    [Signal]
    public delegate void SignalInt64EventHandler(long parameter);

    [Signal]
    public delegate void SignalUInt64EventHandler(ulong parameter);

    [Signal]
    public delegate void SignalSingleEventHandler(float parameter);

    [Signal]
    public delegate void SignalDoubleEventHandler(double parameter);

    [Signal]
    public delegate void SignalStringEventHandler(string parameter);

    [Signal]
    public delegate void SignalVector2EventHandler(Vector2 parameter);

    [Signal]
    public delegate void SignalVector2IEventHandler(Vector2I parameter);

    [Signal]
    public delegate void SignalRect2EventHandler(Rect2 parameter);

    [Signal]
    public delegate void SignalRect2IEventHandler(Rect2I parameter);

    [Signal]
    public delegate void SignalTransform2DEventHandler(Transform2D parameter);

    [Signal]
    public delegate void SignalVector3EventHandler(Vector3 parameter);

    [Signal]
    public delegate void SignalVector3IEventHandler(Vector3I parameter);

    [Signal]
    public delegate void SignalVector4EventHandler(Vector4 parameter);

    [Signal]
    public delegate void SignalVector4IEventHandler(Vector4I parameter);

    [Signal]
    public delegate void SignalBasisEventHandler(Basis parameter);

    [Signal]
    public delegate void SignalQuaternionEventHandler(Quaternion parameter);

    [Signal]
    public delegate void SignalTransform3DEventHandler(Transform3D parameter);

    [Signal]
    public delegate void SignalProjectionEventHandler(Projection parameter);

    [Signal]
    public delegate void SignalAabbEventHandler(Aabb parameter);

    [Signal]
    public delegate void SignalColorEventHandler(Color parameter);

    [Signal]
    public delegate void SignalPlaneEventHandler(Plane parameter);

    [Signal]
    public delegate void SignalCallableEventHandler(Callable parameter);

    [Signal]
    public delegate void SignalSignalEventHandler(Signal parameter);

    [Signal]
    public delegate void SignalGodotObjectEventHandler(GodotObject parameter);

    [Signal]
    public delegate void SignalStringNameEventHandler(StringName parameter);

    [Signal]
    public delegate void SignalNodePathEventHandler(NodePath parameter);

    [Signal]
    public delegate void SignalRidEventHandler(Rid parameter);

    [Signal]
    public delegate void SignalGodotDictionaryEventHandler(GodotDictionary parameter);

    [Signal]
    public delegate void SignalGodotArrayEventHandler(GodotArray parameter);

    [Signal]
    public delegate void SignalPackedByteArrayEventHandler(PackedByteArray parameter);

    [Signal]
    public delegate void SignalPackedInt32ArrayEventHandler(PackedInt32Array parameter);

    [Signal]
    public delegate void SignalPackedInt64ArrayEventHandler(PackedInt64Array parameter);

    [Signal]
    public delegate void SignalPackedFloat32ArrayEventHandler(PackedFloat32Array parameter);

    [Signal]
    public delegate void SignalPackedFloat64ArrayEventHandler(PackedFloat64Array parameter);

    [Signal]
    public delegate void SignalPackedStringArrayEventHandler(PackedStringArray parameter);

    [Signal]
    public delegate void SignalPackedVector2ArrayEventHandler(PackedVector2Array parameter);

    [Signal]
    public delegate void SignalPackedVector3ArrayEventHandler(PackedVector3Array parameter);

    [Signal]
    public delegate void SignalPackedColorArrayEventHandler(PackedColorArray parameter);

    [Signal]
    public delegate void SignalPackedVector4ArrayEventHandler(PackedVector4Array parameter);

    [Signal]
    public delegate void SignalObjectEventHandler({|GODOT0701:object|} parameter);
}

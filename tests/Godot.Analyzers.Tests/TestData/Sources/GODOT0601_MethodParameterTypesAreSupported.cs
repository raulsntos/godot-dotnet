using Godot;
using Godot.Collections;

namespace NS;

[GodotClass]
public partial class MyNode : Node
{
    [BindMethod]
    public void MethodBool(bool parameter) { }

    [BindMethod]
    public void MethodChar(char parameter) { }

    [BindMethod]
    public void MethodSByte(sbyte parameter) { }

    [BindMethod]
    public void MethodByte(byte parameter) { }

    [BindMethod]
    public void MethodInt16(short parameter) { }

    [BindMethod]
    public void MethodUInt16(ushort parameter) { }

    [BindMethod]
    public void MethodInt32(int parameter) { }

    [BindMethod]
    public void MethodUInt32(uint parameter) { }

    [BindMethod]
    public void MethodInt64(long parameter) { }

    [BindMethod]
    public void MethodUInt64(ulong parameter) { }

    [BindMethod]
    public void MethodSingle(float parameter) { }

    [BindMethod]
    public void MethodDouble(double parameter) { }

    [BindMethod]
    public void MethodString(string parameter) { }

    [BindMethod]
    public void MethodVector2(Vector2 parameter) { }

    [BindMethod]
    public void MethodVector2I(Vector2I parameter) { }

    [BindMethod]
    public void MethodRect2(Rect2 parameter) { }

    [BindMethod]
    public void MethodRect2I(Rect2I parameter) { }

    [BindMethod]
    public void MethodTransform2D(Transform2D parameter) { }

    [BindMethod]
    public void MethodVector3(Vector3 parameter) { }

    [BindMethod]
    public void MethodVector3I(Vector3I parameter) { }

    [BindMethod]
    public void MethodVector4(Vector4 parameter) { }

    [BindMethod]
    public void MethodVector4I(Vector4I parameter) { }

    [BindMethod]
    public void MethodBasis(Basis parameter) { }

    [BindMethod]
    public void MethodQuaternion(Quaternion parameter) { }

    [BindMethod]
    public void MethodTransform3D(Transform3D parameter) { }

    [BindMethod]
    public void MethodProjection(Projection parameter) { }

    [BindMethod]
    public void MethodAabb(Aabb parameter) { }

    [BindMethod]
    public void MethodColor(Color parameter) { }

    [BindMethod]
    public void MethodPlane(Plane parameter) { }

    [BindMethod]
    public void MethodCallable(Callable parameter) { }

    [BindMethod]
    public void MethodSignal(Signal parameter) { }

    [BindMethod]
    public void MethodGodotObject(GodotObject parameter) { }

    [BindMethod]
    public void MethodStringName(StringName parameter) { }

    [BindMethod]
    public void MethodNodePath(NodePath parameter) { }

    [BindMethod]
    public void MethodRid(Rid parameter) { }

    [BindMethod]
    public void MethodGodotDictionary(GodotDictionary parameter) { }

    [BindMethod]
    public void MethodGodotArray(GodotArray parameter) { }

    [BindMethod]
    public void MethodPackedByteArray(PackedByteArray parameter) { }

    [BindMethod]
    public void MethodPackedInt32Array(PackedInt32Array parameter) { }

    [BindMethod]
    public void MethodPackedInt64Array(PackedInt64Array parameter) { }

    [BindMethod]
    public void MethodPackedFloat32Array(PackedFloat32Array parameter) { }

    [BindMethod]
    public void MethodPackedFloat64Array(PackedFloat64Array parameter) { }

    [BindMethod]
    public void MethodPackedStringArray(PackedStringArray parameter) { }

    [BindMethod]
    public void MethodPackedVector2Array(PackedVector2Array parameter) { }

    [BindMethod]
    public void MethodPackedVector3Array(PackedVector3Array parameter) { }

    [BindMethod]
    public void MethodPackedColorArray(PackedColorArray parameter) { }

    [BindMethod]
    public void MethodPackedVector4Array(PackedVector4Array parameter) { }

    [BindMethod]
    public void MethodObject({|#0:object|} parameter) { }
}

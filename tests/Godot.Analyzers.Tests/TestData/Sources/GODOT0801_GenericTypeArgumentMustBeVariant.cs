using System;
using Godot;
using Godot.Collections;

public class MyType
{
    public void MethodCallsError()
    {
        Method<{|GODOT0801:object|}>();
    }

    public void MethodCallsOk()
    {
        Method<bool>();
        Method<char>();
        Method<sbyte>();
        Method<byte>();
        Method<short>();
        Method<ushort>();
        Method<int>();
        Method<uint>();
        Method<long>();
        Method<ulong>();
        Method<float>();
        Method<double>();
        Method<string>();
        Method<Vector2>();
        Method<Vector2I>();
        Method<Rect2>();
        Method<Rect2I>();
        Method<Transform2D>();
        Method<Vector3>();
        Method<Vector3I>();
        Method<Vector4>();
        Method<Vector4I>();
        Method<Basis>();
        Method<Quaternion>();
        Method<Transform3D>();
        Method<Projection>();
        Method<Aabb>();
        Method<Color>();
        Method<Plane>();
        Method<Callable>();
        Method<Signal>();
        Method<GodotObject>();
        Method<StringName>();
        Method<NodePath>();
        Method<Rid>();
        Method<GodotDictionary>();
        Method<GodotArray>();
        Method<PackedByteArray>();
        Method<PackedInt32Array>();
        Method<PackedInt64Array>();
        Method<PackedFloat32Array>();
        Method<PackedFloat64Array>();
        Method<PackedStringArray>();
        Method<PackedVector2Array>();
        Method<PackedVector3Array>();
        Method<PackedColorArray>();
        Method<PackedVector4Array>();
    }

    public void MethodCallDynamic()
    {
        dynamic self = this;
        self.Method<object>();
    }

    public void Method<[MustBeVariant] T>() { }

    public void MustBeVariantClasses()
    {
        new ClassWithGenericVariant<bool>();
        new ClassWithGenericVariant<char>();
        new ClassWithGenericVariant<sbyte>();
        new ClassWithGenericVariant<byte>();
        new ClassWithGenericVariant<short>();
        new ClassWithGenericVariant<ushort>();
        new ClassWithGenericVariant<int>();
        new ClassWithGenericVariant<uint>();
        new ClassWithGenericVariant<long>();
        new ClassWithGenericVariant<ulong>();
        new ClassWithGenericVariant<float>();
        new ClassWithGenericVariant<double>();
        new ClassWithGenericVariant<string>();
        new ClassWithGenericVariant<Vector2>();
        new ClassWithGenericVariant<Vector2I>();
        new ClassWithGenericVariant<Rect2>();
        new ClassWithGenericVariant<Rect2I>();
        new ClassWithGenericVariant<Transform2D>();
        new ClassWithGenericVariant<Vector3>();
        new ClassWithGenericVariant<Vector3I>();
        new ClassWithGenericVariant<Vector4>();
        new ClassWithGenericVariant<Vector4I>();
        new ClassWithGenericVariant<Basis>();
        new ClassWithGenericVariant<Quaternion>();
        new ClassWithGenericVariant<Transform3D>();
        new ClassWithGenericVariant<Projection>();
        new ClassWithGenericVariant<Aabb>();
        new ClassWithGenericVariant<Color>();
        new ClassWithGenericVariant<Plane>();
        new ClassWithGenericVariant<Callable>();
        new ClassWithGenericVariant<Signal>();
        new ClassWithGenericVariant<GodotObject>();
        new ClassWithGenericVariant<StringName>();
        new ClassWithGenericVariant<NodePath>();
        new ClassWithGenericVariant<Rid>();
        new ClassWithGenericVariant<GodotDictionary>();
        new ClassWithGenericVariant<GodotArray>();
        new ClassWithGenericVariant<PackedByteArray>();
        new ClassWithGenericVariant<PackedInt32Array>();
        new ClassWithGenericVariant<PackedInt64Array>();
        new ClassWithGenericVariant<PackedFloat32Array>();
        new ClassWithGenericVariant<PackedFloat64Array>();
        new ClassWithGenericVariant<PackedStringArray>();
        new ClassWithGenericVariant<PackedVector2Array>();
        new ClassWithGenericVariant<PackedVector3Array>();
        new ClassWithGenericVariant<PackedColorArray>();
        new ClassWithGenericVariant<PackedVector4Array>();

        new ClassWithGenericVariant<{|GODOT0801:object|}>();
    }
}

public class ClassWithGenericVariant<[MustBeVariant] T> { }

public class MustBeVariantAnnotatedMethods
{
    [GenericTypeAttribute<bool>]
    public void MethodWithAttributeBool() { }

    [GenericTypeAttribute<char>]
    public void MethodWithAttributeChar() { }

    [GenericTypeAttribute<sbyte>]
    public void MethodWithAttributeSByte() { }

    [GenericTypeAttribute<byte>]
    public void MethodWithAttributeByte() { }

    [GenericTypeAttribute<short>]
    public void MethodWithAttributeInt16() { }

    [GenericTypeAttribute<ushort>]
    public void MethodWithAttributeUInt16() { }

    [GenericTypeAttribute<int>]
    public void MethodWithAttributeInt32() { }

    [GenericTypeAttribute<uint>]
    public void MethodWithAttributeUInt32() { }

    [GenericTypeAttribute<long>]
    public void MethodWithAttributeInt64() { }

    [GenericTypeAttribute<ulong>]
    public void MethodWithAttributeUInt64() { }

    [GenericTypeAttribute<float>]
    public void MethodWithAttributeSingle() { }

    [GenericTypeAttribute<double>]
    public void MethodWithAttributeDouble() { }

    [GenericTypeAttribute<string>]
    public void MethodWithAttributeString() { }

    [GenericTypeAttribute<Vector2>]
    public void MethodWithAttributeVector2() { }

    [GenericTypeAttribute<Vector2I>]
    public void MethodWithAttributeVector2I() { }

    [GenericTypeAttribute<Rect2>]
    public void MethodWithAttributeRect2() { }

    [GenericTypeAttribute<Rect2I>]
    public void MethodWithAttributeRect2I() { }

    [GenericTypeAttribute<Transform2D>]
    public void MethodWithAttributeTransform2D() { }

    [GenericTypeAttribute<Vector3>]
    public void MethodWithAttributeVector3() { }

    [GenericTypeAttribute<Vector3I>]
    public void MethodWithAttributeVector3I() { }

    [GenericTypeAttribute<Vector4>]
    public void MethodWithAttributeVector4() { }

    [GenericTypeAttribute<Vector4I>]
    public void MethodWithAttributeVector4I() { }

    [GenericTypeAttribute<Basis>]
    public void MethodWithAttributeBasis() { }

    [GenericTypeAttribute<Quaternion>]
    public void MethodWithAttributeQuaternion() { }

    [GenericTypeAttribute<Transform3D>]
    public void MethodWithAttributeTransform3D() { }

    [GenericTypeAttribute<Projection>]
    public void MethodWithAttributeProjection() { }

    [GenericTypeAttribute<Aabb>]
    public void MethodWithAttributeAabb() { }

    [GenericTypeAttribute<Color>]
    public void MethodWithAttributeColor() { }

    [GenericTypeAttribute<Plane>]
    public void MethodWithAttributePlane() { }

    [GenericTypeAttribute<Callable>]
    public void MethodWithAttributeCallable() { }

    [GenericTypeAttribute<Signal>]
    public void MethodWithAttributeSignal() { }

    [GenericTypeAttribute<GodotObject>]
    public void MethodWithAttributeGodotObject() { }

    [GenericTypeAttribute<StringName>]
    public void MethodWithAttributeStringName() { }

    [GenericTypeAttribute<NodePath>]
    public void MethodWithAttributeNodePath() { }

    [GenericTypeAttribute<Rid>]
    public void MethodWithAttributeRid() { }

    [GenericTypeAttribute<GodotDictionary>]
    public void MethodWithAttributeGodotDictionary() { }

    [GenericTypeAttribute<GodotArray>]
    public void MethodWithAttributeGodotArray() { }

    [GenericTypeAttribute<PackedByteArray>]
    public void MethodWithAttributePackedByteArray() { }

    [GenericTypeAttribute<PackedInt32Array>]
    public void MethodWithAttributePackedInt32Array() { }

    [GenericTypeAttribute<PackedInt64Array>]
    public void MethodWithAttributePackedInt64Array() { }

    [GenericTypeAttribute<PackedFloat32Array>]
    public void MethodWithAttributePackedFloat32Array() { }

    [GenericTypeAttribute<PackedFloat64Array>]
    public void MethodWithAttributePackedFloat64Array() { }

    [GenericTypeAttribute<PackedStringArray>]
    public void MethodWithAttributePackedStringArray() { }

    [GenericTypeAttribute<PackedVector2Array>]
    public void MethodWithAttributePackedVector2Array() { }

    [GenericTypeAttribute<PackedVector3Array>]
    public void MethodWithAttributePackedVector3Array() { }

    [GenericTypeAttribute<PackedColorArray>]
    public void MethodWithAttributePackedColorArray() { }

    [GenericTypeAttribute<PackedVector4Array>]
    public void MethodWithAttributePackedVector4Array() { }

    [GenericTypeAttribute<{|GODOT0801:object|}>]
    public void MethodWithWrongAttribute() { }

    [NestedGenericTypeAttributeContainer.NestedGenericTypeAttribute<bool>]
    public void MethodWithNestedAttribute() { }
}

[GenericTypeAttribute<bool>]
public class ClassVariantAnnotatedBool { }

[GenericTypeAttribute<char>]
public class ClassVariantAnnotatedChar { }

[GenericTypeAttribute<sbyte>]
public class ClassVariantAnnotatedSByte { }

[GenericTypeAttribute<byte>]
public class ClassVariantAnnotatedByte { }

[GenericTypeAttribute<short>]
public class ClassVariantAnnotatedInt16 { }

[GenericTypeAttribute<ushort>]
public class ClassVariantAnnotatedUInt16 { }

[GenericTypeAttribute<int>]
public class ClassVariantAnnotatedInt32 { }

[GenericTypeAttribute<uint>]
public class ClassVariantAnnotatedUInt32 { }

[GenericTypeAttribute<long>]
public class ClassVariantAnnotatedInt64 { }

[GenericTypeAttribute<ulong>]
public class ClassVariantAnnotatedUInt64 { }

[GenericTypeAttribute<float>]
public class ClassVariantAnnotatedSingle { }

[GenericTypeAttribute<double>]
public class ClassVariantAnnotatedDouble { }

[GenericTypeAttribute<string>]
public class ClassVariantAnnotatedString { }

[GenericTypeAttribute<Vector2>]
public class ClassVariantAnnotatedVector2 { }

[GenericTypeAttribute<Vector2I>]
public class ClassVariantAnnotatedVector2I { }

[GenericTypeAttribute<Rect2>]
public class ClassVariantAnnotatedRect2 { }

[GenericTypeAttribute<Rect2I>]
public class ClassVariantAnnotatedRect2I { }

[GenericTypeAttribute<Transform2D>]
public class ClassVariantAnnotatedTransform2D { }

[GenericTypeAttribute<Vector3>]
public class ClassVariantAnnotatedVector3 { }

[GenericTypeAttribute<Vector3I>]
public class ClassVariantAnnotatedVector3I { }

[GenericTypeAttribute<Vector4>]
public class ClassVariantAnnotatedVector4 { }

[GenericTypeAttribute<Vector4I>]
public class ClassVariantAnnotatedVector4I { }

[GenericTypeAttribute<Basis>]
public class ClassVariantAnnotatedBasis { }

[GenericTypeAttribute<Quaternion>]
public class ClassVariantAnnotatedQuaternion { }

[GenericTypeAttribute<Transform3D>]
public class ClassVariantAnnotatedTransform3D { }

[GenericTypeAttribute<Projection>]
public class ClassVariantAnnotatedProjection { }

[GenericTypeAttribute<Aabb>]
public class ClassVariantAnnotatedAabb { }

[GenericTypeAttribute<Color>]
public class ClassVariantAnnotatedColor { }

[GenericTypeAttribute<Plane>]
public class ClassVariantAnnotatedPlane { }

[GenericTypeAttribute<Callable>]
public class ClassVariantAnnotatedCallable { }

[GenericTypeAttribute<Signal>]
public class ClassVariantAnnotatedSignal { }

[GenericTypeAttribute<GodotObject>]
public class ClassVariantAnnotatedGodotObject { }

[GenericTypeAttribute<StringName>]
public class ClassVariantAnnotatedStringName { }

[GenericTypeAttribute<NodePath>]
public class ClassVariantAnnotatedNodePath { }

[GenericTypeAttribute<Rid>]
public class ClassVariantAnnotatedRid { }

[GenericTypeAttribute<GodotDictionary>]
public class ClassVariantAnnotatedGodotDictionary { }

[GenericTypeAttribute<GodotArray>]
public class ClassVariantAnnotatedGodotArray { }

[GenericTypeAttribute<PackedByteArray>]
public class ClassVariantAnnotatedPackedByteArray { }

[GenericTypeAttribute<PackedInt32Array>]
public class ClassVariantAnnotatedPackedInt32Array { }

[GenericTypeAttribute<PackedInt64Array>]
public class ClassVariantAnnotatedPackedInt64Array { }

[GenericTypeAttribute<PackedFloat32Array>]
public class ClassVariantAnnotatedPackedFloat32Array { }

[GenericTypeAttribute<PackedFloat64Array>]
public class ClassVariantAnnotatedPackedFloat64Array { }

[GenericTypeAttribute<PackedStringArray>]
public class ClassVariantAnnotatedPackedStringArray { }

[GenericTypeAttribute<PackedVector2Array>]
public class ClassVariantAnnotatedPackedVector2Array { }

[GenericTypeAttribute<PackedVector3Array>]
public class ClassVariantAnnotatedPackedVector3Array { }

[GenericTypeAttribute<PackedColorArray>]
public class ClassVariantAnnotatedPackedColorArray { }

[GenericTypeAttribute<PackedVector4Array>]
public class ClassVariantAnnotatedPackedVector4Array { }

[GenericTypeAttribute<{|GODOT0801:object|}>]
public class ClassNonVariantAnnotated { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class GenericTypeAttribute<[MustBeVariant] T> : Attribute { }

public class NestedGenericTypeAttributeContainer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class NestedGenericTypeAttribute<[MustBeVariant] T> : Attribute { }
}

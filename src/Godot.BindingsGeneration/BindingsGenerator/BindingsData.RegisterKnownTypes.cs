using System;
using Godot.BindingsGeneration.Marshallers;
using Godot.BindingsGeneration.Reflection;

namespace Godot.BindingsGeneration;

partial class BindingsData
{
    private void RegisterKnownTypes()
    {
        // Primitive types.

        RegisterType("bool", KnownTypes.SystemBoolean);
        RegisterType("int", KnownTypes.SystemInt64);
        RegisterType("float", KnownTypes.SystemDouble);

        RegisterMetaType("int", "int", KnownTypes.SystemInt32, KnownTypes.SystemInt64);
        RegisterMetaType("int", "int8", KnownTypes.SystemSByte, KnownTypes.SystemInt64);
        RegisterMetaType("int", "int16", KnownTypes.SystemInt16, KnownTypes.SystemInt64);
        RegisterMetaType("int", "int32", KnownTypes.SystemInt32, KnownTypes.SystemInt64);
        RegisterMetaType("int", "int64", KnownTypes.SystemInt64, KnownTypes.SystemInt64);
        RegisterMetaType("int", "uint8", KnownTypes.SystemByte, KnownTypes.SystemInt64);
        RegisterMetaType("int", "uint16", KnownTypes.SystemUInt16, KnownTypes.SystemInt64);
        RegisterMetaType("int", "uint32", KnownTypes.SystemUInt32, KnownTypes.SystemInt64);
        RegisterMetaType("int", "uint64", KnownTypes.SystemUInt64, KnownTypes.SystemInt64);

        RegisterMetaType("float", "half", KnownTypes.SystemHalf, KnownTypes.SystemDouble);
        RegisterMetaType("float", "float", KnownTypes.SystemSingle, KnownTypes.SystemDouble);
        RegisterMetaType("float", "double", KnownTypes.SystemDouble, KnownTypes.SystemDouble);

        RegisterMetaType("int", "char16", KnownTypes.SystemChar, KnownTypes.SystemInt64);
        RegisterMetaType("int", "char32", KnownTypes.SystemTextRune, KnownTypes.SystemInt64);

        RegisterPtrMarshaller(KnownTypes.SystemBoolean, new BlittablePtrMarshallerWriter(KnownTypes.SystemBoolean));

        RegisterPtrMarshaller(KnownTypes.SystemSByte, IntegerPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemInt16, IntegerPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemInt32, IntegerPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemInt64, IntegerPtrMarshallerWriter.Instance);

        RegisterPtrMarshaller(KnownTypes.SystemByte, IntegerPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemUInt16, IntegerPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemUInt32, IntegerPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemUInt64, IntegerPtrMarshallerWriter.Instance);

        RegisterPtrMarshaller(KnownTypes.SystemHalf, FloatingPointPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemSingle, FloatingPointPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemDouble, FloatingPointPtrMarshallerWriter.Instance);

        RegisterPtrMarshaller(KnownTypes.SystemChar, IntegerPtrMarshallerWriter.Instance);
        RegisterPtrMarshaller(KnownTypes.SystemTextRune, IntegerPtrMarshallerWriter.Instance);

        RegisterVariantMarshaller(KnownTypes.SystemSByte, IntegerVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemInt16, IntegerVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemInt32, IntegerVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemInt64, IntegerVariantMarshallerWriter.Instance);

        RegisterVariantMarshaller(KnownTypes.SystemByte, IntegerVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemUInt16, IntegerVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemUInt32, IntegerVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemUInt64, IntegerVariantMarshallerWriter.Instance);

        RegisterVariantMarshaller(KnownTypes.SystemHalf, FloatingPointVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemSingle, FloatingPointVariantMarshallerWriter.Instance);
        RegisterVariantMarshaller(KnownTypes.SystemDouble, FloatingPointVariantMarshallerWriter.Instance);

        RegisterDefaultValueParser(KnownTypes.SystemBoolean, BooleanDefaultValueParser.Instance);

        RegisterDefaultValueParser(KnownTypes.SystemSByte, NumberDefaultValueParser<sbyte>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemInt16, NumberDefaultValueParser<short>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemInt32, NumberDefaultValueParser<int>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemInt64, NumberDefaultValueParser<long>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemByte, NumberDefaultValueParser<byte>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemUInt16, NumberDefaultValueParser<ushort>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemUInt32, NumberDefaultValueParser<uint>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemUInt64, NumberDefaultValueParser<ulong>.Instance);

        RegisterDefaultValueParser(KnownTypes.SystemHalf, NumberDefaultValueParser<Half>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemSingle, NumberDefaultValueParser<float>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemDouble, NumberDefaultValueParser<double>.Instance);

        RegisterDefaultValueParser(KnownTypes.SystemChar, NumberDefaultValueParser<char>.Instance);
        RegisterDefaultValueParser(KnownTypes.SystemTextRune, RuneDefaultValueParser.Instance);

        RegisterDefaultValueParser(KnownTypes.SystemEnum, NumberDefaultValueParser<long>.Instance);

        // Godot built-in types.

        RegisterType("AABB", KnownTypes.GodotAabb);
        RegisterType("Basis", KnownTypes.GodotBasis);
        RegisterType("Callable", KnownTypes.GodotCallable, KnownTypes.NativeGodotCallable);
        RegisterType("Color", KnownTypes.GodotColor);
        RegisterType("NodePath", KnownTypes.GodotNodePath, KnownTypes.NativeGodotNodePath);
        RegisterType("Plane", KnownTypes.GodotPlane);
        RegisterType("Projection", KnownTypes.GodotProjection);
        RegisterType("Quaternion", KnownTypes.GodotQuaternion);
        RegisterType("Rect2", KnownTypes.GodotRect2);
        RegisterType("Rect2i", KnownTypes.GodotRect2I);
        RegisterType("RID", KnownTypes.GodotRid);
        RegisterType("Signal", KnownTypes.GodotSignal, KnownTypes.NativeGodotSignal);
        RegisterType("String", KnownTypes.SystemString, KnownTypes.NativeGodotString);
        RegisterType("StringName", KnownTypes.GodotStringName, KnownTypes.NativeGodotStringName);
        RegisterType("Transform2D", KnownTypes.GodotTransform2D);
        RegisterType("Transform3D", KnownTypes.GodotTransform3D);
        RegisterType("Vector2", KnownTypes.GodotVector2);
        RegisterType("Vector2i", KnownTypes.GodotVector2I);
        RegisterType("Vector3", KnownTypes.GodotVector3);
        RegisterType("Vector3i", KnownTypes.GodotVector3I);
        RegisterType("Vector4", KnownTypes.GodotVector4);
        RegisterType("Vector4i", KnownTypes.GodotVector4I);
        RegisterType("Variant", KnownTypes.GodotVariant, KnownTypes.NativeGodotVariant);

        _typeDB.RegisterTypeName("Vector3.Axis", new EnumInfo("Vector3.Axis", "Godot"));

        RegisterPtrMarshaller(KnownTypes.GodotAabb, new BlittablePtrMarshallerWriter(KnownTypes.GodotAabb));
        RegisterPtrMarshaller(KnownTypes.GodotBasis, new BlittablePtrMarshallerWriter(KnownTypes.GodotBasis));
        RegisterRuntimePtrMarshaller(KnownTypes.GodotCallable, "CallableMarshaller");
        RegisterPtrMarshaller(KnownTypes.GodotColor, new BlittablePtrMarshallerWriter(KnownTypes.GodotColor));
        RegisterPtrMarshaller(KnownTypes.GodotNodePath, new InteropStructPtrMarshallerWriter(KnownTypes.GodotNodePath, KnownTypes.NativeGodotNodePath.MakePointerType()));
        RegisterPtrMarshaller(KnownTypes.GodotPlane, new BlittablePtrMarshallerWriter(KnownTypes.GodotPlane));
        RegisterPtrMarshaller(KnownTypes.GodotProjection, new BlittablePtrMarshallerWriter(KnownTypes.GodotProjection));
        RegisterPtrMarshaller(KnownTypes.GodotQuaternion, new BlittablePtrMarshallerWriter(KnownTypes.GodotQuaternion));
        RegisterPtrMarshaller(KnownTypes.GodotRect2, new BlittablePtrMarshallerWriter(KnownTypes.GodotRect2));
        RegisterPtrMarshaller(KnownTypes.GodotRect2I, new BlittablePtrMarshallerWriter(KnownTypes.GodotRect2I));
        RegisterPtrMarshaller(KnownTypes.GodotRid, new BlittablePtrMarshallerWriter(KnownTypes.GodotRid));
        RegisterPtrMarshaller(KnownTypes.GodotSignal, new InteropStructPtrMarshallerWriter(KnownTypes.GodotSignal, KnownTypes.NativeGodotSignal.MakePointerType()));
        RegisterRuntimePtrMarshaller(KnownTypes.SystemString, "StringMarshaller");
        RegisterPtrMarshaller(KnownTypes.GodotStringName, new InteropStructPtrMarshallerWriter(KnownTypes.GodotStringName, KnownTypes.NativeGodotStringName.MakePointerType()));
        RegisterPtrMarshaller(KnownTypes.GodotTransform2D, new BlittablePtrMarshallerWriter(KnownTypes.GodotTransform2D));
        RegisterPtrMarshaller(KnownTypes.GodotTransform3D, new BlittablePtrMarshallerWriter(KnownTypes.GodotTransform3D));
        RegisterPtrMarshaller(KnownTypes.GodotVector2, new BlittablePtrMarshallerWriter(KnownTypes.GodotVector2));
        RegisterPtrMarshaller(KnownTypes.GodotVector2I, new BlittablePtrMarshallerWriter(KnownTypes.GodotVector2I));
        RegisterPtrMarshaller(KnownTypes.GodotVector3, new BlittablePtrMarshallerWriter(KnownTypes.GodotVector3));
        RegisterPtrMarshaller(KnownTypes.GodotVector3I, new BlittablePtrMarshallerWriter(KnownTypes.GodotVector3I));
        RegisterPtrMarshaller(KnownTypes.GodotVector4, new BlittablePtrMarshallerWriter(KnownTypes.GodotVector4));
        RegisterPtrMarshaller(KnownTypes.GodotVector4I, new BlittablePtrMarshallerWriter(KnownTypes.GodotVector4I));
        RegisterPtrMarshaller(KnownTypes.GodotVariant, new InteropStructPtrMarshallerWriter(KnownTypes.GodotVariant, KnownTypes.NativeGodotVariant.MakePointerType()));

        RegisterPtrMarshaller(KnownTypes.NativeGodotCallable, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotCallable));
        RegisterPtrMarshaller(KnownTypes.NativeGodotNodePath, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotNodePath));
        RegisterPtrMarshaller(KnownTypes.NativeGodotSignal, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotSignal));
        RegisterPtrMarshaller(KnownTypes.NativeGodotString, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotString));
        RegisterPtrMarshaller(KnownTypes.NativeGodotStringName, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotStringName));
        RegisterPtrMarshaller(KnownTypes.NativeGodotVariant, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotVariant));

        RegisterVariantMarshaller(KnownTypes.GodotAabb, new InteropStructVariantMarshallerWriter(KnownTypes.GodotAabb, "Aabb"));
        RegisterVariantMarshaller(KnownTypes.GodotBasis, new InteropStructVariantMarshallerWriter(KnownTypes.GodotBasis, "Basis"));
        RegisterRuntimeVariantMarshaller(KnownTypes.GodotCallable, "CallableMarshaller");
        RegisterVariantMarshaller(KnownTypes.GodotColor, new InteropStructVariantMarshallerWriter(KnownTypes.GodotColor, "Color"));
        RegisterVariantMarshaller(KnownTypes.GodotNodePath, new InteropStructVariantMarshallerWriter(KnownTypes.GodotNodePath, "NodePath", KnownTypes.NativeGodotNodePath, createMethodSuffix: "TakingOwnership"));
        RegisterVariantMarshaller(KnownTypes.GodotPlane, new InteropStructVariantMarshallerWriter(KnownTypes.GodotPlane, "Plane"));
        RegisterVariantMarshaller(KnownTypes.GodotProjection, new InteropStructVariantMarshallerWriter(KnownTypes.GodotProjection, "Projection"));
        RegisterVariantMarshaller(KnownTypes.GodotQuaternion, new InteropStructVariantMarshallerWriter(KnownTypes.GodotQuaternion, "Quaternion"));
        RegisterVariantMarshaller(KnownTypes.GodotRect2, new InteropStructVariantMarshallerWriter(KnownTypes.GodotRect2, "Rect2"));
        RegisterVariantMarshaller(KnownTypes.GodotRect2I, new InteropStructVariantMarshallerWriter(KnownTypes.GodotRect2I, "Rect2I"));
        RegisterVariantMarshaller(KnownTypes.GodotRid, new InteropStructVariantMarshallerWriter(KnownTypes.GodotRid, "Rid"));
        RegisterVariantMarshaller(KnownTypes.GodotSignal, new InteropStructVariantMarshallerWriter(KnownTypes.GodotSignal, "Signal", KnownTypes.NativeGodotSignal, createMethodSuffix: "TakingOwnership"));
        RegisterRuntimeVariantMarshaller(KnownTypes.SystemString, "StringMarshaller");
        RegisterVariantMarshaller(KnownTypes.GodotStringName, new InteropStructVariantMarshallerWriter(KnownTypes.GodotStringName, "StringName", KnownTypes.NativeGodotStringName, createMethodSuffix: "TakingOwnership"));
        RegisterVariantMarshaller(KnownTypes.GodotTransform2D, new InteropStructVariantMarshallerWriter(KnownTypes.GodotTransform2D, "Transform2D"));
        RegisterVariantMarshaller(KnownTypes.GodotTransform3D, new InteropStructVariantMarshallerWriter(KnownTypes.GodotTransform3D, "Transform3D"));
        RegisterVariantMarshaller(KnownTypes.GodotVector2, new InteropStructVariantMarshallerWriter(KnownTypes.GodotVector2, "Vector2"));
        RegisterVariantMarshaller(KnownTypes.GodotVector2I, new InteropStructVariantMarshallerWriter(KnownTypes.GodotVector2I, "Vector2I"));
        RegisterVariantMarshaller(KnownTypes.GodotVector3, new InteropStructVariantMarshallerWriter(KnownTypes.GodotVector3, "Vector3"));
        RegisterVariantMarshaller(KnownTypes.GodotVector3I, new InteropStructVariantMarshallerWriter(KnownTypes.GodotVector3I, "Vector3I"));
        RegisterVariantMarshaller(KnownTypes.GodotVector4, new InteropStructVariantMarshallerWriter(KnownTypes.GodotVector4, "Vector4"));
        RegisterVariantMarshaller(KnownTypes.GodotVector4I, new InteropStructVariantMarshallerWriter(KnownTypes.GodotVector4I, "Vector4I"));
        RegisterVariantMarshaller(KnownTypes.GodotVariant, new VariantVariantMarshallerWriter(KnownTypes.GodotVariant));

        RegisterVariantMarshaller(KnownTypes.NativeGodotCallable, new InteropStructVariantMarshallerWriter(KnownTypes.NativeGodotCallable, "Callable", createMethodSuffix: "TakingOwnership"));
        RegisterVariantMarshaller(KnownTypes.NativeGodotNodePath, new InteropStructVariantMarshallerWriter(KnownTypes.NativeGodotNodePath, "NodePath", createMethodSuffix: "TakingOwnership"));
        RegisterVariantMarshaller(KnownTypes.NativeGodotSignal, new InteropStructVariantMarshallerWriter(KnownTypes.NativeGodotSignal, "Signal", createMethodSuffix: "TakingOwnership"));
        RegisterVariantMarshaller(KnownTypes.NativeGodotString, new InteropStructVariantMarshallerWriter(KnownTypes.NativeGodotString, "String", createMethodSuffix: "TakingOwnership"));
        RegisterVariantMarshaller(KnownTypes.NativeGodotStringName, new InteropStructVariantMarshallerWriter(KnownTypes.NativeGodotStringName, "StringName", createMethodSuffix: "TakingOwnership"));
        RegisterVariantMarshaller(KnownTypes.NativeGodotVariant, new VariantVariantMarshallerWriter(KnownTypes.NativeGodotVariant));

        RegisterDefaultValueParser(KnownTypes.GodotAabb, new EmptyConstructorDefaultValueParser("AABB"));
        RegisterDefaultValueParser(KnownTypes.GodotBasis, new EmptyConstructorDefaultValueParser("Basis"));
        RegisterDefaultValueParser(KnownTypes.GodotCallable, new EmptyConstructorDefaultValueParser("Callable"));
        RegisterDefaultValueParser(KnownTypes.GodotColor, new VectorDefaultValueParser(KnownTypes.GodotColor));
        RegisterDefaultValueParser(KnownTypes.GodotNodePath, new NodePathDefaultValueParser(KnownTypes.GodotNodePath));
        RegisterDefaultValueParser(KnownTypes.GodotPlane, new EmptyConstructorDefaultValueParser("Plane"));
        RegisterDefaultValueParser(KnownTypes.GodotProjection, new EmptyConstructorDefaultValueParser("Projection"));
        RegisterDefaultValueParser(KnownTypes.GodotQuaternion, new EmptyConstructorDefaultValueParser("Quaternion"));
        RegisterDefaultValueParser(KnownTypes.GodotRect2, new VectorDefaultValueParser(KnownTypes.GodotRect2));
        RegisterDefaultValueParser(KnownTypes.GodotRect2I, new VectorDefaultValueParser(KnownTypes.GodotRect2I));
        RegisterDefaultValueParser(KnownTypes.GodotRid, new EmptyConstructorDefaultValueParser("RID"));
        RegisterDefaultValueParser(KnownTypes.GodotSignal, new EmptyConstructorDefaultValueParser("Signal"));
        RegisterDefaultValueParser(KnownTypes.SystemString, new StringDefaultValueParser(KnownTypes.SystemString));
        RegisterDefaultValueParser(KnownTypes.GodotStringName, new StringDefaultValueParser(KnownTypes.GodotStringName));
        RegisterDefaultValueParser(KnownTypes.GodotTransform2D, new TransformDefaultValueParser(KnownTypes.GodotTransform2D));
        RegisterDefaultValueParser(KnownTypes.GodotTransform3D, new TransformDefaultValueParser(KnownTypes.GodotTransform3D));
        RegisterDefaultValueParser(KnownTypes.GodotVector2, new VectorDefaultValueParser(KnownTypes.GodotVector2));
        RegisterDefaultValueParser(KnownTypes.GodotVector2I, new VectorDefaultValueParser(KnownTypes.GodotVector2I));
        RegisterDefaultValueParser(KnownTypes.GodotVector3, new VectorDefaultValueParser(KnownTypes.GodotVector3));
        RegisterDefaultValueParser(KnownTypes.GodotVector3I, new VectorDefaultValueParser(KnownTypes.GodotVector3I));
        RegisterDefaultValueParser(KnownTypes.GodotVector4, new VectorDefaultValueParser(KnownTypes.GodotVector4));
        RegisterDefaultValueParser(KnownTypes.GodotVector4I, new VectorDefaultValueParser(KnownTypes.GodotVector4I));
        RegisterDefaultValueParser(KnownTypes.GodotVariant, new VariantDefaultValueParser(KnownTypes.GodotVariant));

        RegisterDefaultValueParser(KnownTypes.NativeGodotCallable, new EmptyConstructorDefaultValueParser("Callable"));
        RegisterDefaultValueParser(KnownTypes.NativeGodotNodePath, new NodePathDefaultValueParser(KnownTypes.NativeGodotNodePath));
        RegisterDefaultValueParser(KnownTypes.NativeGodotSignal, new EmptyConstructorDefaultValueParser("Signal"));
        RegisterDefaultValueParser(KnownTypes.NativeGodotString, new StringDefaultValueParser(KnownTypes.NativeGodotString));
        RegisterDefaultValueParser(KnownTypes.NativeGodotStringName, new StringDefaultValueParser(KnownTypes.NativeGodotStringName));
        RegisterDefaultValueParser(KnownTypes.NativeGodotVariant, new VariantDefaultValueParser(KnownTypes.NativeGodotVariant));

        // Godot Object-derived types.

        _typeDB.RegisterUnmanagedType(KnownTypes.GodotObject, KnownTypes.SystemIntPtr);
        _typeDB.RegisterUnmanagedType(KnownTypes.SystemIntPtr);

        RegisterRuntimePtrMarshaller(KnownTypes.GodotObject, "GodotObjectMarshaller");
        RegisterPtrMarshaller(KnownTypes.SystemIntPtr, new BlittablePtrMarshallerWriter(KnownTypes.SystemIntPtr));

        RegisterRuntimeVariantMarshaller(KnownTypes.GodotObject, "GodotObjectMarshaller");

        // Packed arrays.
        RegisterPackedArray("PackedByteArray", KnownTypes.GodotPackedByteArray, KnownTypes.NativeGodotPackedByteArray);
        RegisterPackedArray("PackedInt32Array", KnownTypes.GodotPackedInt32Array, KnownTypes.NativeGodotPackedInt32Array);
        RegisterPackedArray("PackedInt64Array", KnownTypes.GodotPackedInt64Array, KnownTypes.NativeGodotPackedInt64Array);
        RegisterPackedArray("PackedFloat32Array", KnownTypes.GodotPackedFloat32Array, KnownTypes.NativeGodotPackedFloat32Array);
        RegisterPackedArray("PackedFloat64Array", KnownTypes.GodotPackedFloat64Array, KnownTypes.NativeGodotPackedFloat64Array);
        RegisterPackedArray("PackedStringArray", KnownTypes.GodotPackedStringArray, KnownTypes.NativeGodotPackedStringArray);
        RegisterPackedArray("PackedVector2Array", KnownTypes.GodotPackedVector2Array, KnownTypes.NativeGodotPackedVector2Array);
        RegisterPackedArray("PackedVector3Array", KnownTypes.GodotPackedVector3Array, KnownTypes.NativeGodotPackedVector3Array);
        RegisterPackedArray("PackedColorArray", KnownTypes.GodotPackedColorArray, KnownTypes.NativeGodotPackedColorArray);
        RegisterPackedArray("PackedVector4Array", KnownTypes.GodotPackedVector4Array, KnownTypes.NativeGodotPackedVector4Array);
        void RegisterPackedArray(string engineTypeName, TypeInfo type, TypeInfo unmanagedType)
        {
            RegisterType(engineTypeName, type, unmanagedType);

            RegisterPtrMarshaller(type, new InteropStructPtrMarshallerWriter(type, unmanagedType.MakePointerType()));
            RegisterPtrMarshaller(unmanagedType, new InteropStructPtrMarshallerWriter(unmanagedType));

            RegisterVariantMarshaller(type, new InteropStructVariantMarshallerWriter(type, engineTypeName, unmanagedType, createMethodSuffix: "Copying"));
            RegisterVariantMarshaller(unmanagedType, new InteropStructVariantMarshallerWriter(unmanagedType, engineTypeName, createMethodSuffix: "Copying"));

            RegisterDefaultValueParser(type, new PackedArrayDefaultValueParser(type));
        }

        // Godot collections.

        RegisterType("Array", KnownTypes.GodotArray, KnownTypes.NativeGodotArray);
        RegisterType("Dictionary", KnownTypes.GodotDictionary, KnownTypes.NativeGodotDictionary);

        _typeDB.RegisterUnmanagedType(KnownTypes.GodotArrayGeneric, KnownTypes.NativeGodotArray);
        _typeDB.RegisterUnmanagedType(KnownTypes.GodotDictionaryGeneric, KnownTypes.NativeGodotDictionary);

        RegisterPtrMarshaller(KnownTypes.GodotArray, new GodotArrayAndDictionaryPtrMarshallerWriter(KnownTypes.GodotArray, KnownTypes.NativeGodotArray.MakePointerType()));
        RegisterPtrMarshaller(KnownTypes.GodotArrayGeneric, new GodotArrayAndDictionaryPtrMarshallerWriter(KnownTypes.GodotArrayGeneric, KnownTypes.NativeGodotArray.MakePointerType()));
        RegisterPtrMarshaller(KnownTypes.NativeGodotArray, new GodotArrayAndDictionaryPtrMarshallerWriter(KnownTypes.NativeGodotArray));

        RegisterPtrMarshaller(KnownTypes.GodotDictionary, new GodotArrayAndDictionaryPtrMarshallerWriter(KnownTypes.GodotDictionary, KnownTypes.NativeGodotDictionary.MakePointerType()));
        RegisterPtrMarshaller(KnownTypes.GodotDictionaryGeneric, new GodotArrayAndDictionaryPtrMarshallerWriter(KnownTypes.GodotDictionaryGeneric, KnownTypes.NativeGodotDictionary.MakePointerType()));
        RegisterPtrMarshaller(KnownTypes.NativeGodotDictionary, new GodotArrayAndDictionaryPtrMarshallerWriter(KnownTypes.NativeGodotDictionary));

        RegisterVariantMarshaller(KnownTypes.GodotArray, new InteropStructVariantMarshallerWriter(KnownTypes.GodotArray, "Array", KnownTypes.NativeGodotArray, createMethodSuffix: "Copying"));
        RegisterVariantMarshaller(KnownTypes.GodotArrayGeneric, new InteropStructVariantMarshallerWriter(KnownTypes.GodotArrayGeneric, "Array", KnownTypes.NativeGodotArray, createMethodSuffix: "Copying"));
        RegisterVariantMarshaller(KnownTypes.NativeGodotArray, new InteropStructVariantMarshallerWriter(KnownTypes.NativeGodotArray, "Array", createMethodSuffix: "Copying"));

        RegisterVariantMarshaller(KnownTypes.GodotDictionary, new InteropStructVariantMarshallerWriter(KnownTypes.GodotDictionary, "Dictionary", KnownTypes.NativeGodotDictionary, createMethodSuffix: "Copying"));
        RegisterVariantMarshaller(KnownTypes.GodotDictionaryGeneric, new InteropStructVariantMarshallerWriter(KnownTypes.GodotDictionaryGeneric, "Dictionary", KnownTypes.NativeGodotDictionary, createMethodSuffix: "Copying"));
        RegisterVariantMarshaller(KnownTypes.NativeGodotDictionary, new InteropStructVariantMarshallerWriter(KnownTypes.NativeGodotDictionary, "Dictionary", createMethodSuffix: "Copying"));

        RegisterDefaultValueParser(KnownTypes.GodotArray, new GodotArrayDefaultValueParser(KnownTypes.GodotArray));
        RegisterDefaultValueParser(KnownTypes.GodotArrayGeneric, new GodotArrayDefaultValueParser(KnownTypes.GodotArrayGeneric));
        RegisterDefaultValueParser(KnownTypes.NativeGodotArray, new GodotArrayDefaultValueParser(KnownTypes.NativeGodotArray));

        RegisterDefaultValueParser(KnownTypes.GodotDictionary, new GodotDictionaryDefaultValueParser(KnownTypes.GodotDictionary));
        RegisterDefaultValueParser(KnownTypes.GodotDictionaryGeneric, new GodotDictionaryDefaultValueParser(KnownTypes.GodotDictionaryGeneric));
        RegisterDefaultValueParser(KnownTypes.NativeGodotDictionary, new GodotDictionaryDefaultValueParser(KnownTypes.NativeGodotDictionary));

        // MEMBER MAPPINGS: We also need to register member mappings for some APIs that aren't generated.
        // We don't need to add every API here, only the ones that are referenced in the documentation.
        // For some APIs we can reference BCL APIs instead, but keep in mind the documentation is often
        // worded in a way that is specific to Godot, for example "String.length" is not the same as
        // "System.String.Length" because Godot Strings are UTF-32 and System.String is UTF-16, so the
        // documentation may mention this when talking about the length which would be misleading if we
        // referenced the BCL API instead.

        // Members (built-in types).
        RegisterMemberMapping(KnownTypes.GodotAabb, "position", "Position");
        RegisterMemberMapping(KnownTypes.GodotAabb, "size", "Size");
        RegisterMemberMapping(KnownTypes.GodotAabb, "end", "End");
        RegisterMemberMapping(KnownTypes.GodotBasis, "IDENTITY", "Identity");
        RegisterMemberMapping(KnownTypes.GodotBasis, "FLIP_X", "FlipX");
        RegisterMemberMapping(KnownTypes.GodotBasis, "FLIP_Y", "FlipY");
        RegisterMemberMapping(KnownTypes.GodotBasis, "FLIP_Z", "FlipZ");
        RegisterMemberMapping(KnownTypes.GodotCallable, "bind", "Bind");
        RegisterMemberMapping(KnownTypes.GodotCallable, "unbind", "Unbind");
        RegisterMemberMapping(KnownTypes.GodotCallable, "call", "Call");
        RegisterMemberMapping(KnownTypes.GodotCallable, "call_deferred", "CallDeferred");
        RegisterMemberMapping(KnownTypes.GodotColor, "r", "R");
        RegisterMemberMapping(KnownTypes.GodotColor, "g", "G");
        RegisterMemberMapping(KnownTypes.GodotColor, "b", "B");
        RegisterMemberMapping(KnownTypes.GodotColor, "a", "A");
        RegisterMemberMapping(KnownTypes.GodotColor, "r8", "R8");
        RegisterMemberMapping(KnownTypes.GodotColor, "g8", "G8");
        RegisterMemberMapping(KnownTypes.GodotColor, "b8", "B8");
        RegisterMemberMapping(KnownTypes.GodotColor, "a8", "A8");
        RegisterMemberMapping(KnownTypes.GodotColor, "h", "H");
        RegisterMemberMapping(KnownTypes.GodotColor, "s", "S");
        RegisterMemberMapping(KnownTypes.GodotColor, "v", "V");
        RegisterMemberMapping(KnownTypes.GodotNodePath, "get_as_property_path", "GetAsPropertyPath");
        RegisterMemberMapping(KnownTypes.GodotPlane, "normal", "Normal");
        RegisterMemberMapping(KnownTypes.GodotPlane, "d", "D");
        RegisterMemberMapping(KnownTypes.GodotPlane, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotPlane, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotPlane, "z", "Z");
        RegisterMemberMapping(KnownTypes.GodotPlane, "PLANE_YZ", "PlaneYZ");
        RegisterMemberMapping(KnownTypes.GodotPlane, "PLANE_XZ", "PlaneXZ");
        RegisterMemberMapping(KnownTypes.GodotPlane, "PLANE_XY", "PlaneXY");
        RegisterMemberMapping(KnownTypes.GodotProjection, "ZERO", "Zero");
        RegisterMemberMapping(KnownTypes.GodotProjection, "IDENTITY", "Identity");
        RegisterMemberMapping(KnownTypes.GodotQuaternion, "IDENTITY", "Identity");
        RegisterMemberMapping(KnownTypes.GodotQuaternion, "slerp", "Slerp");
        RegisterMemberMapping(KnownTypes.GodotRect2, "position", "Position");
        RegisterMemberMapping(KnownTypes.GodotRect2, "size", "Size");
        RegisterMemberMapping(KnownTypes.GodotRect2, "end", "End");
        RegisterMemberMapping(KnownTypes.GodotRect2I, "position", "Position");
        RegisterMemberMapping(KnownTypes.GodotRect2I, "size", "Size");
        RegisterMemberMapping(KnownTypes.GodotRect2I, "end", "End");
        RegisterMemberMapping(KnownTypes.GodotTransform2D, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotTransform2D, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotTransform2D, "origin", "Origin");
        RegisterMemberMapping(KnownTypes.GodotTransform2D, "IDENTITY", "Identity");
        RegisterMemberMapping(KnownTypes.GodotTransform2D, "FLIP_X", "FlipX");
        RegisterMemberMapping(KnownTypes.GodotTransform2D, "FLIP_Y", "FlipY");
        RegisterMemberMapping(KnownTypes.GodotTransform2D, "get_scale", "Scale");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "basis", "Basis");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "origin", "Origin");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "IDENTITY", "Identity");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "FLIP_X", "FlipX");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "FLIP_Y", "FlipY");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "FLIP_Z", "FlipZ");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "affine_inverse", "AffineInverse");
        RegisterMemberMapping(KnownTypes.GodotTransform3D, "orthonormalized", "Orthonormalized");
        RegisterMemberMapping(KnownTypes.GodotVector2, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotVector2, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotVector2, "ZERO", "Zero");
        RegisterMemberMapping(KnownTypes.GodotVector2, "ONE", "One");
        RegisterMemberMapping(KnownTypes.GodotVector2, "INF", "Inf");
        RegisterMemberMapping(KnownTypes.GodotVector2, "UP", "Up");
        RegisterMemberMapping(KnownTypes.GodotVector2, "DOWN", "Down");
        RegisterMemberMapping(KnownTypes.GodotVector2, "RIGHT", "Right");
        RegisterMemberMapping(KnownTypes.GodotVector2, "LEFT", "Left");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "MIN", "MinValue");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "MAX", "MaxValue");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "ZERO", "Zero");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "ONE", "One");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "UP", "Up");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "DOWN", "Down");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "RIGHT", "Right");
        RegisterMemberMapping(KnownTypes.GodotVector2I, "LEFT", "Left");
        RegisterMemberMapping(KnownTypes.GodotVector3, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotVector3, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotVector3, "z", "Z");
        RegisterMemberMapping(KnownTypes.GodotVector3, "ZERO", "Zero");
        RegisterMemberMapping(KnownTypes.GodotVector3, "ONE", "One");
        RegisterMemberMapping(KnownTypes.GodotVector3, "INF", "Inf");
        RegisterMemberMapping(KnownTypes.GodotVector3, "UP", "Up");
        RegisterMemberMapping(KnownTypes.GodotVector3, "DOWN", "Down");
        RegisterMemberMapping(KnownTypes.GodotVector3, "RIGHT", "Right");
        RegisterMemberMapping(KnownTypes.GodotVector3, "LEFT", "Left");
        RegisterMemberMapping(KnownTypes.GodotVector3, "FORWARD", "Forward");
        RegisterMemberMapping(KnownTypes.GodotVector3, "BACK", "Back");
        RegisterMemberMapping(KnownTypes.GodotVector3, "MODEL_LEFT", "ModelLeft");
        RegisterMemberMapping(KnownTypes.GodotVector3, "MODEL_RIGHT", "ModelRight");
        RegisterMemberMapping(KnownTypes.GodotVector3, "MODEL_TOP", "ModelTop");
        RegisterMemberMapping(KnownTypes.GodotVector3, "MODEL_BOTTOM", "ModelBottom");
        RegisterMemberMapping(KnownTypes.GodotVector3, "MODEL_FRONT", "ModelFront");
        RegisterMemberMapping(KnownTypes.GodotVector3, "MODEL_REAR", "ModelRear");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "z", "Z");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "MIN", "MinValue");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "MAX", "MaxValue");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "ZERO", "Zero");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "ONE", "One");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "UP", "Up");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "DOWN", "Down");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "RIGHT", "Right");
        RegisterMemberMapping(KnownTypes.GodotVector3I, "LEFT", "Left");
        RegisterMemberMapping(KnownTypes.GodotVector4, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotVector4, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotVector4, "z", "Z");
        RegisterMemberMapping(KnownTypes.GodotVector4, "w", "W");
        RegisterMemberMapping(KnownTypes.GodotVector4, "ZERO", "Zero");
        RegisterMemberMapping(KnownTypes.GodotVector4, "ONE", "One");
        RegisterMemberMapping(KnownTypes.GodotVector4, "INF", "Inf");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "x", "X");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "y", "Y");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "z", "Z");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "w", "W");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "MIN", "MinValue");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "MAX", "MaxValue");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "ZERO", "Zero");
        RegisterMemberMapping(KnownTypes.GodotVector4I, "ONE", "One");

        // Members (collections).
        RegisterMemberMapping(KnownTypes.GodotArray, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotArray, "shuffle", "Shuffle");
        RegisterMemberMapping(KnownTypes.GodotDictionary, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedByteArray, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedInt32Array, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedInt64Array, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedFloat32Array, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedFloat64Array, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedStringArray, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedVector2Array, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedVector3Array, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedColorArray, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedVector4Array, "size", "Count");
        RegisterMemberMapping(KnownTypes.GodotPackedInt32Array, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedInt64Array, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedFloat32Array, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedFloat64Array, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedStringArray, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedVector2Array, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedVector3Array, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedColorArray, "to_byte_array", "ToPackedByteArray");
        RegisterMemberMapping(KnownTypes.GodotPackedVector4Array, "to_byte_array", "ToPackedByteArray");

        // Members (Object).
        RegisterMemberMapping(KnownTypes.GodotObject, "free", "Free");
        RegisterMemberMapping(KnownTypes.GodotObject, "to_string", "ToString");
        RegisterMemberMapping(KnownTypes.GodotObject, "_get", "_Get");
        RegisterMemberMapping(KnownTypes.GodotObject, "_set", "_Set");
        RegisterMemberMapping(KnownTypes.GodotObject, "_get_property_list", "_GetPropertyList");
        RegisterMemberMapping(KnownTypes.GodotObject, "_property_can_revert", "_PropertyCanRevert");
        RegisterMemberMapping(KnownTypes.GodotObject, "_property_get_revert", "_PropertyGetRevert");
        RegisterMemberMapping(KnownTypes.GodotObject, "_validate_property", "_ValidateProperty");
        RegisterMemberMapping(KnownTypes.GodotObject, "_notification", "_Notification");
        RegisterGlobalMemberMapping(KnownTypes.GodotObject, "instance_from_id", "InstanceFromId");
        RegisterMemberMapping(KnownTypes.GodotObject, "get_instance_id", "GetInstanceId");
        RegisterGlobalMemberMapping(KnownTypes.GodotObject, "is_instance_valid", "IsInstanceValid");

        // Members (global scope).
        RegisterGlobalMemberMappingNoType("@GDScript.assert", "global::System.Diagnostics.Debug.Assert(bool)");
        RegisterGlobalMemberMappingNoType("@GDScript.TAU", "global::Godot.Mathf.Tau");
        RegisterGlobalMemberMappingNoType("@GDScript.PI", "global::Godot.Mathf.Pi");
        RegisterGlobalMemberMappingNoType("@GDScript.INF", "global::Godot.Mathf.Inf");
        RegisterGlobalMemberMappingNoType("@GDScript.NAN", "global::Godot.Mathf.NaN");
        RegisterGlobalMemberMappingNoType("@GlobalScope.abs", "global::Godot.Mathf.Abs(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.atan2", "global::Godot.Mathf.Atan2(float, float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.clamp", "global::Godot.Mathf.Clamp(float, float, float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.db_to_linear", "global::Godot.Mathf.DbToLinear(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.deg_to_rad", "global::Godot.Mathf.DegToRad(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.ease", "global::Godot.Mathf.Ease(float, float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.is_equal_approx", "global::Godot.Mathf.IsEqualApprox(float, float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.is_finite", "global::Godot.Mathf.IsFinite(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.is_inf", "global::Godot.Mathf.IsInf(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.is_nan", "global::Godot.Mathf.IsNaN(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.is_zero_approx", "global::Godot.Mathf.IsZeroApprox(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.lerp", "global::Godot.Mathf.Lerp(float, float, float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.linear_to_db", "global::Godot.Mathf.LinearToDb(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.posmod", "global::Godot.Mathf.PosMod(int, int)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.fposmod", "global::Godot.Mathf.PosMod(float, float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.rad_to_deg", "global::Godot.Mathf.RadToDeg(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.remap", "global::Godot.Mathf.Remap(float, float, float, float, float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.sign", "global::Godot.Mathf.Sign(float)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.bytes_to_var", "global::Godot.GD.BytesToVar");
        RegisterGlobalMemberMappingNoType("@GlobalScope.type_convert", "global::Godot.GD.TypeConvert");
        RegisterGlobalMemberMappingNoType("@GDScript.hash", "global::Godot.GD.Hash");
        RegisterGlobalMemberMappingNoType("@GDScript.load", "global::Godot.GD.Load");
        RegisterGlobalMemberMappingNoType("@GlobalScope.print", "global::Godot.GD.Print(string)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.print_verbose", "global::Godot.GD.PrintVerbose(string)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.push_error", "global::Godot.GD.PushError(string)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.push_warning", "global::Godot.GD.PushWarning(string)");
        RegisterGlobalMemberMappingNoType("@GlobalScope.randi", "global::Godot.GD.Randi");
        RegisterGlobalMemberMappingNoType("@GlobalScope.seed", "global::Godot.GD.Seed");
        RegisterGlobalMemberMappingNoType("@GlobalScope.str", "global::System.Object.ToString");
        RegisterGlobalMemberMappingNoType("@GlobalScope.str_to_var", "global::Godot.GD.StrToVar");
        RegisterGlobalMemberMappingNoType("@GlobalScope.var_to_bytes", "global::Godot.GD.VarToBytes");
        RegisterGlobalMemberMappingNoType("@GlobalScope.var_to_str", "global::Godot.GD.VarToStr");
        RegisterGlobalMemberMappingNoType("@GlobalScope.weakref", "global::Godot.GodotObject.WeakRef");

        void RegisterType(string engineTypeName, TypeInfo type, TypeInfo? unmanagedType = null)
        {
            _typeDB.RegisterTypeName(engineTypeName, type);
            _typeDB.RegisterUnmanagedType(type, unmanagedType ?? type);
            if (unmanagedType is not null && type != unmanagedType)
            {
                _typeDB.RegisterUnmanagedType(unmanagedType);
            }
        }

        void RegisterMetaType(string engineTypeName, string engineTypeMeta, TypeInfo type, TypeInfo? unmanagedType = null)
        {
            _typeDB.RegisterTypeMetaName(engineTypeName, engineTypeMeta, type);
            _typeDB.RegisterUnmanagedType(type, unmanagedType ?? type);
            if (unmanagedType is not null && type != unmanagedType)
            {
                _typeDB.RegisterUnmanagedType(unmanagedType);
            }
        }

        void RegisterPtrMarshaller(TypeInfo type, PtrMarshallerWriter marshaller)
        {
            _typeDB.RegisterPtrMarshaller(type, marshaller);
        }

        void RegisterRuntimePtrMarshaller(TypeInfo type, string marshallerTypeName, TypeInfo? marshallableType = null)
        {
            // If a marshallable type was not provided, assume it's the same as the type we're
            // registering the marshaller for.
            marshallableType ??= type;

            var unmanagedType = _typeDB.GetUnmanagedType(marshallableType);

            var marshallerType = new TypeInfo(marshallerTypeName, "Godot.NativeInterop.Marshallers")
            {
                TypeAttributes = TypeAttributes.ReferenceType,
                GenericTypeArgumentCount = type.GenericTypeArgumentCount,
            };

            var marshaller = new RuntimePtrMarshallerWriter(marshallerType, marshallableType, unmanagedType.MakePointerType());
            RegisterPtrMarshaller(type, marshaller);
        }

        void RegisterVariantMarshaller(TypeInfo type, VariantMarshallerWriter marshaller)
        {
            _typeDB.RegisterVariantMarshaller(type, marshaller);
        }

        void RegisterRuntimeVariantMarshaller(TypeInfo type, string marshallerTypeName, TypeInfo? marshallableType = null)
        {
            // If a marshallable type was not provided, assume it's the same as the type we're
            // registering the marshaller for.
            marshallableType ??= type;

            var marshallerType = new TypeInfo(marshallerTypeName, "Godot.NativeInterop.Marshallers")
            {
                TypeAttributes = TypeAttributes.ReferenceType,
                GenericTypeArgumentCount = type.GenericTypeArgumentCount,
            };

            var marshaller = new RuntimeVariantMarshallerWriter(marshallerType, marshallableType);
            _typeDB.RegisterVariantMarshaller(type, marshaller);
        }

        void RegisterDefaultValueParser(TypeInfo type, DefaultValueParser defaultValueParser)
        {
            _typeDB.RegisterDefaultValueParser(type, defaultValueParser);
        }

        void RegisterMemberMapping(TypeInfo type, string engineMemberName, string memberName)
        {
            _typeDB.RegisterMemberMapping(type, engineMemberName, $"{type.FullNameWithGlobal}.{memberName}");
        }

        void RegisterGlobalMemberMapping(TypeInfo type, string fullyQualifiedEngineMemberName, string memberName)
        {
            _typeDB.RegisterGlobalMemberMapping(fullyQualifiedEngineMemberName, $"{type.FullNameWithGlobal}.{memberName}");
        }

        void RegisterGlobalMemberMappingNoType(string fullyQualifiedEngineMemberName, string fullyQualifiedMemberName)
        {
            _typeDB.RegisterGlobalMemberMapping(fullyQualifiedEngineMemberName, fullyQualifiedMemberName);
        }
    }
}

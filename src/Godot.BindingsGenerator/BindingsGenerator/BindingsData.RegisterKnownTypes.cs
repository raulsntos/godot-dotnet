using System;
using Godot.BindingsGenerator.Marshallers;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

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
        RegisterRuntimePtrMarshaller(KnownTypes.GodotNodePath, "NodePathMarshaller");
        RegisterPtrMarshaller(KnownTypes.GodotPlane, new BlittablePtrMarshallerWriter(KnownTypes.GodotPlane));
        RegisterPtrMarshaller(KnownTypes.GodotProjection, new BlittablePtrMarshallerWriter(KnownTypes.GodotProjection));
        RegisterPtrMarshaller(KnownTypes.GodotQuaternion, new BlittablePtrMarshallerWriter(KnownTypes.GodotQuaternion));
        RegisterPtrMarshaller(KnownTypes.GodotRect2, new BlittablePtrMarshallerWriter(KnownTypes.GodotRect2));
        RegisterPtrMarshaller(KnownTypes.GodotRect2I, new BlittablePtrMarshallerWriter(KnownTypes.GodotRect2I));
        RegisterPtrMarshaller(KnownTypes.GodotRid, new BlittablePtrMarshallerWriter(KnownTypes.GodotRid));
        RegisterPtrMarshaller(KnownTypes.GodotSignal, new InteropStructPtrMarshallerWriter(KnownTypes.GodotSignal, KnownTypes.NativeGodotSignal.MakePointerType()));
        RegisterRuntimePtrMarshaller(KnownTypes.SystemString, "StringMarshaller");
        RegisterRuntimePtrMarshaller(KnownTypes.GodotStringName, "StringNameMarshaller");
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
        RegisterRuntimeVariantMarshaller(KnownTypes.GodotNodePath, "NodePathMarshaller");
        RegisterVariantMarshaller(KnownTypes.GodotPlane, new InteropStructVariantMarshallerWriter(KnownTypes.GodotPlane, "Plane"));
        RegisterVariantMarshaller(KnownTypes.GodotProjection, new InteropStructVariantMarshallerWriter(KnownTypes.GodotProjection, "Projection"));
        RegisterVariantMarshaller(KnownTypes.GodotQuaternion, new InteropStructVariantMarshallerWriter(KnownTypes.GodotQuaternion, "Quaternion"));
        RegisterVariantMarshaller(KnownTypes.GodotRect2, new InteropStructVariantMarshallerWriter(KnownTypes.GodotRect2, "Rect2"));
        RegisterVariantMarshaller(KnownTypes.GodotRect2I, new InteropStructVariantMarshallerWriter(KnownTypes.GodotRect2I, "Rect2I"));
        RegisterVariantMarshaller(KnownTypes.GodotRid, new InteropStructVariantMarshallerWriter(KnownTypes.GodotRid, "Rid"));
        RegisterVariantMarshaller(KnownTypes.GodotSignal, new InteropStructVariantMarshallerWriter(KnownTypes.GodotSignal, "Signal", KnownTypes.NativeGodotSignal, createMethodSuffix: "TakingOwnership"));
        RegisterRuntimeVariantMarshaller(KnownTypes.SystemString, "StringMarshaller");
        RegisterRuntimeVariantMarshaller(KnownTypes.GodotStringName, "StringNameMarshaller");
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
        RegisterPackedArray("PackedByteArray", KnownTypes.SystemByte, KnownTypes.GodotPackedByteArray, KnownTypes.NativeGodotPackedByteArray);
        RegisterPackedArray("PackedInt32Array", KnownTypes.SystemInt32, KnownTypes.GodotPackedInt32Array, KnownTypes.NativeGodotPackedInt32Array);
        RegisterPackedArray("PackedInt64Array", KnownTypes.SystemInt64, KnownTypes.GodotPackedInt64Array, KnownTypes.NativeGodotPackedInt64Array);
        RegisterPackedArray("PackedFloat32Array", KnownTypes.SystemSingle, KnownTypes.GodotPackedFloat32Array, KnownTypes.NativeGodotPackedFloat32Array);
        RegisterPackedArray("PackedFloat64Array", KnownTypes.SystemDouble, KnownTypes.GodotPackedFloat64Array, KnownTypes.NativeGodotPackedFloat64Array);
        RegisterPackedArray("PackedStringArray", KnownTypes.SystemString, KnownTypes.GodotPackedStringArray, KnownTypes.NativeGodotPackedStringArray);
        RegisterPackedArray("PackedVector2Array", KnownTypes.GodotVector2, KnownTypes.GodotPackedVector2Array, KnownTypes.NativeGodotPackedVector2Array);
        RegisterPackedArray("PackedVector3Array", KnownTypes.GodotVector3, KnownTypes.GodotPackedVector3Array, KnownTypes.NativeGodotPackedVector3Array);
        RegisterPackedArray("PackedColorArray", KnownTypes.GodotColor, KnownTypes.GodotPackedColorArray, KnownTypes.NativeGodotPackedColorArray);
        RegisterPackedArray("PackedVector4Array", KnownTypes.GodotVector3, KnownTypes.GodotPackedVector4Array, KnownTypes.NativeGodotPackedVector4Array);
        void RegisterPackedArray(string engineTypeName, TypeInfo itemType, TypeInfo type, TypeInfo unmanagedType)
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

        RegisterRuntimePtrMarshaller(KnownTypes.GodotArray, "GodotArrayMarshaller");
        RegisterRuntimePtrMarshaller(KnownTypes.GodotArrayGeneric, "GodotArrayMarshaller");
        RegisterPtrMarshaller(KnownTypes.NativeGodotArray, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotArray));

        RegisterRuntimePtrMarshaller(KnownTypes.GodotDictionary, "GodotDictionaryMarshaller");
        RegisterRuntimePtrMarshaller(KnownTypes.GodotDictionaryGeneric, "GodotDictionaryMarshaller");
        RegisterPtrMarshaller(KnownTypes.NativeGodotDictionary, new InteropStructPtrMarshallerWriter(KnownTypes.NativeGodotDictionary));

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
    }
}

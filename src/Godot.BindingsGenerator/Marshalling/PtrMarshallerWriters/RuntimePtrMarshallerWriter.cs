using System;
using System.CodeDom.Compiler;
using System.IO;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class RuntimePtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The runtime type that implements the marshalling between
    /// <see cref="_marshallableType"/> and <see cref="_unmanagedPointerType"/>.
    /// </summary>
    internal TypeInfo MarshallerType { get; }

    /// <summary>
    /// The type that can be marshalled by the marshaller.
    /// Other types may also be supported if they can be implicitly converted
    /// but the unmarshalling method will return this type so it will need to
    /// be casted.
    /// </summary>
    private readonly TypeInfo _marshallableType;

    /// <summary>
    /// The unmanaged pointer type that the marshaller returns when
    /// marshalling <see cref="_marshallableType"/>.
    /// </summary>
    private readonly TypeInfo _unmanagedPointerType;

    public RuntimePtrMarshallerWriter(TypeInfo marshallerType, TypeInfo marshallableType, TypeInfo unmanagedPointerType)
    {
        if (marshallerType.Namespace != "Godot.NativeInterop.Marshallers")
        {
            throw new ArgumentException($"Invalid marshaller type: '{marshallerType.FullName}'. Not in the expected namespace.", nameof(marshallerType));
        }
        if (!unmanagedPointerType.IsPointerType)
        {
            throw new ArgumentException("Unmanaged pointer type must be a pointer.", nameof(unmanagedPointerType));
        }

        MarshallerType = marshallerType;
        _marshallableType = marshallableType;
        _unmanagedPointerType = unmanagedPointerType;
    }

    protected override TypeInfo GetMarshallableType()
    {
        return _marshallableType;
    }

    protected override TypeInfo GetUnmanagedPointerTypeCore()
    {
        return _unmanagedPointerType;
    }

    private void WriteMarshallerType(TextWriter writer, TypeInfo type)
    {
        var marshallerType = MarshallerType;

        if (marshallerType.IsGenericTypeDefinition)
        {
            // If the registered marshaller type is a generic type definition,
            // the type to marshal must be a constructed generic with the
            // same type argument count.

            if (!type.IsGenericType)
            {
                throw new ArgumentException($"Marshaller '{marshallerType.FullName}' can't marshal the type '{type.FullName}' because it's not generic.", nameof(type));
            }

            if (marshallerType.GenericTypeArgumentCount != type.GenericTypeArgumentCount)
            {
                throw new ArgumentException($"Marshaller '{marshallerType.FullName}' can't marshal type '{type.FullName}' because the type argument count does not match.", nameof(type));
            }

            marshallerType = marshallerType.MakeGenericType(type.GenericTypeArguments);
        }

        writer.Write(marshallerType.FullNameWithGlobal);
    }

    protected override void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        writer.Write($"{destination} = ");
        WriteMarshallerType(writer, type);
        writer.Write(".ConvertToUnmanaged(");
        if (type.IsEnum)
        {
            // Godot encodes all enums as a 64-bit integer.
            writer.Write("(long)");
        }
        writer.Write(source);
        if (type.IsGenericType && type.GenericTypeDefinition == KnownTypes.Nullable)
        {
            // Get the Nullable<T>.Value or the type's default value if it's null.
            writer.Write($".GetValueOrDefault()");
        }
        writer.WriteLine(");");
    }

    protected override void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        writer.Write($"{destination} = ");
        // Check if the returned type needs to be downcasted.
        if (type != _marshallableType)
        {
            writer.Write($"({type.FullNameWithGlobal})");
        }
        WriteMarshallerType(writer, type);
        writer.WriteLine($".ConvertFromUnmanaged({source});");
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        WriteMarshallerType(writer, type);
        writer.Write(".Free(");
        if (type.IsEnum)
        {
            // Godot encodes all enums as a 64-bit integer.
            writer.Write("(long*)");
        }
        writer.Write(source);
        writer.WriteLine(");");
    }
}

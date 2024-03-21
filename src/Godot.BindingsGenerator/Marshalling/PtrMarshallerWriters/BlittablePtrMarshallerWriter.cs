using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class BlittablePtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The type that will be marshalled.
    /// </summary>
    private readonly TypeInfo _marshallableType;

    /// <summary>
    /// The type of the fixed pointer to the marshalled value.
    /// </summary>
    private readonly TypeInfo _unmanagedPointerType;

    public override bool NeedsCleanup => false;

    public BlittablePtrMarshallerWriter(TypeInfo marshallableType)
    {
        if (!marshallableType.IsValueType)
        {
            throw new ArgumentException("Marshallable type must be a value type.", nameof(marshallableType));
        }

        _marshallableType = marshallableType;
        _unmanagedPointerType = marshallableType.MakePointerType();
    }

    protected override TypeInfo GetMarshallableType()
    {
        return _marshallableType;
    }

    protected override TypeInfo GetUnmanagedPointerTypeCore()
    {
        return _unmanagedPointerType;
    }

    protected override void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.WriteLine($"{destination} = &{source};");
    }

    protected override void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.WriteLine($"{destination} = *{source};");
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        // Nothing to free.
    }
}

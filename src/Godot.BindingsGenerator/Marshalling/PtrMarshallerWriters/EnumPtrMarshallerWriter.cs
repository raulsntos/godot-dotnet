using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class EnumPtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The enum type that will be marshalled.
    /// </summary>
    private readonly EnumInfo _marshallableType;

    /// <summary>
    /// The underlying type of the enum that will be marshalled.
    /// </summary>
    private readonly TypeInfo _underlyingMarshallableType;

    /// <summary>
    /// The underlying <see cref="PtrMarshallerWriter"/> that will be used
    /// to marshall the enum's underlying type.
    /// </summary>
    private readonly PtrMarshallerWriter _underlyingMarshaller;

    public override bool NeedsCleanup => _underlyingMarshaller.NeedsCleanup;

    public EnumPtrMarshallerWriter(EnumInfo marshallableType)
    {
        _marshallableType = marshallableType;
        _underlyingMarshallableType = marshallableType.UnderlyingType ?? KnownTypes.SystemInt32;
        _underlyingMarshaller = IntegerPtrMarshallerWriter.Instance;
    }

    protected override TypeInfo GetMarshallableType()
    {
        return _marshallableType;
    }

    protected override TypeInfo GetUnmanagedPointerTypeCore()
    {
        return _underlyingMarshaller.UnmanagedPointerType;
    }

    protected override bool WriteSetupToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        return _underlyingMarshaller.WriteSetupToUnmanaged(writer, _underlyingMarshallableType, $"(long){source}", destination);
    }

    protected override bool WriteSetupToUnmanagedUninitializedCore(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        return _underlyingMarshaller.WriteSetupToUnmanagedUninitialized(writer, _underlyingMarshallableType, destination);
    }

    protected override void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.WriteLine($"{destination} = ({_underlyingMarshaller.UnmanagedPointerType.FullNameWithGlobal})(&{source});");
    }

    protected override void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.WriteLine($"{destination} = ({type.FullNameWithGlobal})(*{source});");
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        _underlyingMarshaller.WriteFree(writer, type, source);
    }
}

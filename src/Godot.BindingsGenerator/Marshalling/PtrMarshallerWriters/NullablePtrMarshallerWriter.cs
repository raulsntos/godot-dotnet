using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class NullablePtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The Nullable<T> type that will be marshalled.
    /// </summary>
    private readonly TypeInfo _marshallableType;

    /// <summary>
    /// The underlying <see cref="PtrMarshallerWriter"/> that will be used
    /// to marshall the T from the Nullable<T>.
    /// </summary>
    private readonly PtrMarshallerWriter _underlyingMarshaller;

    public override bool NeedsCleanup => _underlyingMarshaller.NeedsCleanup;

    public NullablePtrMarshallerWriter(TypeInfo marshallableType, PtrMarshallerWriter underlyingMarshaller)
    {
        if (!marshallableType.IsGenericType || marshallableType.GenericTypeDefinition != KnownTypes.Nullable)
        {
            // Only Nullable<T> is supported.
            throw new ArgumentException($"Marshallable type must be a constructed Nullable<T> type.", nameof(marshallableType));
        }
        if (underlyingMarshaller.MarshallableType != marshallableType.GenericTypeArguments[0])
        {
            // Only T from the Nullable<T> is supported.
            throw new ArgumentException("Underlying marshaller's marshallable type must match the T in the Nullable<T> type.", nameof(underlyingMarshaller));
        }

        _marshallableType = marshallableType;
        _underlyingMarshaller = underlyingMarshaller;
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

        var marshallableType = type.GenericTypeArguments[0];
        if (!_underlyingMarshaller.WriteSetupToUnmanaged(writer, marshallableType, $"{source}.Value", destination))
        {
            writer.WriteLine($"{marshallableType.FullNameWithGlobal} {destination} = {source}.Value;");
        }
        return true;
    }

    protected override bool WriteSetupToUnmanagedUninitializedCore(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        var marshallableType = type.GenericTypeArguments[0];
        return _underlyingMarshaller.WriteSetupToUnmanagedUninitialized(writer, marshallableType, destination);
    }

    protected override void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        var marshallableType = type.GenericTypeArguments[0];
        _underlyingMarshaller.WriteConvertToUnmanaged(writer, marshallableType, source, destination);
    }

    protected override void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        var marshallableType = type.GenericTypeArguments[0];
        _underlyingMarshaller.WriteConvertFromUnmanaged(writer, marshallableType, source, destination);
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be freed by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        var marshallableType = type.GenericTypeArguments[0];
        _underlyingMarshaller.WriteFree(writer, marshallableType, source);
    }
}

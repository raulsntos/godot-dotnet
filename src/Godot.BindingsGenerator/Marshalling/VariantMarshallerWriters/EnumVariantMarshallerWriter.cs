using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class EnumVariantMarshallerWriter : VariantMarshallerWriter
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
    /// The underlying <see cref="VariantMarshallerWriter"/> that will be used
    /// to marshall the enum's underlying type.
    /// </summary>
    private readonly VariantMarshallerWriter _underlyingMarshaller;

    public override bool NeedsCleanup => _underlyingMarshaller.NeedsCleanup;

    public EnumVariantMarshallerWriter(EnumInfo marshallableType)
    {
        _marshallableType = marshallableType;
        _underlyingMarshallableType = marshallableType.UnderlyingType ?? KnownTypes.SystemInt32;
        _underlyingMarshaller = IntegerVariantMarshallerWriter.Instance;
    }

    protected override TypeInfo GetMarshallableType()
    {
        return _marshallableType;
    }

    protected override bool WriteSetupToVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        return _underlyingMarshaller.WriteSetupToVariant(writer, _underlyingMarshallableType, $"{source}.Value", destination);
    }

    protected override bool WriteSetupToVariantUninitializedCore(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        return _underlyingMarshaller.WriteSetupToVariantUninitialized(writer, _underlyingMarshallableType, destination);
    }

    protected override void WriteConvertToVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        _underlyingMarshaller.WriteConvertToVariant(writer, _underlyingMarshallableType, $"(long)({source})", destination);
    }

    protected override void WriteConvertFromVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.WriteLine($"{destination} = ({type.FullNameWithGlobal})global::Godot.NativeInterop.NativeGodotVariant.ConvertToInt(*{source});");
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        _underlyingMarshaller.WriteFree(writer, type, source);
    }
}

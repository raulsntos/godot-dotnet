using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class VariantVariantMarshallerWriter : VariantMarshallerWriter
{
    /// <summary>
    /// The type that will be marshalled.
    /// </summary>
    private readonly TypeInfo _marshallableType;

    public override bool NeedsCleanup => false;

    public VariantVariantMarshallerWriter(TypeInfo marshallableType)
    {
        _marshallableType = marshallableType;
    }

    protected override TypeInfo GetMarshallableType()
    {
        return _marshallableType;
    }

    protected override void WriteConvertToVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        if (type == KnownTypes.NativeGodotVariant)
        {
            writer.WriteLine($"{destination} = &{source};");
            return;
        }

        writer.WriteLine($"{destination} = {source}.NativeValue.DangerousSelfRef.GetUnsafeAddress();");
    }

    protected override void WriteConvertFromVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        if (type == KnownTypes.NativeGodotVariant)
        {
            writer.WriteLine($"{destination} = *{source};");
            return;
        }

        writer.WriteLine($"{destination} = {KnownTypes.GodotVariant.FullName}.CreateTakingOwnership(*{source});");
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        // Nothing to free.
    }
}

using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class InteropStructVariantMarshallerWriter : VariantMarshallerWriter
{
    /// <summary>
    /// The interop struct type that will be marshalled.
    /// </summary>
    private readonly TypeInfo _marshallableType;

    /// <summary>
    /// The type of the native interop struct if it's different from
    /// <see cref="_marshallableType"/>.
    /// </summary>
    private readonly TypeInfo _unmanagedType;

    private readonly string _engineTypeName;

    private readonly string? _createMethodSuffix;

    public override bool NeedsCleanup => false;

    public InteropStructVariantMarshallerWriter(TypeInfo marshallableType, string engineTypeName, TypeInfo? unmanagedType = null, string? createMethodSuffix = null)
    {
        _marshallableType = marshallableType;
        _unmanagedType = unmanagedType ?? marshallableType;

        _engineTypeName = engineTypeName;
        _createMethodSuffix = createMethodSuffix;
    }

    protected override TypeInfo GetMarshallableType()
    {
        return _marshallableType;
    }

    protected override void WriteConvertToVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (!IsTypeCompatible(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.Write($"{destination} = ");
        if (type.IsReferenceType)
        {
            writer.Write($"{source} is not null ? ");
        }
        writer.Write("global::Godot.NativeInterop.NativeGodotVariant");
        writer.Write($".CreateFrom{_engineTypeName}{_createMethodSuffix}(");
        writer.Write(source);
        if (type != _unmanagedType)
        {
            writer.Write($".NativeValue.DangerousSelfRef");
        }
        writer.Write(").GetUnsafeAddress()");
        if (type.IsReferenceType)
        {
            writer.Write(" : default");
        }
        writer.WriteLine(';');
    }

    protected override void WriteConvertFromVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (!IsTypeCompatible(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.Write($"{destination} = ");
        if (type != _unmanagedType)
        {
            writer.Write($"{type.FullNameWithGlobal}.CreateTakingOwnership(");
        }
        writer.Write($"global::Godot.NativeInterop.NativeGodotVariant.ConvertTo{_engineTypeName}(*{source})");
        if (type != _unmanagedType)
        {
            writer.Write(')');
        }
        writer.WriteLine(';');
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        // Nothing to free.
    }

    private bool IsTypeCompatible(TypeInfo type)
    {
        if (type == _marshallableType)
        {
            return true;
        }

        // Allow using the marshaller registered for the generic type definition with constructed types.
        if (type.IsGenericType && type.GenericTypeDefinition == _marshallableType)
        {
            return true;
        }

        return false;
    }
}

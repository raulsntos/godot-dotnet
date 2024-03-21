using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class InteropStructPtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The interop struct type that will be marshalled.
    /// </summary>
    private readonly TypeInfo _marshallableType;

    /// <summary>
    /// The type of the fixed pointer to the marshalled value.
    /// </summary>
    private readonly TypeInfo _unmanagedPointerType;

    public override bool NeedsCleanup => false;

    public InteropStructPtrMarshallerWriter(TypeInfo marshallableType, TypeInfo? unmanagedPointerType = null)
    {
        if (unmanagedPointerType is null && !marshallableType.IsByRefLike)
        {
            throw new ArgumentException($"Type '{marshallableType.FullName}' can't be marshalled by this marshaller. When {nameof(unmanagedPointerType)} is not provided only ref structs are supported.", nameof(marshallableType));
        }
        if (unmanagedPointerType is not null && !unmanagedPointerType.IsPointerType)
        {
            throw new ArgumentException("Unmanaged pointer type must be a pointer.", nameof(unmanagedPointerType));
        }

        _marshallableType = marshallableType;
        _unmanagedPointerType = unmanagedPointerType ?? marshallableType.MakePointerType();
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

        writer.Write($"{destination} = ");
        if (type.IsReferenceType)
        {
            writer.Write($"{source} is not null ? ");
        }
        writer.Write($"{source}");
        if (!type.IsByRefLike)
        {
            writer.Write(".NativeValue.DangerousSelfRef");
        }
        writer.Write(".GetUnsafeAddress()");
        if (type.IsReferenceType)
        {
            writer.Write(" : default");
        }
        writer.WriteLine(';');
    }

    protected override void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != _marshallableType)
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.Write($"{destination} = ");
        if (!type.IsByRefLike)
        {
            writer.WriteLine($"{_marshallableType.FullNameWithGlobal}.CreateTakingOwnership(*{source});");
        }
        else
        {
            writer.WriteLine($"*{source};");
        }
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        // Nothing to free.
    }
}

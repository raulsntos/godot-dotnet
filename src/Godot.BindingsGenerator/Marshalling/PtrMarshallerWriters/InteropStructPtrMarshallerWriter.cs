using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
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

    protected override bool WriteSetupToUnmanagedUninitializedCore(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        if (type.IsReferenceType)
        {
            // For reference types we want to declare a variable of the unmanaged type; otherwise,
            // since uninitialized reference types are null, we'd always get null pointers.
            TypeInfo? unmanagedType = _unmanagedPointerType.PointedAtType;
            Debug.Assert(unmanagedType is not null);
            writer.WriteLine($"{unmanagedType.FullNameWithGlobal} {destination};");
            writer.WriteLine($"global::System.Runtime.CompilerServices.Unsafe.SkipInit(out {destination});");

            // We assume the destination name in 'WriteConvertToUnmanaged' so let's check just to make sure.
            // There are other ways in which this could fail, but it's probably good enough.
            Debug.Assert(destination.EndsWith("Native", StringComparison.Ordinal));

            return true;
        }

        return false;
    }

    protected override void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (!IsTypeCompatible(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be marshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        if (type.IsReferenceType && source.EndsWith("Native", StringComparison.Ordinal))
        {
            // For reference types we may be using the unmanaged type instead
            // if the 'WriteSetupToUnmanagedUninitialized' method was called
            // (i.e.: for return parameters)
            TypeInfo? unmanagedType = _unmanagedPointerType.PointedAtType;
            Debug.Assert(unmanagedType is not null);
            type = unmanagedType;
        }

        writer.Write($"{destination} = ");
        if (type.IsReferenceType)
        {
            writer.Write($"{source} is not null ? ");
        }
        writer.Write(source);
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
        if (!IsTypeCompatible(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' can't be unmarshalled by this marshaller. Only '{_marshallableType.FullName}' is supported.", nameof(type));
        }

        writer.Write($"{destination} = ");
        if (!type.IsByRefLike)
        {
            writer.WriteLine($"{type.FullNameWithGlobal}.CreateTakingOwnership(*{source});");
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

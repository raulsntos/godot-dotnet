using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class PtrPtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The pointer type that can be marshalled by the marshaller.
    /// </summary>
    private readonly TypeInfo _pointerType;

    public PtrPtrMarshallerWriter(TypeInfo pointerType)
    {
        if (!pointerType.IsPointerType)
        {
            throw new ArgumentException($"Invalid type: '{pointerType.FullName}'. PtrPtrMarshallerWriter can only marshal pointer types.", nameof(pointerType));
        }

        _pointerType = pointerType;
    }

    protected override TypeInfo GetMarshallableType()
    {
        return _pointerType;
    }

    protected override TypeInfo GetUnmanagedPointerTypeCore()
    {
        return KnownTypes.SystemVoidPtr;
    }

    protected override void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        writer.WriteLine($"{destination} = &{source};");
    }

    protected override void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        throw new NotSupportedException("Can't unmarshal pointer types.");
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        // Nothing to free.
    }
}

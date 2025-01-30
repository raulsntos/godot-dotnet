using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class FloatingPointPtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The type of the fixed pointer to the marshalled value.
    /// </summary>
    private static readonly TypeInfo _unmanagedPointerType = KnownTypes.SystemDouble.MakePointerType();

    public static FloatingPointPtrMarshallerWriter Instance { get; } = new();

    public override bool NeedsCleanup => false;

    private FloatingPointPtrMarshallerWriter() { }

    protected override TypeInfo GetMarshallableType()
    {
        return KnownTypes.SystemDouble;
    }

    protected override TypeInfo GetUnmanagedPointerTypeCore()
    {
        return _unmanagedPointerType;
    }

    protected override bool WriteSetupToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != KnownTypes.SystemDouble)
        {
            writer.Write($"double {destination} = ");
            if (type == KnownTypes.SystemHalf)
            {
                // Half is the only type that needs to be converted explicitly.
                writer.Write("(double)");
            }
            writer.WriteLine($"{source};");
            return true;
        }

        return false;
    }

    protected override bool WriteSetupToUnmanagedUninitializedCore(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        if (type != KnownTypes.SystemDouble)
        {
            writer.WriteLine($"double {destination};");
            writer.WriteLine($"global::System.Runtime.CompilerServices.Unsafe.SkipInit(out {destination});");
            return true;
        }

        return false;
    }

    protected override void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        writer.WriteLine($"{destination} = &{source};");
    }

    protected override void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != KnownTypes.SystemDouble)
        {
            writer.WriteLine($"{destination} = ({type.FullNameWithGlobal})(*{source});");
        }
        else
        {
            writer.WriteLine($"{destination} = *{source};");
        }
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        // Nothing to free.
    }
}

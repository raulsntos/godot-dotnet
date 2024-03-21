using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class IntegerPtrMarshallerWriter : PtrMarshallerWriter
{
    /// <summary>
    /// The type of the fixed pointer to the marshalled value.
    /// </summary>
    private readonly static TypeInfo _unmanagedPointerType = KnownTypes.SystemInt64.MakePointerType();

    private readonly static TypeInfo[] _compatibleTypes =
    [
        KnownTypes.SystemSByte, KnownTypes.SystemByte,
        KnownTypes.SystemInt16, KnownTypes.SystemUInt16,
        KnownTypes.SystemInt32, KnownTypes.SystemUInt32,
        KnownTypes.SystemInt64, KnownTypes.SystemUInt64,
    ];

    public static IntegerPtrMarshallerWriter Instance { get; } = new();

    public override bool NeedsCleanup => false;

    private IntegerPtrMarshallerWriter() { }

    protected override TypeInfo GetMarshallableType()
    {
        return KnownTypes.SystemInt64;
    }

    protected override TypeInfo GetUnmanagedPointerTypeCore()
    {
        return _unmanagedPointerType;
    }

    protected override bool WriteSetupToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        if (type != KnownTypes.SystemInt64)
        {
            writer.Write($"long {destination} = ");
            if (type == KnownTypes.SystemUInt64)
            {
                // UInt64 is the only type that needs to be converted explicitly.
                writer.Write("(long)");
            }
            writer.WriteLine($"{source};");
            return true;
        }

        return false;
    }

    protected override bool WriteSetupToUnmanagedUninitializedCore(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        if (type != KnownTypes.SystemInt64)
        {
            writer.WriteLine($"long {destination};");
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
        if (type != KnownTypes.SystemInt64)
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

using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator.Marshallers;

internal sealed class FloatingPointVariantMarshallerWriter : VariantMarshallerWriter
{
    private readonly static TypeInfo[] _compatibleTypes =
    [
        KnownTypes.SystemHalf,
        KnownTypes.SystemSingle,
        KnownTypes.SystemDouble,
    ];

    public static FloatingPointVariantMarshallerWriter Instance { get; } = new();

    public override bool NeedsCleanup => false;

    private FloatingPointVariantMarshallerWriter() { }

    protected override TypeInfo GetMarshallableType()
    {
        return KnownTypes.SystemDouble;
    }

    protected override void WriteConvertToVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        writer.Write($"{destination} = global::Godot.NativeInterop.NativeGodotVariant.CreateFromFloat(");
        if (type != KnownTypes.SystemDouble)
        {
            writer.Write($"(double)({source})");
        }
        else
        {
            writer.Write(source);
        }
        writer.WriteLine(").GetUnsafeAddress();");
    }

    protected override void WriteConvertFromVariantCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        writer.Write($"{destination} = global::Godot.NativeInterop.NativeGodotVariant.ConvertToFloat(");
        if (type != KnownTypes.SystemDouble)
        {
            writer.Write($"({type.FullNameWithGlobal})(*{source})");
        }
        else
        {
            writer.Write($"*{source}");
        }
        writer.WriteLine(");");
    }

    protected override void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source)
    {
        // Nothing to free.
    }
}

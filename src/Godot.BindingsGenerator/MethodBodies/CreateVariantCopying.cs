using System.CodeDom.Compiler;
using System.Diagnostics;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CreateVariantCopying : MethodBody
{
    private readonly string _targetTypeName;

    private readonly TypeDB _typeDB;

    public override bool RequiresUnsafeCode => true;

    public CreateVariantCopying(string targetTypeName, TypeDB typeDB)
    {
        _targetTypeName = targetTypeName;
        _typeDB = typeDB;
    }

    public override void Write(MethodBase owner, IndentedTextWriter writer)
    {
        Debug.Assert(owner.Parameters.Count == 1, "Variant's CreateFrom methods must take exactly one parameter.");

        TypeInfo? targetTypeUnmanaged = owner.Parameters[0].Type;

        Debug.Assert(targetTypeUnmanaged.IsValueType, message: "Variant's CreateFrom methods must take a struct type parameter.");

        var marshaller = _typeDB.GetPtrMarshaller(targetTypeUnmanaged);

        if (targetTypeUnmanaged == KnownTypes.SystemIntPtr)
        {
            // If the pointer is null, we can avoid the interop call
            // and just return a default Variant.
            writer.WriteLine("// Avoid interop call for null pointers.");
            writer.WriteLine("if (value == 0)");
            writer.OpenBlock();
            writer.WriteLine("return default;");
            writer.CloseBlock();
        }

        writer.WriteLine($"{marshaller.UnmanagedPointerType.FullNameWithGlobal} __valueNative = null;");

        writer.WriteLine("try");
        writer.OpenBlock();

        marshaller.WriteConvertToUnmanaged(writer, targetTypeUnmanaged, "value", "__valueNative");

        writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant destination = default;");
        writer.WriteLine($"_variantFrom{_targetTypeName}Constructor(destination.GetUnsafeAddress(), __valueNative);");
        writer.WriteLine("return destination;");

        writer.CloseBlock();
        writer.WriteLine("finally");
        writer.OpenBlock();

        marshaller.WriteFree(writer, targetTypeUnmanaged, "__valueNative");

        writer.CloseBlock();
    }
}

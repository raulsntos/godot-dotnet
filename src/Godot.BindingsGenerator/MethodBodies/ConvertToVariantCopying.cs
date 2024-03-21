using System.CodeDom.Compiler;
using System.Diagnostics;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class ConvertToVariantCopying : MethodBody
{
    private readonly string _targetTypeName;

    public override bool RequiresUnsafeCode => true;

    public ConvertToVariantCopying(string targetTypeName)
    {
        _targetTypeName = targetTypeName;
    }

    public override void Write(MethodBase owner, IndentedTextWriter writer)
    {
        TypeInfo? returnType = owner is MethodInfo method ? method.ReturnType : null;

        Debug.Assert(returnType is not null && returnType.IsValueType, "Variant's ConvertTo methods must return a struct type.");

        if (returnType == KnownTypes.NativeGodotString)
        {
            // When converting Variant to String, we need to add a special case
            // if the Variant is null to return an empty string; otherwise, the
            // interop call will convert it to the string "Null".
            writer.WriteLine("""// Avoid converting null Variants to the string "Null".""");
            writer.WriteLine("if (value.Type == global::Godot.VariantType.Nil)");
            writer.OpenBlock();
            writer.WriteLine("return default;");
            writer.CloseBlock();
        }

        writer.WriteLine($"{returnType.FullNameWithGlobal} dest = default;");
        writer.WriteLine($"_variantTo{_targetTypeName}Constructor(&dest, value.GetUnsafeAddress());");
        writer.WriteLine("return dest;");
    }
}

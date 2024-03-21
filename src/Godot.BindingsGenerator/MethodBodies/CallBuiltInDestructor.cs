using System.CodeDom.Compiler;
using System.Diagnostics;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CallBuiltInDestructor : MethodBody
{
    private readonly string _variantType;

    public override bool RequiresUnsafeCode => true;

    public CallBuiltInDestructor(string variantType)
    {
        _variantType = variantType;
    }

    public override void Write(MethodBase owner, IndentedTextWriter writer)
    {
        // Retrieve and cache the destructor.
        writer.WriteLine($"global::Godot.NativeInterop.MethodBind.GetAndCacheBuiltInDestructor(ref _destructor, global::Godot.NativeInterop.GDExtensionVariantType.{_variantType});");

        if (owner is MethodInfo method)
        {
            Debug.Assert(method.ReturnType is null);
        }

        Debug.Assert(owner.Parameters.Count == 1);
        var parameter = owner.Parameters[0];
        string escapedParameterName = SourceCodeWriter.EscapeIdentifier(parameter.Name);

        writer.WriteLine($"_destructor({escapedParameterName}.GetUnsafeAddress());");
    }
}

using System.CodeDom.Compiler;
using System.Diagnostics;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CallBuiltInConstructor : PtrCallMethodBody<PtrCallMethodBodyContext>
{
    private readonly string _variantType;
    private readonly int _constructorIndex;

    public CallBuiltInConstructor(string variantType, int constructorIndex, TypeDB typeDB) : base(typeDB)
    {
        _variantType = variantType;
        _constructorIndex = constructorIndex;
    }

    protected override PtrCallMethodBodyContext CreatePtrCallContext(MethodBase owner)
    {
        var method = (MethodInfo)owner;
        Debug.Assert(method is not null && method.ReturnType is not null);

        return new PtrCallMethodBodyContext()
        {
            IsStatic = true,
            Parameters = method.Parameters,
            ReturnType = method.ReturnType,

            ReturnVariableName = "__destination",
        };
    }

    protected override void RetrieveMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"global::Godot.NativeInterop.MethodBind.GetAndCacheBuiltInConstructor(ref _constructor{_constructorIndex}, global::Godot.NativeInterop.GDExtensionVariantType.{_variantType}, {_constructorIndex});");
    }

    protected override void InvokeMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        string argsVariable = context.Parameters.Count > 0 ? context.ArgsVariableName : "null";
        string returnVariable = $"{context.ReturnVariableName}Ptr";

        writer.WriteLine($"_constructor{_constructorIndex}({returnVariable}, {argsVariable});");
    }
}

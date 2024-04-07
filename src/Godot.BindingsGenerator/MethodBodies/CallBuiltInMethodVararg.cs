using System.CodeDom.Compiler;
using System.Linq;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CallBuiltInMethodVararg : VarargCallMethodBody<VarargCallMethodBodyContext>
{
    private readonly string _variantType;
    private readonly MethodInfo _method;
    private readonly GodotBuiltInMethodInfo _engineMethod;

    public CallBuiltInMethodVararg(string variantType, MethodInfo method, GodotBuiltInMethodInfo engineMethod, TypeDB typeDB) : base(typeDB)
    {
        _variantType = variantType;
        _method = method;
        _engineMethod = engineMethod;
    }

    protected override VarargCallMethodBodyContext CreateVarargCallContext(MethodBase owner)
    {
        TypeInfo? returnType = owner is MethodInfo method ? method.ReturnType : null;

        // Skip the `self` parameter.
        ParameterInfo[] parameters = owner.Parameters.Skip(_engineMethod.IsStatic ? 0 : 1).ToArray();

        return new VarargCallMethodBodyContext()
        {
            IsStatic = _engineMethod.IsStatic,
            Parameters = parameters,
            ReturnType = returnType,
            MarshalReturnTypeAsPtr = true,
        };
    }

    protected override void SetupInstanceParameter(VarargCallMethodBodyContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"void* {context.InstanceVariableName} = self.GetUnsafeAddress();");
    }

    protected override void RetrieveMethodBind(VarargCallMethodBodyContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"""global::Godot.NativeInterop.MethodBind.GetAndCacheBuiltInMethod(ref _{_method.Name}_MethodBind, global::Godot.NativeInterop.GDExtensionVariantType.{_variantType}, "{_engineMethod.Name}"u8, {_engineMethod.Hash}L);""");
    }

    protected override void InvokeMethodBind(VarargCallMethodBodyContext context, IndentedTextWriter writer)
    {
        string instanceVariable = !context.IsStatic ? context.InstanceVariableName : "null";
        string returnVariable = context.ReturnType is not null ? $"{context.ReturnVariableName}Ptr" : "null";

        writer.WriteLine($"_{_method.Name}_MethodBind({instanceVariable}, (void**){context.ArgsVariableName}, {returnVariable}, {context.ArgsCountVariableName});");
    }
}

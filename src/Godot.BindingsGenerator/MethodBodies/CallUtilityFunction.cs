using System.CodeDom.Compiler;
using System.Linq;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CallUtilityFunction : PtrCallMethodBody<PtrCallMethodBodyContext>
{
    private readonly MethodInfo _method;
    private readonly GodotUtilityFunctionInfo _engineMethod;

    public CallUtilityFunction(MethodInfo method, GodotUtilityFunctionInfo engineMethod, TypeDB typeDB) : base(typeDB)
    {
        _method = method;
        _engineMethod = engineMethod;
    }

    protected override PtrCallMethodBodyContext CreatePtrCallContext(MethodBase owner)
    {
        TypeInfo? returnType = owner is MethodInfo method ? method.ReturnType : null;

        int argsCount = owner.Parameters.Count;
        ParameterInfo[] parameters = owner.Parameters.TakeLast(argsCount).ToArray();

        return new PtrCallMethodBodyContext()
        {
            IsStatic = true,
            Parameters = parameters,
            ReturnType = returnType,
        };
    }

    protected override void RetrieveMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"""global::Godot.NativeInterop.MethodBind.GetAndCacheUtilityFunction(ref _{_method.Name}_MethodBind, "{_engineMethod.Name}"u8, {_engineMethod.Hash}L);""");

        writer.WriteDefaultParameterValues(context.Parameters, _engineMethod.Arguments, TypeDB);
    }

    protected override void InvokeMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        string argsVariable = context.Parameters.Count > 0 ? context.ArgsVariableName : "null";
        string returnVariable = context.ReturnType is not null ? $"{context.ReturnVariableName}Ptr" : "null";

        writer.WriteLine($"_{_method.Name}_MethodBind({returnVariable}, {argsVariable}, {context.Parameters.Count});");
    }
}

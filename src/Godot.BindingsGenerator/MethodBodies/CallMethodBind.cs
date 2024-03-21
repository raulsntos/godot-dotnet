using System.CodeDom.Compiler;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CallMethodBind : PtrCallMethodBody<PtrCallMethodBodyContext>
{
    private readonly MethodInfo _method;
    private readonly GodotMethodInfo _engineMethod;

    public CallMethodBind(MethodInfo method, GodotMethodInfo engineMethod, TypeDB typeDB) : base(typeDB)
    {
        _method = method;
        _engineMethod = engineMethod;
    }

    protected override PtrCallMethodBodyContext CreatePtrCallContext(MethodBase owner)
    {
        TypeInfo? returnType = owner is MethodInfo method ? method.ReturnType : null;

        return new PtrCallMethodBodyContext()
        {
            IsStatic = owner.IsStatic,
            Parameters = owner.Parameters,
            ReturnType = returnType,
        };
    }

    protected override void SetupInstanceParameter(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"void* {context.InstanceVariableName} = (void*)NativePtr;");
    }

    protected override void RetrieveMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"global::Godot.NativeInterop.MethodBind.GetAndCacheMethodBind(ref _{_method.Name}_MethodBind, NativeName, MethodName.{_method.Name}, {_engineMethod.Hash}L);");

        writer.WriteDefaultParameterValues(context.Parameters, _engineMethod.Arguments, TypeDB);
    }

    protected override void InvokeMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        string instanceVariable = !context.IsStatic ? context.InstanceVariableName : "null";
        string argsVariable = context.Parameters.Count > 0 ? context.ArgsVariableName : "null";
        string returnVariable = context.ReturnType is not null ? $"{context.ReturnVariableName}Ptr" : "null";

        writer.WriteLine($"global::Godot.Bridge.GodotBridge.GDExtensionInterface.object_method_bind_ptrcall(_{_method.Name}_MethodBind, {instanceVariable}, {argsVariable}, {returnVariable});");
    }
}

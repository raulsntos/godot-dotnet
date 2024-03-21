using System.CodeDom.Compiler;
using System.Linq;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CallBuiltInMethod : PtrCallMethodBody<CallBuiltInMethod.Context>
{
    private readonly string _variantType;
    private readonly MethodInfo _method;
    private readonly GodotBuiltInMethodInfo _engineMethod;

    public CallBuiltInMethod(string variantType, MethodInfo method, GodotBuiltInMethodInfo engineMethod, TypeDB typeDB) : base(typeDB)
    {
        _variantType = variantType;
        _method = method;
        _engineMethod = engineMethod;
    }

    internal class Context : PtrCallMethodBodyContext
    {
        public required TypeInfo InstanceType { get; init; }
    }

    protected override Context CreatePtrCallContext(MethodBase owner)
    {
        TypeInfo? returnType = owner is MethodInfo method ? method.ReturnType : null;

        int argsCount = owner.Parameters.Count;
        if (!_engineMethod.IsStatic)
        {
            // Remove the `self` parameter from the arg count.
            argsCount--;
        }

        ParameterInfo[] parameters = owner.Parameters.TakeLast(argsCount).ToArray();

        return new Context()
        {
            IsStatic = _engineMethod.IsStatic,
            Parameters = parameters,
            ReturnType = returnType,

            InstanceType = owner.Parameters[0].Type,
        };
    }

    protected override void SetupInstanceParameter(Context context, IndentedTextWriter writer)
    {
        var instanceType = context.InstanceType;
        if (instanceType.IsByRefLike)
        {
            writer.WriteLine($"void* {context.InstanceVariableName} = self.GetUnsafeAddress();");
        }
        else
        {
            writer.WriteLine($"void* {context.InstanceVariableName} = &self;");
        }
    }

    protected override void RetrieveMethodBind(Context context, IndentedTextWriter writer)
    {
        writer.WriteLine($"""global::Godot.NativeInterop.MethodBind.GetAndCacheBuiltInMethod(ref _{_method.Name}_MethodBind, global::Godot.NativeInterop.GDExtensionVariantType.{_variantType}, "{_engineMethod.Name}"u8, {_engineMethod.Hash}L);""");

        writer.WriteDefaultParameterValues(context.Parameters, _engineMethod.Arguments, TypeDB);
    }

    protected override void InvokeMethodBind(Context context, IndentedTextWriter writer)
    {
        string instanceVariable = !context.IsStatic ? context.InstanceVariableName : "null";
        string argsVariable = context.Parameters.Count > 0 ? context.ArgsVariableName : "null";
        string returnVariable = context.ReturnType is not null ? $"{context.ReturnVariableName}Ptr" : "null";

        writer.WriteLine($"_{_method.Name}_MethodBind({instanceVariable}, {argsVariable}, {returnVariable}, {context.Parameters.Count});");
    }
}

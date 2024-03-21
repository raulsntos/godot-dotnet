using System.CodeDom.Compiler;
using System.Diagnostics;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CallBuiltInOperator : PtrCallMethodBody<PtrCallMethodBodyContext>
{
    private readonly string _operatorKind;
    private readonly string _variantLeftType;
    private readonly string _variantRightType;
    private readonly MethodInfo _method;
    private readonly GodotOperatorInfo _engineOperator;

    public CallBuiltInOperator(string operatorKind, string variantLeftType, string variantRightType, MethodInfo method, GodotOperatorInfo engineOperator, TypeDB typeDB) : base(typeDB)
    {
        _operatorKind = operatorKind;
        _variantLeftType = variantLeftType;
        _variantRightType = variantRightType;
        _method = method;
        _engineOperator = engineOperator;
    }

    protected override PtrCallMethodBodyContext CreatePtrCallContext(MethodBase owner)
    {
        TypeInfo? returnType = owner is MethodInfo method ? method.ReturnType : null;

        Debug.Assert(owner.Parameters.Count is 1 or 2, "Operator method must have exactly 1 or 2 parameters.");

        return new PtrCallMethodBodyContext()
        {
            IsStatic = true,
            Parameters = owner.Parameters,
            ReturnType = returnType,
        };
    }

    protected override void RetrieveMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"global::Godot.NativeInterop.MethodBind.GetAndCacheBuiltInOperator(ref _{_method.Name}_{_engineOperator.RightType}_OperatorEvaluator, global::Godot.NativeInterop.GDExtensionVariantOperator.{_operatorKind}, global::Godot.NativeInterop.GDExtensionVariantType.{_variantLeftType}, global::Godot.NativeInterop.GDExtensionVariantType.{_variantRightType});");
    }

    protected override void InvokeMethodBind(PtrCallMethodBodyContext context, IndentedTextWriter writer)
    {
        string leftVariable = $"{context.ArgsVariableName}[0]";
        string rightVariable = context.Parameters.Count == 2 ? $"{context.ArgsVariableName}[1]" : "null";
        string returnVariable = context.ReturnType is not null ? $"{context.ReturnVariableName}Ptr" : "null";

        writer.WriteLine($"_{_method.Name}_{_engineOperator.RightType}_OperatorEvaluator({leftVariable}, {rightVariable}, {returnVariable});");
    }
}

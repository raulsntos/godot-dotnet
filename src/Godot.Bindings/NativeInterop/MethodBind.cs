using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Godot.Bridge;

namespace Godot.NativeInterop;

internal static partial class MethodBind
{
    public static unsafe void GetAndCacheMethodBind(ref void* methodBind, StringName className, StringName methodName, [ConstantExpected] long methodHash)
    {
        if (methodBind is null)
        {
            NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;
            NativeGodotStringName methodNameNative = methodName.NativeValue.DangerousSelfRef;
            methodBind = GodotBridge.GDExtensionInterface.classdb_get_method_bind(&classNameNative, &methodNameNative, methodHash);
        }

        MissingGodotMethodException.ThrowIfNull(methodBind, SR.MissingGodotMethod_MethodBindNotFound);
    }

    public static unsafe void GetAndCacheUtilityFunction(ref delegate* unmanaged[Cdecl]<void*, void**, int, void> utilityFunction, ReadOnlySpan<byte> methodNameAscii, [ConstantExpected] long methodHash)
    {
        if (utilityFunction is null)
        {
            using NativeGodotStringName methodNameNative = NativeGodotStringName.Create(methodNameAscii, isStatic: false);
            utilityFunction = GodotBridge.GDExtensionInterface.variant_get_ptr_utility_function(&methodNameNative, methodHash);
        }

        MissingGodotMethodException.ThrowIfNull(utilityFunction, SR.MissingGodotMethod_UtilityFunctionNotFound);
    }

    public static unsafe void GetAndCacheBuiltInMethod(ref delegate* unmanaged[Cdecl]<void*, void**, void*, int, void> builtInMethod, GDExtensionVariantType variantType, ReadOnlySpan<byte> methodNameAscii, [ConstantExpected] long methodHash)
    {
        if (builtInMethod is null)
        {
            using NativeGodotStringName methodNameNative = NativeGodotStringName.Create(methodNameAscii, isStatic: false);
            builtInMethod = GodotBridge.GDExtensionInterface.variant_get_ptr_builtin_method(variantType, &methodNameNative, methodHash);
        }

        MissingGodotMethodException.ThrowIfNull(builtInMethod, SR.MissingGodotMethod_BuiltInNotFound);
    }

    public static unsafe void GetAndCacheBuiltInConstructor(ref delegate* unmanaged[Cdecl]<void*, void**, void> constructor, GDExtensionVariantType variantType, int constructorIndex)
    {
        if (constructor is null)
        {
            constructor = GodotBridge.GDExtensionInterface.variant_get_ptr_constructor(variantType, constructorIndex);
        }

        MissingGodotMethodException.ThrowIfNull(constructor, SR.MissingGodotMethod_ConstructorNotFound);
    }

    public static unsafe void GetAndCacheBuiltInDestructor(ref delegate* unmanaged[Cdecl]<void*, void> destructor, GDExtensionVariantType variantType)
    {
        if (destructor is null)
        {
            destructor = GodotBridge.GDExtensionInterface.variant_get_ptr_destructor(variantType);
        }

        MissingGodotMethodException.ThrowIfNull(destructor, SR.MissingGodotMethod_DestructorNotFound);
    }

    public static unsafe void GetAndCacheBuiltInOperator(ref delegate* unmanaged[Cdecl]<void*, void*, void*, void> operatorMethod, GDExtensionVariantOperator operatorKind, GDExtensionVariantType leftVariantType, GDExtensionVariantType rightVariantType)
    {
        if (operatorMethod is null)
        {
            operatorMethod = GodotBridge.GDExtensionInterface.variant_get_ptr_operator_evaluator(operatorKind, leftVariantType, rightVariantType);
        }

        MissingGodotMethodException.ThrowIfNull(operatorMethod, SR.MissingGodotMethod_OperatorEvaluatorNotFound);
    }

    [Conditional("DEBUG")]
    public static unsafe void DebugCheckCallError(scoped NativeGodotStringName method, void* instance, NativeGodotVariantPtrSpan args, GDExtensionCallError error)
    {
        if (error.error == GDExtensionCallErrorType.GDEXTENSION_CALL_OK)
        {
            return;
        }

        using NativeGodotVariant instanceVariant = NativeGodotVariant.CreateFromObject((nint)instance);
        string where = GetCallErrorWhere(method, instanceVariant, args);
        string errorText = GetCallErrorMessage(error, where, args);
        GD.PushError(errorText);

        static string GetCallErrorWhere(scoped NativeGodotStringName method, scoped in NativeGodotVariant instance, NativeGodotVariantPtrSpan args)
        {
            string? methodstr = null;
            string basestr = GetVariantTypeName(instance);

            if (method == GodotObject.MethodName.Call || (basestr == "Godot.TreeItem" && method == TreeItem.MethodName.CallRecursive))
            {
                if (args.Length >= 1)
                {
                    methodstr = NativeGodotVariant.GetOrConvertToString(args[0]).ToString();
                }
            }

            if (string.IsNullOrEmpty(methodstr))
            {
                methodstr = StringName.CreateTakingOwnership(method).ToString();
            }

            return $"function '{methodstr}' in base '{basestr}'";
        }

        static string GetCallErrorMessage(GDExtensionCallError error, string where, NativeGodotVariantPtrSpan args)
        {
            switch (error.error)
            {
                case GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INVALID_ARGUMENT:
                {
                    int errorarg = error.argument;
                    VariantType expected = (VariantType)error.expected;
                    // Handle the Object to Object case separately as we don't have further class details.
#if DEBUG
                    if (expected == VariantType.Object && args[errorarg].Type == expected)
                    {
                        return SR.FormatCallError_InvalidArgument_TypeMismatchObjectDerived(where, errorarg + 1, GetVariantTypeName(args[errorarg]));
                    }
                    else if (expected == VariantType.Array && args[errorarg].Type == expected)
                    {
                        return SR.FormatCallError_InvalidArgument_TypeMismatchArrayElementType(where, errorarg + 1, GetVariantTypeName(args[errorarg]));
                    }
                    else
#endif
                    {
                        return SR.FormatCallError_InvalidArgument_TypeMismatch(where, errorarg + 1, args[errorarg].Type, expected);
                    }
                }
                case GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_TOO_MANY_ARGUMENTS:
                case GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_TOO_FEW_ARGUMENTS:
                    return SR.FormatCallError_ArgumentCountMismatch(where, error.expected);
                case GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INVALID_METHOD:
                    return SR.FormatCallError_InvalidMethod(where);
                case GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INSTANCE_IS_NULL:
                    return SR.FormatCallError_InstanceIsNull(where);
                case GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_METHOD_NOT_CONST:
                    return SR.FormatCallError_MethodNotConst(where);
                default:
                    return $"Bug, call error: #{error.error}";
            }
        }

        static string GetVariantTypeName(scoped in NativeGodotVariant variant)
        {
            if (variant.Type == VariantType.Object)
            {
                GodotObject? obj = Marshallers.GodotObjectMarshaller.ConvertFromVariant(variant.GetUnsafeAddress());
                if (obj is null)
                {
                    return "null instance";
                }
                else if (!GodotObject.IsInstanceValid(obj))
                {
                    return "previously freed";
                }
                else
                {
                    return obj.GetType().ToString();
                }
            }

            return variant.Type.ToString();
        }
    }
}

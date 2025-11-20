using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.NativeInterop;

namespace Godot.Bridge;

partial class ClassRegistrationContext
{
    private readonly HashSet<StringName> _registeredMethods = new(StringNameEqualityComparer.Default);

    private readonly Dictionary<StringName, GCHandle> _registeredMethodHandles = [];

    // The MethodInfo must be referenced somewhere so the GC doesn't release it.
    // We need to keep it alive because it contains the MethodBindInvoker that
    // invokes the method in the 'call_func' and 'ptrcall_func' callbacks.
    private readonly Dictionary<StringName, MethodInfo> _registeredMethodImplementations = new(StringNameEqualityComparer.Default);

    /// <summary>
    /// Register a method in the class.
    /// </summary>
    /// <param name="methodInfo">Information that describes the method to register.</param>
    /// <exception cref="ArgumentException">
    /// A method has already been registered with the same name.
    /// </exception>
    public unsafe void BindMethod(MethodInfo methodInfo)
    {
        if (!_registeredMethods.Add(methodInfo.Name))
        {
            throw new ArgumentException(SR.FormatArgument_MethodAlreadyRegistered(methodInfo.Name, ClassName), nameof(methodInfo));
        }

        _registeredMethodImplementations[methodInfo.Name] = methodInfo;

        _registerBindingActions.Enqueue(() =>
        {
            // Convert managed method info to the internal unmanaged type.
            var methodInfoNative = new GDExtensionClassMethodInfo();
            {
                NativeGodotStringName nameNative = methodInfo.Name.NativeValue.DangerousSelfRef;
                methodInfoNative.name = &nameNative;

                var methodFlags = GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAGS_DEFAULT;
                if (methodInfo.IsStatic)
                {
                    methodFlags |= GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAG_STATIC;
                }
                methodInfoNative.method_flags = (uint)methodFlags;

                // Return

                if (methodInfo.Return is not null)
                {
                    // Convert managed property info to the internal unmanaged type.
                    GDExtensionPropertyInfo ret;
                    {
                        NativeGodotStringName returnNameNative = methodInfo.Return.Name.NativeValue.DangerousSelfRef;
                        NativeGodotStringName returnClassNameNative = (methodInfo.Return.ClassName?.NativeValue ?? default).DangerousSelfRef;
                        NativeGodotString hintStringNative = NativeGodotString.Create(methodInfo.Return.HintString);

                        ret = new GDExtensionPropertyInfo
                        {
                            type = (GDExtensionVariantType)methodInfo.Return.Type,
                            name = &returnNameNative,

                            hint = (uint)methodInfo.Return.Hint,
                            hint_string = &hintStringNative,
                            class_name = &returnClassNameNative,
                            usage = (uint)methodInfo.Return.Usage,
                        };
                    }

                    methodInfoNative.has_return_value = true;
                    methodInfoNative.return_value_info = &ret;
                    methodInfoNative.return_value_metadata = (GDExtensionClassMethodArgumentMetadata)methodInfo.Return.TypeMetadata;
                }

                // Parameters

                var args = stackalloc GDExtensionPropertyInfo[methodInfo.Parameters.Count];
                var argsMetadata = stackalloc GDExtensionClassMethodArgumentMetadata[methodInfo.Parameters.Count];
                var argsDefaultValues = stackalloc NativeGodotVariant*[methodInfo.Parameters.Count];

                uint optionalParameterCount = 0;
                for (int i = 0; i < methodInfo.Parameters.Count; i++)
                {
                    var parameter = methodInfo.Parameters[i];

                    if (optionalParameterCount > 0 && parameter.DefaultValue is null)
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_MethodOptionalParametersMustAppearAfterRequiredParameters);
                    }

                    if (parameter.DefaultValue is not null)
                    {
                        NativeGodotVariant defaultValue = parameter.DefaultValue.Value.NativeValue.DangerousSelfRef;
                        argsDefaultValues[optionalParameterCount++] = &defaultValue;
                    }

                    // Convert managed parameter info to the internal unmanaged type.
                    {
                        NativeGodotStringName parameterNameNative = parameter.Name.NativeValue.DangerousSelfRef;
                        NativeGodotStringName parameterClassNameNative = (parameter.ClassName?.NativeValue ?? default).DangerousSelfRef;
                        NativeGodotString hintStringNative = NativeGodotString.Create(parameter.HintString);

                        args[i] = new GDExtensionPropertyInfo
                        {
                            type = (GDExtensionVariantType)parameter.Type,
                            name = &parameterNameNative,

                            hint = (uint)parameter.Hint,
                            hint_string = &hintStringNative,
                            class_name = &parameterClassNameNative,
                            usage = (uint)parameter.Usage,
                        };
                    }
                    argsMetadata[i] = (GDExtensionClassMethodArgumentMetadata)parameter.TypeMetadata;
                }

                methodInfoNative.argument_count = (uint)methodInfo.Parameters.Count;
                methodInfoNative.arguments_info = args;
                methodInfoNative.arguments_metadata = argsMetadata;

                methodInfoNative.default_argument_count = optionalParameterCount;
                methodInfoNative.default_arguments = argsDefaultValues;
            }

            var methodGCHandle = GCHandle.Alloc(methodInfo, GCHandleType.Normal);
            _registeredMethodHandles.Add(methodInfo.Name, methodGCHandle);

            nint methodInfoPtr = GCHandle.ToIntPtr(methodGCHandle);
            methodInfoNative.call_func = &CallWithVariantArgs_Native;
            methodInfoNative.ptrcall_func = &CallWithPtrArgs_Native;
            methodInfoNative.method_userdata = (void*)methodInfoPtr;

            NativeGodotStringName classNameNative = ClassName.NativeValue.DangerousSelfRef;

            GodotBridge.GDExtensionInterface.classdb_register_extension_class_method(GodotBridge.LibraryPtr, &classNameNative, &methodInfoNative);
        });
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void CallWithPtrArgs_Native(void* methodUserData, void* instance, void** args, void* outRet)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)methodUserData);
        var method = (MethodInfo?)gcHandle.Target;

        Debug.Assert(method is not null);

        method.Invoker.CallWithPtrArgs(method, instance, args, outRet);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void CallWithVariantArgs_Native(void* methodUserData, void* instance, NativeGodotVariant** args, long argCount, NativeGodotVariant* outRet, GDExtensionCallError* outError)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)methodUserData);
        var method = (MethodInfo?)gcHandle.Target;

        Debug.Assert(method is not null);

        method.Invoker.CallWithVariantArgs(method, instance, new NativeGodotVariantPtrSpan(args, (int)argCount), outRet, outError);
    }
}

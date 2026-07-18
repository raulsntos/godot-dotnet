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

    private readonly Dictionary<StringName, GCHandle<MethodDefinition>> _registeredMethodHandles = [];

    // The MethodDefinition must be referenced somewhere so the GC doesn't release it.
    // We need to keep it alive because it contains the MethodBindInvoker that
    // invokes the method in the 'call_func' and 'ptrcall_func' callbacks.
    private readonly Dictionary<StringName, MethodDefinition> _registeredMethodImplementations = new(StringNameEqualityComparer.Default);

    /// <summary>
    /// Register a method in the class.
    /// </summary>
    /// <param name="methodDefinition">Information that describes the method to register.</param>
    /// <exception cref="ArgumentException">
    /// A method has already been registered with the same name.
    /// </exception>
    public unsafe void BindMethod(MethodDefinition methodDefinition)
    {
        if (!_registeredMethods.Add(methodDefinition.Name))
        {
            throw new ArgumentException(SR.FormatArgument_MethodAlreadyRegistered(methodDefinition.Name, ClassName), nameof(methodDefinition));
        }

        _registeredMethodImplementations[methodDefinition.Name] = methodDefinition;

        _registerBindingActions.Enqueue(() =>
        {
            int parameterCount = methodDefinition.Parameters.Count;

            // Convert managed method info to the internal unmanaged type.
            // The engine reads the pointers stored in 'methodInfoNative' when the method
            // is registered at the end, so every value that is pointed to must be kept
            // alive, in its own stack slot, until then. Block or loop scoped locals must
            // not be used for these values because their stack slots could be reused,
            // leaving dangling or aliased pointers behind.
            var methodInfoNative = new GDExtensionClassMethodInfo();

            NativeGodotStringName nameNative = methodDefinition.Name.NativeValue.DangerousSelfRef;
            methodInfoNative.name = &nameNative;

            var methodFlags = GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAGS_DEFAULT;
            if (methodDefinition.IsStatic)
            {
                methodFlags |= GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAG_STATIC;
            }
            methodInfoNative.method_flags = (uint)methodFlags;

            // Return

            GDExtensionPropertyInfo returnInfoNative = default;
            NativeGodotStringName returnNameNative = default;
            NativeGodotStringName returnClassNameNative = default;
            NativeGodotString returnHintStringNative = default;

            if (methodDefinition.Return is not null)
            {
                // Convert managed property info to the internal unmanaged type.
                returnNameNative = methodDefinition.Return.Name.NativeValue.DangerousSelfRef;
                returnClassNameNative = (methodDefinition.Return.ClassName?.NativeValue ?? default).DangerousSelfRef;
                returnHintStringNative = NativeGodotString.Create(methodDefinition.Return.HintString);

                returnInfoNative = new GDExtensionPropertyInfo()
                {
                    type = (GDExtensionVariantType)methodDefinition.Return.Type,
                    name = &returnNameNative,

                    hint = (uint)methodDefinition.Return.Hint,
                    hint_string = &returnHintStringNative,
                    class_name = &returnClassNameNative,
                    usage = (uint)methodDefinition.Return.Usage,
                };

                methodInfoNative.has_return_value = true;
                methodInfoNative.return_value_info = &returnInfoNative;
                methodInfoNative.return_value_metadata = (GDExtensionClassMethodArgumentMetadata)methodDefinition.Return.TypeMetadata;
            }

            // Parameters

            var args = stackalloc GDExtensionPropertyInfo[parameterCount];
            var argsMetadata = stackalloc GDExtensionClassMethodArgumentMetadata[parameterCount];
            var argsDefaultValues = stackalloc NativeGodotVariant*[parameterCount];

            // Parallel buffers with one slot for each parameter, so the pointers stored
            // in 'args' and 'argsDefaultValues' remain distinct and valid until the
            // method is registered below.
            var argsNamesNative = stackalloc NativeGodotStringName[parameterCount];
            var argsClassNamesNative = stackalloc NativeGodotStringName[parameterCount];
            var argsHintStringsNative = stackalloc NativeGodotString[parameterCount];
            var argsDefaultValuesNative = stackalloc NativeGodotVariant[parameterCount];

            uint optionalParameterCount = ConvertParameterInfosToNative(methodDefinition.Parameters, args, argsMetadata, argsDefaultValues, argsNamesNative, argsClassNamesNative, argsHintStringsNative, argsDefaultValuesNative);

            methodInfoNative.argument_count = (uint)parameterCount;
            methodInfoNative.arguments_info = args;
            methodInfoNative.arguments_metadata = argsMetadata;

            methodInfoNative.default_argument_count = optionalParameterCount;
            methodInfoNative.default_arguments = argsDefaultValues;

            var methodGCHandle = new GCHandle<MethodDefinition>(methodDefinition);
            _registeredMethodHandles.Add(methodDefinition.Name, methodGCHandle);

            nint methodDefinitionPtr = GCHandle<MethodDefinition>.ToIntPtr(methodGCHandle);
            methodInfoNative.call_func = &CallWithVariantArgs_Native;
            methodInfoNative.ptrcall_func = &CallWithPtrArgs_Native;
            methodInfoNative.method_userdata = (void*)methodDefinitionPtr;

            NativeGodotStringName classNameNative = ClassName.NativeValue.DangerousSelfRef;

            GodotBridge.GDExtensionInterface.classdb_register_extension_class_method(GodotBridge.LibraryPtr, &classNameNative, &methodInfoNative);

            // The engine copies the data when the method is registered, so the native
            // strings created for the conversion can be destroyed now.
            returnHintStringNative.Dispose();
            for (int i = 0; i < parameterCount; i++)
            {
                argsHintStringsNative[i].Dispose();
            }
        });
    }

    /// <summary>
    /// Converts the managed parameter definitions to the internal unmanaged type,
    /// filling the buffers provided by the caller. All the buffers must have one
    /// slot for each parameter and must be pinned or stack allocated because
    /// <paramref name="args"/> and <paramref name="argsDefaultValues"/> store
    /// pointers to the slots of the other buffers, which must remain valid for
    /// as long as the converted parameter information is in use.
    /// </summary>
    /// <returns>The number of optional parameters (i.e.: parameters with a default value).</returns>
    internal static unsafe uint ConvertParameterInfosToNative(List<ParameterDefinition> parameters, GDExtensionPropertyInfo* args, GDExtensionClassMethodArgumentMetadata* argsMetadata, NativeGodotVariant** argsDefaultValues, NativeGodotStringName* argsNamesNative, NativeGodotStringName* argsClassNamesNative, NativeGodotString* argsHintStringsNative, NativeGodotVariant* argsDefaultValuesNative)
    {
        // Validate the parameter ordering up front, before any native strings are
        // created, so an invalid definition throws without leaking them.
        bool seenOptionalParameter = false;
        for (int i = 0; i < parameters.Count; i++)
        {
            if (parameters[i].DefaultValue is not null)
            {
                seenOptionalParameter = true;
            }
            else if (seenOptionalParameter)
            {
                throw new InvalidOperationException(SR.InvalidOperation_MethodOptionalParametersMustAppearAfterRequiredParameters);
            }
        }

        uint optionalParameterCount = 0;
        for (int i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];

            if (parameter.DefaultValue is not null)
            {
                argsDefaultValuesNative[i] = parameter.DefaultValue.Value.NativeValue.DangerousSelfRef;
                argsDefaultValues[optionalParameterCount++] = &argsDefaultValuesNative[i];
            }

            // Convert managed parameter info to the internal unmanaged type.
            argsNamesNative[i] = parameter.Name.NativeValue.DangerousSelfRef;
            argsClassNamesNative[i] = (parameter.ClassName?.NativeValue ?? default).DangerousSelfRef;
            argsHintStringsNative[i] = NativeGodotString.Create(parameter.HintString);

            args[i] = new GDExtensionPropertyInfo()
            {
                type = (GDExtensionVariantType)parameter.Type,
                name = &argsNamesNative[i],

                hint = (uint)parameter.Hint,
                hint_string = &argsHintStringsNative[i],
                class_name = &argsClassNamesNative[i],
                usage = (uint)parameter.Usage,
            };
            argsMetadata[i] = (GDExtensionClassMethodArgumentMetadata)parameter.TypeMetadata;
        }

        return optionalParameterCount;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void CallWithPtrArgs_Native(void* methodUserData, void* instance, void** args, void* outRet)
    {
        try
        {
            var gcHandle = GCHandle<MethodDefinition>.FromIntPtr((nint)methodUserData);
            var method = gcHandle.Target;

            method.Invoker.CallWithPtrArgs(method, instance, args, outRet);
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception)) { }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void CallWithVariantArgs_Native(void* methodUserData, void* instance, NativeGodotVariant** args, long argCount, NativeGodotVariant* outRet, GDExtensionCallError* outError)
    {
        try
        {
            var gcHandle = GCHandle<MethodDefinition>.FromIntPtr((nint)methodUserData);
            var method = gcHandle.Target;

            method.Invoker.CallWithVariantArgs(method, instance, new NativeGodotVariantPtrSpan(args, (int)argCount), outRet, outError);
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            outError->error = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INVALID_METHOD;
        }
    }
}

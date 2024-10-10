using System;
using Godot.NativeInterop;

namespace Godot.Bridge;

partial class ClassRegistrationContext
{
    /// <summary>
    /// Register a virtual method in the class.
    /// Virtual methods can be overridden in user scripts, but the extension implementation
    /// of the method must use <see cref="GodotObject.CallVirtualMethod(StringName)"/>
    /// or <see cref="GodotObject.TryCallVirtualMethod(StringName)"/> to call the script
    /// override.
    /// </summary>
    /// <param name="methodInfo">Information that describes the method to register.</param>
    /// <exception cref="ArgumentException">
    /// A method has already been registered with the same name.
    /// </exception>
    public unsafe void BindVirtualMethod(VirtualMethodInfo methodInfo)
    {
        if (!_registeredMethods.Add(methodInfo.Name))
        {
            throw new ArgumentException(SR.FormatArgument_MethodAlreadyRegistered(methodInfo.Name, ClassName), nameof(methodInfo));
        }

        // Convert managed method info to the internal unmanaged type.
        var methodInfoNative = new GDExtensionClassVirtualMethodInfo();
        {
            NativeGodotStringName nameNative = methodInfo.Name.NativeValue.DangerousSelfRef;
            methodInfoNative.name = &nameNative;

            var methodFlags = GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAGS_DEFAULT | GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAG_VIRTUAL;
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

                methodInfoNative.return_value = ret;
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
            methodInfoNative.arguments = args;
            methodInfoNative.arguments_metadata = argsMetadata;
        }

        NativeGodotStringName classNameNative = ClassName.NativeValue.DangerousSelfRef;

        GodotBridge.GDExtensionInterface.classdb_register_extension_class_virtual_method(GodotBridge.LibraryPtr, &classNameNative, &methodInfoNative);
    }
}

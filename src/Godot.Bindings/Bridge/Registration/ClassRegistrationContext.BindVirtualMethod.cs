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
    /// <param name="methodDefinition">Information that describes the method to register.</param>
    /// <exception cref="ArgumentException">
    /// A method has already been registered with the same name.
    /// </exception>
    public unsafe void BindVirtualMethod(VirtualMethodDefinition methodDefinition)
    {
        if (!_registeredMethods.Add(methodDefinition.Name))
        {
            throw new ArgumentException(SR.FormatArgument_MethodAlreadyRegistered(methodDefinition.Name, ClassName), nameof(methodDefinition));
        }

        _registerBindingActions.Enqueue(() =>
        {
            int parameterCount = methodDefinition.Parameters.Count;

            // Convert managed method info to the internal unmanaged type.
            // The engine reads the pointers stored in 'methodInfoNative' when the method
            // is registered at the end, so every value that is pointed to must be kept
            // alive, in its own stack slot, until then. Block or loop scoped locals must
            // not be used for these values because their stack slots could be reused,
            // leaving dangling or aliased pointers behind.
            var methodInfoNative = new GDExtensionClassVirtualMethodInfo();

            NativeGodotStringName nameNative = methodDefinition.Name.NativeValue.DangerousSelfRef;
            methodInfoNative.name = &nameNative;

            var methodFlags = GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAGS_DEFAULT | GDExtensionClassMethodFlags.GDEXTENSION_METHOD_FLAG_VIRTUAL;
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

                methodInfoNative.return_value = returnInfoNative;
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

            // Virtual method registration doesn't include default arguments, so the
            // optional parameter count and the default value buffers are unused.
            _ = ConvertParameterInfosToNative(methodDefinition.Parameters, args, argsMetadata, argsDefaultValues, argsNamesNative, argsClassNamesNative, argsHintStringsNative, argsDefaultValuesNative);

            methodInfoNative.argument_count = (uint)parameterCount;
            methodInfoNative.arguments = args;
            methodInfoNative.arguments_metadata = argsMetadata;

            NativeGodotStringName classNameNative = ClassName.NativeValue.DangerousSelfRef;

            GodotBridge.GDExtensionInterface.classdb_register_extension_class_virtual_method(GodotBridge.LibraryPtr, &classNameNative, &methodInfoNative);

            // The engine copies the data when the method is registered, so the native
            // strings created for the conversion can be destroyed now.
            returnHintStringNative.Dispose();
            for (int i = 0; i < parameterCount; i++)
            {
                argsHintStringsNative[i].Dispose();
            }
        });
    }
}

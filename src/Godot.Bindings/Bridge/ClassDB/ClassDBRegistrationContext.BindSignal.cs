using System;
using System.Collections.Generic;
using Godot.NativeInterop;

namespace Godot.Bridge;

partial class ClassDBRegistrationContext
{
    private readonly HashSet<StringName> _registeredSignals = [];

    private const int ParameterSpanThreshold = 8;

    /// <summary>
    /// Register a signal in the class.
    /// The registered class can be emitted with
    /// <see cref="GodotObject.EmitSignal(StringName, ReadOnlySpan{Variant})"/>
    /// using the name that the signal was registered with.
    /// </summary>
    /// <param name="signalInfo">Information that describes the signal to register.</param>
    /// <exception cref="ArgumentException">
    /// A signal has already been registered with the same name.
    /// </exception>
    public unsafe void BindSignal(SignalInfo signalInfo)
    {
        if (!_registeredSignals.Add(signalInfo.Name))
        {
            throw new ArgumentException(SR.FormatArgument_SignalAlreadyRegistered(signalInfo.Name, ClassName), nameof(signalInfo));
        }

        // Convert managed signal info to the internal unmanaged type.
        Span<GDExtensionPropertyInfo> parameters = signalInfo.Parameters.Count <= ParameterSpanThreshold
            ? stackalloc GDExtensionPropertyInfo[ParameterSpanThreshold].Slice(0, signalInfo.Parameters.Count)
            : new GDExtensionPropertyInfo[signalInfo.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = signalInfo.Parameters[i];

            NativeGodotStringName parameterNameNative = parameterInfo.Name.NativeValue.DangerousSelfRef;
            NativeGodotStringName parameterClassNameNative = (parameterInfo.ClassName?.NativeValue ?? default).DangerousSelfRef;
            NativeGodotString hintStringNative = NativeGodotString.Create(parameterInfo.HintString);

            parameters[i] = new GDExtensionPropertyInfo()
            {
                type = (GDExtensionVariantType)parameterInfo.Type,
                name = &parameterNameNative,

                hint = (uint)parameterInfo.Hint,
                hint_string = &hintStringNative,
                class_name = &parameterClassNameNative,
                usage = (uint)parameterInfo.Usage,
            };
        }

        NativeGodotStringName signalNameNative = signalInfo.Name.NativeValue.DangerousSelfRef;

        NativeGodotStringName classNameNative = ClassName.NativeValue.DangerousSelfRef;

        fixed (GDExtensionPropertyInfo* parametersPtr = parameters)
        {
            GodotBridge.GDExtensionInterface.classdb_register_extension_class_signal(GodotBridge.LibraryPtr, &classNameNative, &signalNameNative, parametersPtr, parameters.Length);
        }
    }
}

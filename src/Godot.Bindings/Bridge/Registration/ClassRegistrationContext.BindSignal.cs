using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot.NativeInterop;

namespace Godot.Bridge;

partial class ClassRegistrationContext
{
    private readonly HashSet<StringName> _registeredSignals = new(StringNameEqualityComparer.Default);

    private const int ParameterSpanThreshold = 8;

    /// <summary>
    /// Register a signal in the class.
    /// The registered class can be emitted with
    /// <see cref="GodotObject.EmitSignal(StringName, ReadOnlySpan{Variant})"/>
    /// using the name that the signal was registered with.
    /// </summary>
    /// <param name="signalDefinition">Information that describes the signal to register.</param>
    /// <exception cref="ArgumentException">
    /// A signal has already been registered with the same name.
    /// </exception>
    public unsafe void BindSignal(SignalDefinition signalDefinition)
    {
        if (!_registeredSignals.Add(signalDefinition.Name))
        {
            throw new ArgumentException(SR.FormatArgument_SignalAlreadyRegistered(signalDefinition.Name, ClassName), nameof(signalDefinition));
        }

        _registerBindingActions.Enqueue(() =>
        {
            int parameterCount = signalDefinition.Parameters.Count;

            // Convert managed signal info to the internal unmanaged type.
            Span<GDExtensionPropertyInfo> parameters = parameterCount <= ParameterSpanThreshold
                ? stackalloc GDExtensionPropertyInfo[ParameterSpanThreshold].Slice(0, parameterCount)
                : new GDExtensionPropertyInfo[parameterCount];

            // Parallel buffers with one slot for each parameter, so the pointers stored
            // in 'parameters' remain distinct and valid until the signal is registered
            // below. Loop scoped locals must not be used for these values because their
            // stack slots would be reused by every iteration, leaving all the pointers
            // aliasing the same address. The slot types are ref structs, so the buffers
            // can't use managed arrays like 'parameters' does; when the parameter count
            // exceeds 'ParameterSpanThreshold' allocate the buffers from native memory
            // instead, which doesn't require pinning.
            var parameterNamesStackBuffer = stackalloc NativeGodotStringName[ParameterSpanThreshold];
            var parameterClassNamesStackBuffer = stackalloc NativeGodotStringName[ParameterSpanThreshold];
            var parameterHintStringsStackBuffer = stackalloc NativeGodotString[ParameterSpanThreshold];

            NativeGodotStringName* parameterNamesNative = parameterNamesStackBuffer;
            NativeGodotStringName* parameterClassNamesNative = parameterClassNamesStackBuffer;
            NativeGodotString* parameterHintStringsNative = parameterHintStringsStackBuffer;

            if (parameterCount > ParameterSpanThreshold)
            {
                parameterNamesNative = (NativeGodotStringName*)NativeMemory.Alloc((nuint)parameterCount, (nuint)sizeof(NativeGodotStringName));
                parameterClassNamesNative = (NativeGodotStringName*)NativeMemory.Alloc((nuint)parameterCount, (nuint)sizeof(NativeGodotStringName));
                parameterHintStringsNative = (NativeGodotString*)NativeMemory.Alloc((nuint)parameterCount, (nuint)sizeof(NativeGodotString));
            }

            try
            {
                fixed (GDExtensionPropertyInfo* parametersPtr = parameters)
                {
                    ConvertSignalParameterInfosToNative(signalDefinition.Parameters, parametersPtr, parameterNamesNative, parameterClassNamesNative, parameterHintStringsNative);

                    NativeGodotStringName signalNameNative = signalDefinition.Name.NativeValue.DangerousSelfRef;

                    NativeGodotStringName classNameNative = ClassName.NativeValue.DangerousSelfRef;

                    GodotBridge.GDExtensionInterface.classdb_register_extension_class_signal(GodotBridge.LibraryPtr, &classNameNative, &signalNameNative, parametersPtr, parameterCount);

                    // The engine copies the data when the signal is registered, so the native
                    // strings created for the conversion can be destroyed now.
                    for (int i = 0; i < parameterCount; i++)
                    {
                        parameterHintStringsNative[i].Dispose();
                    }
                }
            }
            finally
            {
                if (parameterCount > ParameterSpanThreshold)
                {
                    NativeMemory.Free(parameterNamesNative);
                    NativeMemory.Free(parameterClassNamesNative);
                    NativeMemory.Free(parameterHintStringsNative);
                }
            }
        });
    }

    /// <summary>
    /// Converts the managed parameter definitions to the internal unmanaged type,
    /// filling the buffers provided by the caller. All the buffers must have one
    /// slot for each parameter and must be pinned or stack allocated because
    /// <paramref name="parameters"/> stores pointers to the slots of the other
    /// buffers, which must remain valid for as long as the converted parameter
    /// information is in use.
    /// </summary>
    internal static unsafe void ConvertSignalParameterInfosToNative(List<ParameterDefinition> parameterDefinitions, GDExtensionPropertyInfo* parameters, NativeGodotStringName* parameterNamesNative, NativeGodotStringName* parameterClassNamesNative, NativeGodotString* parameterHintStringsNative)
    {
        for (int i = 0; i < parameterDefinitions.Count; i++)
        {
            var parameterDefinition = parameterDefinitions[i];

            parameterNamesNative[i] = parameterDefinition.Name.NativeValue.DangerousSelfRef;
            parameterClassNamesNative[i] = (parameterDefinition.ClassName?.NativeValue ?? default).DangerousSelfRef;
            parameterHintStringsNative[i] = NativeGodotString.Create(parameterDefinition.HintString);

            parameters[i] = new GDExtensionPropertyInfo()
            {
                type = (GDExtensionVariantType)parameterDefinition.Type,
                name = &parameterNamesNative[i],

                hint = (uint)parameterDefinition.Hint,
                hint_string = &parameterHintStringsNative[i],
                class_name = &parameterClassNamesNative[i],
                usage = (uint)parameterDefinition.Usage,
            };
        }
    }
}

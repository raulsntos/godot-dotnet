using System;
using System.Collections.Generic;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot.Bindings.Tests;

// These tests run without a Godot engine host, so every GDExtension interface
// function is a null function pointer and can't be called. Non-empty StringName
// and String values can't be created in this environment because they require
// interop calls, so the definitions below use empty names and null hint strings.
// The assertions target the address stored in each converted slot, which is
// exactly what the parameter aliasing bug corrupted (every parameter's pointers
// ended up aliasing the same reused stack slot, so all parameters registered
// with the last parameter's data), and the default values, which can be created
// without interop for the Bool, Int and Float variant types.
public class ClassRegistrationContextConversionTests
{
    [Fact]
    public unsafe void ConvertParameterInfosToNativeGivesEachParameterItsOwnSlot()
    {
        var parameters = new List<ParameterDefinition>
        {
            new ParameterDefinition(new StringName(""), VariantType.Int, VariantTypeMetadata.Int32, 1L),
            new ParameterDefinition(new StringName(""), VariantType.Float, VariantTypeMetadata.Double, 2.5),
            new ParameterDefinition(new StringName(""), VariantType.Bool, VariantTypeMetadata.None, true),
        };
        int parameterCount = parameters.Count;

        var args = stackalloc GDExtensionPropertyInfo[parameterCount];
        var argsMetadata = stackalloc GDExtensionClassMethodArgumentMetadata[parameterCount];
        var argsDefaultValues = stackalloc NativeGodotVariant*[parameterCount];
        var argsNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var argsClassNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var argsHintStringsNative = stackalloc NativeGodotString[parameterCount];
        var argsDefaultValuesNative = stackalloc NativeGodotVariant[parameterCount];

        uint optionalParameterCount = ClassRegistrationContext.ConvertParameterInfosToNative(parameters, args, argsMetadata, argsDefaultValues, argsNamesNative, argsClassNamesNative, argsHintStringsNative, argsDefaultValuesNative);

        Assert.Equal(3u, optionalParameterCount);

        for (int i = 0; i < parameterCount; i++)
        {
            // Every parameter's pointers must point to that parameter's own slot.
            Assert.Equal((nint)(&argsNamesNative[i]), (nint)args[i].name);
            Assert.Equal((nint)(&argsClassNamesNative[i]), (nint)args[i].class_name);
            Assert.Equal((nint)(&argsHintStringsNative[i]), (nint)args[i].hint_string);
            Assert.Equal((nint)(&argsDefaultValuesNative[i]), (nint)argsDefaultValues[i]);

            Assert.Equal((GDExtensionVariantType)parameters[i].Type, args[i].type);
            Assert.Equal((GDExtensionClassMethodArgumentMetadata)parameters[i].TypeMetadata, argsMetadata[i]);

            for (int j = i + 1; j < parameterCount; j++)
            {
                // The pointers of different parameters must be pairwise distinct.
                Assert.NotEqual((nint)args[i].name, (nint)args[j].name);
                Assert.NotEqual((nint)args[i].class_name, (nint)args[j].class_name);
                Assert.NotEqual((nint)args[i].hint_string, (nint)args[j].hint_string);
                Assert.NotEqual((nint)argsDefaultValues[i], (nint)argsDefaultValues[j]);
            }
        }

        // Each default value must dereference to the value of the parameter it
        // belongs to; the aliasing bug collapsed all the default values into the
        // last parameter's value.
        Assert.Equal(VariantType.Int, argsDefaultValues[0]->Type);
        Assert.Equal(1L, argsDefaultValues[0]->Int);
        Assert.Equal(VariantType.Float, argsDefaultValues[1]->Type);
        Assert.Equal(2.5, argsDefaultValues[1]->Float);
        Assert.Equal(VariantType.Bool, argsDefaultValues[2]->Type);
        Assert.True(argsDefaultValues[2]->Bool);
    }

    [Fact]
    public unsafe void ConvertParameterInfosToNativeThrowsWhenRequiredParameterFollowsOptional()
    {
        var parameters = new List<ParameterDefinition>
        {
            new ParameterDefinition(new StringName(""), VariantType.Int, VariantTypeMetadata.None, 1L),
            new ParameterDefinition(new StringName(""), VariantType.Int),
        };

        Assert.Throws<InvalidOperationException>(() => Convert(parameters));

        static unsafe void Convert(List<ParameterDefinition> parameters)
        {
            int parameterCount = parameters.Count;

            var args = stackalloc GDExtensionPropertyInfo[parameterCount];
            var argsMetadata = stackalloc GDExtensionClassMethodArgumentMetadata[parameterCount];
            var argsDefaultValues = stackalloc NativeGodotVariant*[parameterCount];
            var argsNamesNative = stackalloc NativeGodotStringName[parameterCount];
            var argsClassNamesNative = stackalloc NativeGodotStringName[parameterCount];
            var argsHintStringsNative = stackalloc NativeGodotString[parameterCount];
            var argsDefaultValuesNative = stackalloc NativeGodotVariant[parameterCount];

            ClassRegistrationContext.ConvertParameterInfosToNative(parameters, args, argsMetadata, argsDefaultValues, argsNamesNative, argsClassNamesNative, argsHintStringsNative, argsDefaultValuesNative);
        }
    }

    // Pins the compaction of default values when the optional parameters trail a
    // required parameter: 'argsDefaultValues' must contain one entry per optional
    // parameter, in order, each pointing at that parameter's own slot in
    // 'argsDefaultValuesNative' (indexed by parameter index, not by optional
    // index). A naive 'argsDefaultValues[i]' write instead of
    // 'argsDefaultValues[optionalParameterCount++]' would fail this test.
    [Fact]
    public unsafe void ConvertParameterInfosToNativeCompactsDefaultsForTrailingOptionalParameters()
    {
        var parameters = new List<ParameterDefinition>
        {
            new ParameterDefinition(new StringName(""), VariantType.Int),
            new ParameterDefinition(new StringName(""), VariantType.Int, VariantTypeMetadata.None, 10L),
            new ParameterDefinition(new StringName(""), VariantType.Bool, VariantTypeMetadata.None, true),
        };
        int parameterCount = parameters.Count;

        var args = stackalloc GDExtensionPropertyInfo[parameterCount];
        var argsMetadata = stackalloc GDExtensionClassMethodArgumentMetadata[parameterCount];
        var argsDefaultValues = stackalloc NativeGodotVariant*[parameterCount];
        var argsNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var argsClassNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var argsHintStringsNative = stackalloc NativeGodotString[parameterCount];
        var argsDefaultValuesNative = stackalloc NativeGodotVariant[parameterCount];

        uint optionalParameterCount = ClassRegistrationContext.ConvertParameterInfosToNative(parameters, args, argsMetadata, argsDefaultValues, argsNamesNative, argsClassNamesNative, argsHintStringsNative, argsDefaultValuesNative);

        Assert.Equal(2u, optionalParameterCount);

        // The compacted default value pointers must map to the trailing optional
        // parameters' own slots (parameter indices 1 and 2), not to the first
        // slots of 'argsDefaultValuesNative'.
        Assert.Equal((nint)(&argsDefaultValuesNative[1]), (nint)argsDefaultValues[0]);
        Assert.Equal((nint)(&argsDefaultValuesNative[2]), (nint)argsDefaultValues[1]);

        Assert.Equal(VariantType.Int, argsDefaultValues[0]->Type);
        Assert.Equal(10L, argsDefaultValues[0]->Int);
        Assert.Equal(VariantType.Bool, argsDefaultValues[1]->Type);
        Assert.True(argsDefaultValues[1]->Bool);

        // Sanity: the parameter name pointers must still be pairwise distinct.
        for (int i = 0; i < parameterCount; i++)
        {
            for (int j = i + 1; j < parameterCount; j++)
            {
                Assert.NotEqual((nint)args[i].name, (nint)args[j].name);
            }
        }
    }

    [Fact]
    public unsafe void ConvertSignalParameterInfosToNativeGivesEachParameterItsOwnSlot()
    {
        var parameters = new List<ParameterDefinition>
        {
            new ParameterDefinition(new StringName(""), VariantType.Int, VariantTypeMetadata.Int32),
            new ParameterDefinition(new StringName(""), VariantType.Float, VariantTypeMetadata.Double),
            new ParameterDefinition(new StringName(""), VariantType.Bool),
        };
        int parameterCount = parameters.Count;

        var parametersNative = stackalloc GDExtensionPropertyInfo[parameterCount];
        var parameterNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var parameterClassNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var parameterHintStringsNative = stackalloc NativeGodotString[parameterCount];

        ClassRegistrationContext.ConvertSignalParameterInfosToNative(parameters, parametersNative, parameterNamesNative, parameterClassNamesNative, parameterHintStringsNative);

        for (int i = 0; i < parameterCount; i++)
        {
            // Every parameter's pointers must point to that parameter's own slot.
            Assert.Equal((nint)(&parameterNamesNative[i]), (nint)parametersNative[i].name);
            Assert.Equal((nint)(&parameterClassNamesNative[i]), (nint)parametersNative[i].class_name);
            Assert.Equal((nint)(&parameterHintStringsNative[i]), (nint)parametersNative[i].hint_string);

            Assert.Equal((GDExtensionVariantType)parameters[i].Type, parametersNative[i].type);

            for (int j = i + 1; j < parameterCount; j++)
            {
                // The pointers of different parameters must be pairwise distinct.
                Assert.NotEqual((nint)parametersNative[i].name, (nint)parametersNative[j].name);
                Assert.NotEqual((nint)parametersNative[i].class_name, (nint)parametersNative[j].class_name);
                Assert.NotEqual((nint)parametersNative[i].hint_string, (nint)parametersNative[j].hint_string);
            }
        }
    }

    // Exercises the conversion helper with a parameter count above
    // 'ParameterSpanThreshold' (8). BindSignal's actual NativeMemory.Alloc/Free
    // heap branch calls 'classdb_register_extension_class_signal' and therefore
    // requires a running Godot editor, so it is covered by integration tests,
    // not here.
    [Fact]
    public unsafe void ConvertSignalParameterInfosToNativeWithManyParametersGivesEachItsOwnSlot()
    {
        var parameters = new List<ParameterDefinition>();
        for (int i = 0; i < 10; i++)
        {
            parameters.Add(new ParameterDefinition(new StringName(""), VariantType.Int));
        }
        int parameterCount = parameters.Count;

        var parametersNative = stackalloc GDExtensionPropertyInfo[parameterCount];
        var parameterNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var parameterClassNamesNative = stackalloc NativeGodotStringName[parameterCount];
        var parameterHintStringsNative = stackalloc NativeGodotString[parameterCount];

        ClassRegistrationContext.ConvertSignalParameterInfosToNative(parameters, parametersNative, parameterNamesNative, parameterClassNamesNative, parameterHintStringsNative);

        for (int i = 0; i < parameterCount; i++)
        {
            // Every parameter's pointers must point to that parameter's own slot.
            Assert.Equal((nint)(&parameterNamesNative[i]), (nint)parametersNative[i].name);
            Assert.Equal((nint)(&parameterClassNamesNative[i]), (nint)parametersNative[i].class_name);
            Assert.Equal((nint)(&parameterHintStringsNative[i]), (nint)parametersNative[i].hint_string);

            for (int j = i + 1; j < parameterCount; j++)
            {
                // The pointers of different parameters must be pairwise distinct.
                Assert.NotEqual((nint)parametersNative[i].name, (nint)parametersNative[j].name);
                Assert.NotEqual((nint)parametersNative[i].class_name, (nint)parametersNative[j].class_name);
                Assert.NotEqual((nint)parametersNative[i].hint_string, (nint)parametersNative[j].hint_string);
            }
        }
    }
}

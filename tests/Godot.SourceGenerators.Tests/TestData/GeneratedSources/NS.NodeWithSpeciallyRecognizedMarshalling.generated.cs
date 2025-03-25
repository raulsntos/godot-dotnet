#nullable enable

namespace NS;

partial class NodeWithSpeciallyRecognizedMarshalling
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
        public static global::Godot.StringName @MethodThatTakesArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesArrayOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsArrayOfInts"u8);
        public static global::Godot.StringName @MethodThatTakesListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesListOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsListOfInts"u8);
        public static global::Godot.StringName @MethodThatTakesArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesArrayOfBooleans"u8);
        public static global::Godot.StringName @MethodThatReturnsArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsArrayOfBooleans"u8);
        public static global::Godot.StringName @MethodThatTakesListOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesListOfBooleans"u8);
        public static global::Godot.StringName @MethodThatReturnsListOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsListOfBooleans"u8);
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @ArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("ArrayOfInts"u8);
        public static global::Godot.StringName @ListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("ListOfInts"u8);
        public static global::Godot.StringName @ArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("ArrayOfBooleans"u8);
        public static global::Godot.StringName @ListOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("ListOfBooleans"u8);
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
    internal static void BindMethods(global::Godot.Bridge.ClassRegistrationContext context)
    {
        context.BindConstructor(() => new NodeWithSpeciallyRecognizedMarshalling());
        context.BindMethod(MethodName.@MethodThatTakesArrayOfInts,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("array"u8), global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array @array) =>
            {
                __instance.@MethodThatTakesArrayOfInts([.. @array]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsArrayOfInts,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@MethodThatReturnsArrayOfInts()]);
            });
        context.BindMethod(MethodName.@MethodThatTakesListOfInts,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("list"u8), global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array @list) =>
            {
                __instance.@MethodThatTakesListOfInts([.. @list]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsListOfInts,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@MethodThatReturnsListOfInts()]);
            });
        context.BindMethod(MethodName.@MethodThatTakesArrayOfBooleans,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("array"u8), global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.GodotArray<bool> @array) =>
            {
                __instance.@MethodThatTakesArrayOfBooleans([.. @array]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsArrayOfBooleans,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.GodotArray<bool>)([.. __instance.@MethodThatReturnsArrayOfBooleans()]);
            });
        context.BindMethod(MethodName.@MethodThatTakesListOfBooleans,
            new global::Godot.Bridge.ParameterInfo(global::Godot.StringName.CreateStaticFromAscii("list"u8), global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.GodotArray<bool> @list) =>
            {
                __instance.@MethodThatTakesListOfBooleans([.. @list]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsListOfBooleans,
            new global::Godot.Bridge.ReturnInfo(global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.GodotArray<bool>)([.. __instance.@MethodThatReturnsListOfBooleans()]);
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@ArrayOfInts, global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@ArrayOfInts]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array value) =>
            {
                __instance.@ArrayOfInts = [.. value];
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@ListOfInts, global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@ListOfInts]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array value) =>
            {
                __instance.@ListOfInts = [.. value];
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@ArrayOfBooleans, global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.GodotArray<bool>)([.. __instance.@ArrayOfBooleans]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.GodotArray<bool> value) =>
            {
                __instance.@ArrayOfBooleans = [.. value];
            });
        context.BindProperty(new global::Godot.Bridge.PropertyInfo(PropertyName.@ListOfBooleans, global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.GodotArray<bool>)([.. __instance.@ListOfBooleans]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.GodotArray<bool> value) =>
            {
                __instance.@ListOfBooleans = [.. value];
            });
    }
}

#nullable enable

namespace NS;

partial class @NodeWithSpeciallyRecognizedMarshalling
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
        public static global::Godot.StringName @MethodThatTakesArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesArrayOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsArrayOfInts"u8);
        public static global::Godot.StringName @MethodThatTakesListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesListOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsListOfInts"u8);
        public static global::Godot.StringName @MethodThatTakesImmutableArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesImmutableArrayOfInts"u8);
        public static global::Godot.StringName @MethodThatTakesIListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesIListOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsImmutableArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsImmutableArrayOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsIEnumerableOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsIEnumerableOfInts"u8);
        public static global::Godot.StringName @MethodThatTakesICollectionOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesICollectionOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsCollectionOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsCollectionOfInts"u8);
        public static global::Godot.StringName @MethodThatReturnsReadOnlyCollectionOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsReadOnlyCollectionOfInts"u8);
        public static global::Godot.StringName @MethodThatTakesArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesArrayOfBooleans"u8);
        public static global::Godot.StringName @MethodThatReturnsArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsArrayOfBooleans"u8);
        public static global::Godot.StringName @MethodThatTakesListOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesListOfBooleans"u8);
        public static global::Godot.StringName @MethodThatReturnsListOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsListOfBooleans"u8);
        public static global::Godot.StringName @MethodThatTakesImmutableArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatTakesImmutableArrayOfBooleans"u8);
        public static global::Godot.StringName @MethodThatReturnsImmutableArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsImmutableArrayOfBooleans"u8);
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @ArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("ArrayOfInts"u8);
        public static global::Godot.StringName @ListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("ListOfInts"u8);
        public static global::Godot.StringName @ImmutableArrayOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("ImmutableArrayOfInts"u8);
        public static global::Godot.StringName @IReadOnlyListOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("IReadOnlyListOfInts"u8);
        public static global::Godot.StringName @IReadOnlyCollectionOfInts { get; } = global::Godot.StringName.CreateStaticFromAscii("IReadOnlyCollectionOfInts"u8);
        public static global::Godot.StringName @ArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("ArrayOfBooleans"u8);
        public static global::Godot.StringName @ListOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("ListOfBooleans"u8);
        public static global::Godot.StringName @ImmutableArrayOfBooleans { get; } = global::Godot.StringName.CreateStaticFromAscii("ImmutableArrayOfBooleans"u8);
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
    }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    {
        global::System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(global::Godot.Collections.GodotArray<bool>).TypeHandle);
        context.BindConstructor(() => new global::NS.NodeWithSpeciallyRecognizedMarshalling());
        context.BindMethod(MethodName.@MethodThatTakesArrayOfInts,
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("array"u8), global::Godot.VariantType.PackedInt32Array)
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
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.PackedInt32Array)
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
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("list"u8), global::Godot.VariantType.PackedInt32Array)
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
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@MethodThatReturnsListOfInts()]);
            });
        context.BindMethod(MethodName.@MethodThatTakesImmutableArrayOfInts,
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("array"u8), global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array @array) =>
            {
                __instance.@MethodThatTakesImmutableArrayOfInts([.. @array]);
            });
        context.BindMethod(MethodName.@MethodThatTakesIListOfInts,
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("list"u8), global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array @list) =>
            {
                __instance.@MethodThatTakesIListOfInts([.. @list]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsImmutableArrayOfInts,
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@MethodThatReturnsImmutableArrayOfInts()]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsIEnumerableOfInts,
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@MethodThatReturnsIEnumerableOfInts()]);
            });
        context.BindMethod(MethodName.@MethodThatTakesICollectionOfInts,
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("collection"u8), global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array @collection) =>
            {
                __instance.@MethodThatTakesICollectionOfInts([.. @collection]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsCollectionOfInts,
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@MethodThatReturnsCollectionOfInts()]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsReadOnlyCollectionOfInts,
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@MethodThatReturnsReadOnlyCollectionOfInts()]);
            });
        context.BindMethod(MethodName.@MethodThatTakesArrayOfBooleans,
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("array"u8), global::Godot.VariantType.Array)
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
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.Array)
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
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("list"u8), global::Godot.VariantType.Array)
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
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.GodotArray<bool>)([.. __instance.@MethodThatReturnsListOfBooleans()]);
            });
        context.BindMethod(MethodName.@MethodThatTakesImmutableArrayOfBooleans,
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("array"u8), global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.GodotArray<bool> @array) =>
            {
                __instance.@MethodThatTakesImmutableArrayOfBooleans([.. @array]);
            });
        context.BindMethod(MethodName.@MethodThatReturnsImmutableArrayOfBooleans,
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.GodotArray<bool>)([.. __instance.@MethodThatReturnsImmutableArrayOfBooleans()]);
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@ArrayOfInts, global::Godot.VariantType.PackedInt32Array)
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
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@ListOfInts, global::Godot.VariantType.PackedInt32Array)
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
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@ImmutableArrayOfInts, global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@ImmutableArrayOfInts]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array value) =>
            {
                __instance.@ImmutableArrayOfInts = [.. value];
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@IReadOnlyListOfInts, global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@IReadOnlyListOfInts]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array value) =>
            {
                __instance.@IReadOnlyListOfInts = [.. value];
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@IReadOnlyCollectionOfInts, global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.PackedInt32Array)([.. __instance.@IReadOnlyCollectionOfInts]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.PackedInt32Array value) =>
            {
                __instance.@IReadOnlyCollectionOfInts = [.. value];
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@ArrayOfBooleans, global::Godot.VariantType.Array)
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
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@ListOfBooleans, global::Godot.VariantType.Array)
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
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@ImmutableArrayOfBooleans, global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "1/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance) =>
            {
                return (global::Godot.Collections.GodotArray<bool>)([.. __instance.@ImmutableArrayOfBooleans]);
            },
            static (NodeWithSpeciallyRecognizedMarshalling __instance, global::Godot.Collections.GodotArray<bool> value) =>
            {
                __instance.@ImmutableArrayOfBooleans = [.. value];
            });
    }
}

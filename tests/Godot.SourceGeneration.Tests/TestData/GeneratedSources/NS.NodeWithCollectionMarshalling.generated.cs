#nullable enable

namespace NS;

partial class @NodeWithCollectionMarshalling
{
    public new partial class MethodName : global::Godot.Node.MethodName
    {
        public static global::Godot.StringName @MethodWithArrayParameter { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodWithArrayParameter"u8);
        public static global::Godot.StringName @MethodThatReturnsArray { get; } = global::Godot.StringName.CreateStaticFromAscii("MethodThatReturnsArray"u8);
    }
    public new partial class ConstantName : global::Godot.Node.ConstantName
    {
    }
    public new partial class PropertyName : global::Godot.Node.PropertyName
    {
        public static global::Godot.StringName @ArrayProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("ArrayProperty"u8);
        public static global::Godot.StringName @DictionaryProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("DictionaryProperty"u8);
        public static global::Godot.StringName @UntypedArrayProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("UntypedArrayProperty"u8);
        public static global::Godot.StringName @UntypedDictionaryProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("UntypedDictionaryProperty"u8);
        public static global::Godot.StringName @PackedProperty { get; } = global::Godot.StringName.CreateStaticFromAscii("PackedProperty"u8);
    }
    public new partial class SignalName : global::Godot.Node.SignalName
    {
        public static global::Godot.StringName @CollectionSignal { get; } = global::Godot.StringName.CreateStaticFromAscii("CollectionSignal"u8);
    }
    public event CollectionSignalEventHandler @CollectionSignal
    {
        add => Connect(SignalName.@CollectionSignal, global::Godot.Callable.From<global::Godot.Collections.GodotArray<double>>(value.Invoke));
        remove => Disconnect(SignalName.@CollectionSignal, global::Godot.Callable.From<global::Godot.Collections.GodotArray<double>>(value.Invoke));
    }
    protected void EmitSignalCollectionSignal(global::Godot.Collections.GodotArray<double> @values)
    {
        EmitSignal(SignalName.@CollectionSignal, [@values]);
    }
#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)
#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.
    {
        global::System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(global::Godot.Collections.GodotArray<int>).TypeHandle);
        global::System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(global::Godot.Collections.GodotDictionary<int, string>).TypeHandle);
        global::System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(global::Godot.Collections.GodotArray<float>).TypeHandle);
        global::System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(global::Godot.Collections.GodotArray<double>).TypeHandle);
        context.BindConstructor(() => new global::NS.NodeWithCollectionMarshalling());
        context.BindMethod(MethodName.@MethodWithArrayParameter,
            new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("array"u8), global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithCollectionMarshalling __instance, global::Godot.Collections.GodotArray<int> @array) =>
            {
                __instance.@MethodWithArrayParameter(@array);
            });
        context.BindMethod(MethodName.@MethodThatReturnsArray,
            new global::Godot.Bridge.ReturnDefinition(global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "3/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithCollectionMarshalling __instance) =>
            {
                return __instance.@MethodThatReturnsArray();
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@ArrayProperty, global::Godot.VariantType.Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithCollectionMarshalling __instance) =>
            {
                return __instance.@ArrayProperty;
            },
            static (NodeWithCollectionMarshalling __instance, global::Godot.Collections.GodotArray<int> value) =>
            {
                __instance.@ArrayProperty = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@DictionaryProperty, global::Godot.VariantType.Dictionary)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:;4/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithCollectionMarshalling __instance) =>
            {
                return __instance.@DictionaryProperty;
            },
            static (NodeWithCollectionMarshalling __instance, global::Godot.Collections.GodotDictionary<int, string> value) =>
            {
                __instance.@DictionaryProperty = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@UntypedArrayProperty, global::Godot.VariantType.Array)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithCollectionMarshalling __instance) =>
            {
                return __instance.@UntypedArrayProperty;
            },
            static (NodeWithCollectionMarshalling __instance, global::Godot.Collections.GodotArray value) =>
            {
                __instance.@UntypedArrayProperty = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@UntypedDictionaryProperty, global::Godot.VariantType.Dictionary)
            {
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithCollectionMarshalling __instance) =>
            {
                return __instance.@UntypedDictionaryProperty;
            },
            static (NodeWithCollectionMarshalling __instance, global::Godot.Collections.GodotDictionary value) =>
            {
                __instance.@UntypedDictionaryProperty = value;
            });
        context.BindProperty(new global::Godot.Bridge.PropertyDefinition(PropertyName.@PackedProperty, global::Godot.VariantType.PackedInt32Array)
            {
                Hint = global::Godot.PropertyHint.TypeString,
                HintString = "2/0:",
                Usage = global::Godot.PropertyUsageFlags.Default,
            },
            static (NodeWithCollectionMarshalling __instance) =>
            {
                return __instance.@PackedProperty;
            },
            static (NodeWithCollectionMarshalling __instance, global::Godot.Collections.PackedInt32Array value) =>
            {
                __instance.@PackedProperty = value;
            });
        context.BindSignal(new global::Godot.Bridge.SignalDefinition(SignalName.@CollectionSignal)
        {
            Parameters =
            {
                new global::Godot.Bridge.ParameterDefinition(global::Godot.StringName.CreateStaticFromAscii("values"u8), global::Godot.VariantType.Array)
                {
                    Hint = global::Godot.PropertyHint.TypeString,
                    HintString = "3/0:",
                    Usage = global::Godot.PropertyUsageFlags.Default,
                },
            },
        });
    }
}

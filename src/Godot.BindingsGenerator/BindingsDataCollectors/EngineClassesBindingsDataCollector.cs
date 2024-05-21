using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;
using Godot.BindingsGenerator.Logging;
using System.Text;

namespace Godot.BindingsGenerator;

internal sealed class EngineClassesBindingsDataCollector : BindingsDataCollector
{
    private readonly Dictionary<TypeInfo, Dictionary<string, MethodInfo>> _engineClassMethodsByEngineName = [];

    // The following properties currently need to be defined with `new` to avoid warnings. We treat
    // them as a special case instead of silencing the warnings altogether, to be warned if more
    // shadowing appears.
    private readonly HashSet<string> _propertiesAllowedToShadowMember =
    [
        "ArrayMesh.BlendShapeMode",
        "Button.TextDirection",
        "Label.TextDirection",
        "LineEdit.TextDirection",
        "LinkButton.TextDirection",
        "MenuBar.TextDirection",
        "RichTextLabel.TextDirection",
        "TextEdit.TextDirection",
    ];

    public override void Initialize(BindingsData.CollectionContext context)
    {
        foreach (var engineClass in context.Api.Classes)
        {
            var type = new TypeInfo(NamingUtils.PascalToPascalCase(engineClass.Name), "Godot")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                TypeAttributes = TypeAttributes.ReferenceType,
                IsPartial = true,
            };

            if (engineClass.Description is not null || engineClass.BriefDescription is not null)
            {
                var comment = new StringBuilder();
                comment.WriteSummary(engineClass.Description, engineClass.BriefDescription);
                type.XMLComment = comment.ToString();
            }


            if (type.Name != engineClass.Name)
            {
                // Add the [GodotNativeClassName] attribute only to classes that have a different
                // name in the generated bindings than the one they use in ClassDB so that source
                // generators can retrieve the native name.
                type.Attributes.Add($"""[global::Godot.GodotNativeClassName("{engineClass.Name}")]""");
            }

            foreach (var engineEnum in engineClass.Enums)
            {
                string key = $"{engineClass.Name}.{engineEnum.Name}";
                var @enum = new EnumInfo(NamingUtils.PascalToPascalCase(engineEnum.Name))
                {
                    VisibilityAttributes = VisibilityAttributes.Public,
                    HasFlagsAttribute = engineEnum.IsBitField,
                    UnderlyingType = KnownTypes.SystemInt64,
                    ContainingType = type,
                };
                type.NestedTypes.Add(@enum);

                context.TypeDB.RegisterTypeName(key, @enum);
            }

            context.AddGeneratedType($"EngineClasses/{type.Name}.cs", type);
            context.TypeDB.RegisterTypeName(engineClass.Name, type);
        }
    }

    public override void Populate(BindingsData.CollectionContext context)
    {
        // IMPORTANT: The populate calls are separated so they can executed in the right order.
        // For example, properties must always be populated after the methods of all the classes
        // have been populated since they depend on them (property accessors use the class methods,
        // sometimes these methods are inherited so the entire inheritance chain must be populated
        // before populating properties).
        foreach (var engineClass in context.Api.Classes)
        {
            PopulateEngineClassBase(context, engineClass);
            PopulateEngineClassMethods(context, engineClass);
        }
        foreach (var engineClass in context.Api.Classes)
        {
            PopulateEngineClassProperties(context, engineClass);
            PopulateEngineClassSignals(context, engineClass);
            PopulateEngineClassEnumsAndConstants(context, engineClass);
        }

        // Generate method in InteropUtils to initialize the dictionary with helpers
        // used to access constructors and static methods for a Godot class from the
        // native class name.
        context.AddGeneratedType("NativeInterop/InteropUtils.cs", new TypeInfo("InteropUtils", "Godot.NativeInterop")
        {
            TypeAttributes = TypeAttributes.ReferenceType,
            IsPartial = true,
            IsStatic = true,
            DeclaredMethods =
            {
                new MethodInfo("EnsureHelpersInitialized")
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    Attributes =
                    {
                        "[global::System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(CreateHelpers))]",
                        "[global::System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(RegisterVirtualOverridesHelpers))]",
                    },
                    IsStatic = true,
                    Body = MethodBody.Create(writer =>
                    {
                        writer.WriteLine($"var createHelpers = new global::System.Collections.Generic.Dictionary<StringName, global::System.Func<nint, global::Godot.GodotObject>>(capacity: {context.Api.Classes.Length});");
                        writer.WriteLine($"var registerVirtualOverridesHelpers = new global::System.Collections.Generic.Dictionary<StringName, global::System.Action<global::Godot.Bridge.ClassDBRegistrationContext>>(capacity: {context.Api.Classes.Length});");

                        foreach (var engineClass in context.Api.Classes)
                        {
                            var type = context.TypeDB.GetTypeFromEngineName(engineClass.Name);

                            writer.WriteLine($"createHelpers.Add({type.FullNameWithGlobal}.NativeName, nativePtr => new {type.FullNameWithGlobal}(nativePtr));");
                            writer.WriteLine($"registerVirtualOverridesHelpers.Add({type.FullNameWithGlobal}.NativeName, {type.FullNameWithGlobal}.RegisterVirtualOverrides);");
                        }

                        writer.WriteLine("CreateHelpers = global::System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(createHelpers);");
                        writer.WriteLine("RegisterVirtualOverridesHelpers = global::System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(registerVirtualOverridesHelpers);");
                    }),
                },
            }
        });
    }

    private static void PopulateEngineClassBase(BindingsData.CollectionContext context, GodotClassInfo engineClass)
    {
        var type = context.TypeDB.GetTypeFromEngineName(engineClass.Name);
        Debug.Assert(context.IsTypeGenerated(type));

        if (!string.IsNullOrEmpty(engineClass.Inherits))
        {
            type.BaseType = context.TypeDB.GetTypeFromEngineName(engineClass.Inherits);
        }

        // Native name field.
        {
            var nativeNameField = new FieldInfo("NativeName", KnownTypes.GodotStringName)
            {
                VisibilityAttributes = VisibilityAttributes.Assembly,
                IsStatic = true,
                IsInitOnly = true,
                IsNew = engineClass.Name != "Object",
                DefaultValue = $"global::Godot.StringName.CreateStaticStringNameFromAsciiLiteral(\"{engineClass.Name}\"u8)",
            };
            type.DeclaredFields.Add(nativeNameField);
        }

        // Singleton property.
        if (context.Singletons.TryGetValue(engineClass.Name, out var singleton))
        {
            var field = new FieldInfo("_singleton", type)
            {
                VisibilityAttributes = VisibilityAttributes.Private,
                IsStatic = true,
            };
            type.DeclaredFields.Add(field);

            var property = new PropertyInfo("Singleton", type)
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsStatic = true,
                Getter = new MethodInfo("get_Singleton")
                {
                    ReturnParameter = ReturnInfo.FromType(type),
                    Body = MethodBody.CreateUnsafe(writer =>
                    {
                        writer.WriteLine("if (_singleton is null)");
                        writer.OpenBlock();
                        writer.WriteLine($"using global::Godot.NativeInterop.NativeGodotStringName nameNative = global::Godot.NativeInterop.NativeGodotStringName.Create(\"{singleton.Name}\"u8);");
                        writer.WriteLine("nint singletonPtr = (nint)global::Godot.Bridge.GodotBridge.GDExtensionInterface.global_get_singleton(nameNative.GetUnsafeAddress());");
                        writer.WriteLine($"_singleton = ({type.FullNameWithGlobal})global::Godot.NativeInterop.Marshallers.GodotObjectMarshaller.GetOrCreateManagedInstance(singletonPtr);");
                        writer.CloseBlock();
                        writer.WriteLine($"return _singleton;");
                    }),
                },
            };
            type.DeclaredProperties.Add(property);
        }

        // Populate constructors.
        {
            // These constructors are already defined in a GodotObject partial definition
            // and only need to be generated for the generated classes.
            if (engineClass.Name != "Object")
            {
                var nativePtrCtor = new ConstructorInfo()
                {
                    VisibilityAttributes = VisibilityAttributes.FamilyOrAssembly,
                    Initializer = "base(nativePtr)",
                    Parameters =
                    {
                        new ParameterInfo("nativePtr", KnownTypes.SystemIntPtr),
                    },
                };
                type.DeclaredConstructors.Add(nativePtrCtor);

                var nativeClassNameCtor = new ConstructorInfo()
                {
                    VisibilityAttributes = VisibilityAttributes.FamilyAndAssembly,
                    Initializer = "base(nativeClassName)",
                    Parameters =
                    {
                        new ParameterInfo("nativeClassName", KnownTypes.NativeGodotStringName),
                    },
                };
                type.DeclaredConstructors.Add(nativeClassNameCtor);

                var parameterlessCtor = new ConstructorInfo()
                {
                    VisibilityAttributes = engineClass.IsInstantiable
                        ? VisibilityAttributes.Public
                        : VisibilityAttributes.FamilyOrAssembly,
                    Initializer = "this(NativeName.NativeValue.DangerousSelfRef)",
                };
                type.DeclaredConstructors.Add(parameterlessCtor);
            }
        }
    }

    private void PopulateEngineClassMethods(BindingsData.CollectionContext context, GodotClassInfo engineClass)
    {
        var type = context.TypeDB.GetTypeFromEngineName(engineClass.Name);
        Debug.Assert(context.IsTypeGenerated(type));

        // Populate methods.
        var virtualMethods = new List<(MethodInfo Method, GodotMethodInfo GodotMethod)>();
        foreach (var engineMethod in engineClass.Methods)
        {
            if (!ShouldGenerateMethod(type, engineClass, engineMethod))
            {
                continue;
            }

            var method = new MethodInfo(NamingUtils.SnakeToPascalCase(engineMethod.Name))
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsStatic = engineMethod.IsStatic,
            };

            AddEngineClassMethodByEngineName(type, engineMethod.Name, method);

            if (engineMethod.Description is not null)
            {
                var comment = new StringBuilder();
                comment.WriteSummary(engineMethod.Description);
                method.XMLComment = comment.ToString();
            }

            if (engineMethod.ReturnValue is not null)
            {
                var returnType = context.TypeDB.GetTypeFromEngineName(engineMethod.ReturnValue.Type, engineMethod.ReturnValue.Meta);
                method.ReturnParameter = ReturnInfo.FromType(returnType);
            }

            if (engineMethod.IsVirtual)
            {
                method.ContractAttributes = ContractAttributes.Virtual;
                method.VisibilityAttributes = VisibilityAttributes.Family;
                virtualMethods.Add((method, engineMethod));
            }
            else
            {
                method.Body = new CallMethodBind(method, engineMethod, context.TypeDB);
            }

            foreach (var arg in engineMethod.Arguments)
            {
                string argName = NamingUtils.SnakeToCamelCase(arg.Name);
                var argType = context.TypeDB.GetTypeFromEngineName(arg.Type, arg.Meta);
                var parameter = new ParameterInfo(argName, argType);
                context.ApplyDefaultValue(arg, parameter);
                method.Parameters.Add(parameter);
            }

            if (engineMethod.IsVararg)
            {
                var parameter = new ParameterInfo("args", KnownTypes.SystemReadOnlySpanOf(KnownTypes.GodotVariant))
                {
                    ScopedKind = ScopedKind.ScopedRef,
                    DefaultValue = "default",
                };
                method.Parameters.Add(parameter);

                method.Body = new CallMethodBindVararg(method, engineMethod, context.TypeDB);
            }

            type.DeclaredMethods.Add(method);

            // Only non-virtual methods have a MethodBind to call.
            if (!engineMethod.IsVirtual)
            {
                type.DeclaredFields.Add(new FieldInfo($"_{method.Name}_MethodBind", KnownTypes.SystemVoidPtr)
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsStatic = true,
                    RequiresUnsafeCode = true,
                });
            }
        }

        // Generate RegisterVirtualOverrides method.
        {
            var registerVirtualOverridesMethod = new MethodInfo("RegisterVirtualOverrides")
            {
                VisibilityAttributes = VisibilityAttributes.Assembly,
                IsStatic = true,
                IsNew = engineClass.Name != "Object",
                Parameters =
                {
                    new ParameterInfo("context", new TypeInfo("ClassDBRegistrationContext", "Godot.Bridge")),
                },
                Body = new RegisterVirtualOverrides(type, virtualMethods),
            };

            type.DeclaredMethods.Add(registerVirtualOverridesMethod);
        }
    }

    private void PopulateEngineClassProperties(BindingsData.CollectionContext context, GodotClassInfo engineClass)
    {
        var type = context.TypeDB.GetTypeFromEngineName(engineClass.Name);
        Debug.Assert(context.IsTypeGenerated(type));

        // Populate properties.
        foreach (var engineProperty in engineClass.Properties)
        {
            // The type specified for the property in the API dump is often not what we want so we prefer
            // the type used by the getter/setter methods.
            string propertyName = NamingUtils.SnakeToPascalCase(engineProperty.Name);
            var property = new PropertyInfo(propertyName, null!)
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsNew = _propertiesAllowedToShadowMember.Contains($"{type.Name}.{propertyName}"),
            };

            if (engineProperty.Description is not null)
            {
                var comment = new StringBuilder();
                comment.WriteSummary(engineProperty.Description);
                property.XMLComment = comment.ToString();
            }

            // TODO: Some methods that are used as property accessors don't seem to be exposed in extensions_api.json
            // https://github.com/godotengine/godot/issues/64429

            if (!string.IsNullOrEmpty(engineProperty.Getter))
            {
                if (!TryGetEngineClassMethodByEngineName(type, engineProperty.Getter, out var getter))
                {
                    context.Logger.LogError($"Could not find method '{engineProperty.Getter}' in type '{type.Name}'.");
                    continue;
                }

#if DEBUG
                // Properties with index have an extra paramater in the getter.
                int expectedParameterCount = engineProperty.Index is not null ? 1 : 0;
                Debug.Assert(getter.Parameters.Count == expectedParameterCount, $"Getter method '{type.Name}.{getter.Name}' has {getter.Parameters.Count} parameters (expected: {expectedParameterCount}).");
                Debug.Assert(getter.ReturnParameter is not null, $"Getter method '{type.Name}.{getter.Name}' returns void.");
#endif

                property.Getter = new MethodInfo($"get_{property.Name}")
                {
                    ReturnParameter = ReturnInfo.FromType(getter.ReturnType!),
                    Body = MethodBody.Create(writer =>
                    {
                        if (engineProperty.Index is not null)
                        {
                            writer.WriteLine($"return {getter.Name}(({getter.Parameters[0].Type.FullNameWithGlobal})({engineProperty.Index}));");
                        }
                        else
                        {
                            writer.WriteLine($"return {getter.Name}();");
                        }
                    }),
                };

                // Hide property accessors.
                // Properties with index may reuse the same getter method multiple times,
                // so we need to make sure we haven't already added the attribute.
                if (!getter.Attributes.Any(attr => attr.Contains("global::System.ComponentModel.EditorBrowsableAttribute")))
                {
                    getter.Attributes.Add("[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]");
                }
            }

            if (!string.IsNullOrEmpty(engineProperty.Setter))
            {
                if (!TryGetEngineClassMethodByEngineName(type, engineProperty.Setter, out var setter))
                {
                    context.Logger.LogError($"Could not find method '{engineProperty.Setter}' in type '{type.Name}'.");
                    continue;
                }

#if DEBUG
                // Properties with index have an extra paramater in the setter.
                int expectedParameterCount = engineProperty.Index is not null ? 2 : 1;
                Debug.Assert(setter.Parameters.Count == expectedParameterCount, $"Setter method '{type.Name}.{setter.Name}' has {setter.Parameters.Count} parameters (expected: {expectedParameterCount}).");
#endif

                var setterParameterIndex = engineProperty.Index is not null ? 1 : 0;
                property.Setter = new MethodInfo($"set_{property.Name}")
                {
                    Parameters =
                    {
                        new ParameterInfo("value", setter.Parameters[setterParameterIndex].Type)
                    },
                    Body = MethodBody.Create(writer =>
                    {
                        if (engineProperty.Index is not null)
                        {
                            writer.WriteLine($"{setter.Name}(({setter.Parameters[0].Type.FullNameWithGlobal})({engineProperty.Index}), value);");
                        }
                        else
                        {
                            writer.WriteLine($"{setter.Name}(value);");
                        }
                    }),
                };

                // Hide property accessors.
                // Properties with index may reuse the same setter method multiple times,
                // so we need to make sure we haven't already added the attribute.
                if (!setter.Attributes.Any(attr => attr.Contains("global::System.ComponentModel.EditorBrowsableAttribute")))
                {
                    setter.Attributes.Add("[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]");
                }
            }

            Debug.Assert(property.Getter is not null || property.Setter is not null);

            if (property.Getter is not null && property.Setter is not null)
            {
                if (type.Name == "Node" && property.Name == "Name")
                {
                    // HARDCODED: Ignore the type mismatch in the Node.Name property.
                    // TODO: The old bindings_generator had a special case for this property, but ideally it should be fixed.
                    // https://github.com/godotengine/godot/pull/76560
                    property.Setter.Body = MethodBody.Create(writer => writer.WriteLine("SetName(value?.ToString());"));
                }
                else
                {
#if DEBUG
                    var getterReturnType = property.Getter.ReturnType;
                    var setterParameterType = property.Setter.Parameters[0].Type;

                    if (getterReturnType != setterParameterType)
                    {
                        // The types don't match, check if the types are T[] and ReadOnlySpan<T>
                        // which is a special case that is allowed.
                        if (!CheckArrayAndSpanMatch(getterReturnType, setterParameterType))
                        {
                            Debug.Fail($"Property '{type.Name}.{property.Name}' getter has a return type that does not match the setter parameter type.");
                        }
                    }

                    static bool CheckArrayAndSpanMatch(TypeInfo? getterReturnType, TypeInfo setterParameterType)
                    {
                        if (getterReturnType?.GenericTypeDefinition == KnownTypes.SystemArray
                            && setterParameterType.GenericTypeDefinition == KnownTypes.SystemReadOnlySpan)
                        {
                            // Check that the T types match.
                            if (getterReturnType.GenericTypeArguments[0] == setterParameterType.GenericTypeArguments[0])
                            {
                                return true;
                            }
                        }

                        return false;
                    }
#endif
                }
            }

            // Use the getter/setter type as the property type.
            property.Type = property.Getter?.ReturnType ?? property.Setter?.Parameters[0].Type ?? property.Type;
            Debug.Assert(property.Type is not null);
            Debug.Assert(property.Type.GenericTypeDefinition != KnownTypes.SystemSpan);
            Debug.Assert(property.Type.GenericTypeDefinition != KnownTypes.SystemReadOnlySpan);

            type.DeclaredProperties.Add(property);
        }
    }

    private static void PopulateEngineClassSignals(BindingsData.CollectionContext context, GodotClassInfo engineClass)
    {
        var type = context.TypeDB.GetTypeFromEngineName(engineClass.Name);
        Debug.Assert(context.IsTypeGenerated(type));

        // Populate signals.
        var actionType = new TypeInfo("Action", "System");
        foreach (var engineSignal in engineClass.Signals)
        {
            string eventName = NamingUtils.SnakeToPascalCase(engineSignal.Name);

            TypeInfo eventHandlerType;

            if (engineSignal.Arguments.Length != 0)
            {
                var @delegate = new DelegateInfo($"{NamingUtils.SnakeToPascalCase(engineSignal.Name)}EventHandler")
                {
                    VisibilityAttributes = VisibilityAttributes.Public,
                    ContainingType = type,
                };

                foreach (var arg in engineSignal.Arguments)
                {
                    string argName = NamingUtils.SnakeToCamelCase(arg.Name);
                    var argType = context.TypeDB.GetTypeFromEngineName(arg.Type, arg.Meta);
                    var parameter = new ParameterInfo(argName, argType);
                    @delegate.Parameters.Add(parameter);
                }

                type.NestedTypes.Add(@delegate);
                eventHandlerType = @delegate;

                // Add trampoline so we can create the Callable in the event's add/remove accessors.
                var eventTrampoline = new MethodInfo($"{eventName}Trampoline")
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsStatic = true,
                    Parameters =
                    {
                        new ParameterInfo("delegate", KnownTypes.SystemObject),
                        new ParameterInfo("args", KnownTypes.NativeGodotVariantPtrSpan),
                        new ParameterInfo("ret", KnownTypes.NativeGodotVariant)
                        {
                            RefKind = RefKind.Out,
                        },
                    },
                    Body = MethodBody.Create(writer =>
                    {
                        writer.WriteLine($"(({eventHandlerType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        for (int i = 0; i < @delegate.Parameters.Count; i++)
                        {
                            var parameter = @delegate.Parameters[i];
                            writer.Write($"global::Godot.NativeInterop.Marshalling.ConvertFromVariant<{parameter.Type.FullNameWithGlobal}>(args[{i}])");
                            if (i < @delegate.Parameters.Count - 1)
                            {
                                writer.WriteLine(',');
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                        writer.WriteLine("ret = default;");
                    }),
                };
                type.DeclaredMethods.Add(eventTrampoline);
            }
            else
            {
                eventHandlerType = actionType;
            }

            var @event = new EventInfo(eventName, eventHandlerType)
            {
                VisibilityAttributes = VisibilityAttributes.Public,
            };


            if (engineSignal.Description is not null)
            {
                var comment = new StringBuilder();
                comment.WriteSummary(engineSignal.Description);
                @event.XMLComment = comment.ToString();
            }

            @event.AddAccessor = new MethodInfo($"add_{@event.Name}")
            {
                Parameters =
                {
                    new ParameterInfo("value", @event.EventHandlerType)
                },
                Body = MethodBody.CreateUnsafe(writer =>
                {
                    if (engineSignal.Arguments.Length != 0)
                    {
                        writer.WriteLine($"Connect(SignalName.{@event.Name}, Callable.CreateWithUnsafeTrampoline(value, &{@event.Name}Trampoline));");
                    }
                    else
                    {
                        writer.WriteLine($"Connect(SignalName.{@event.Name}, Callable.From(value));");
                    }
                }),
            };
            @event.RemoveAccessor = new MethodInfo($"remove_{@event.Name}")
            {
                Parameters =
                {
                    new ParameterInfo("value", @event.EventHandlerType)
                },
                Body = MethodBody.CreateUnsafe(writer =>
                {
                    if (engineSignal.Arguments.Length != 0)
                    {
                        writer.WriteLine($"Disconnect(SignalName.{@event.Name}, Callable.CreateWithUnsafeTrampoline(value, &{@event.Name}Trampoline));");
                    }
                    else
                    {
                        writer.WriteLine($"Disconnect(SignalName.{@event.Name}, Callable.From(value));");
                    }
                }),
            };
            type.DeclaredEvents.Add(@event);
        }
    }

    private static void PopulateEngineClassEnumsAndConstants(BindingsData.CollectionContext context, GodotClassInfo engineClass)
    {
        var type = context.TypeDB.GetTypeFromEngineName(engineClass.Name);
        Debug.Assert(context.IsTypeGenerated(type));

        // Populate nested enums.
        foreach (var engineEnum in engineClass.Enums)
        {
            string key = $"{engineClass.Name}.{engineEnum.Name}";
            var enumType = context.TypeDB.GetTypeFromEngineName(key);

            if (enumType is not EnumInfo @enum)
            {
                context.Logger.LogError($"Type found for '{enumType.Name}' is not an enum.");
                continue;
            }

            foreach (var (name, value) in engineEnum.Values)
            {
                /// Todo add comment to enum
                @enum.Values.Add((NamingUtils.SnakeToPascalCase(name), value, null));
            }

            int enumPrefix = NamingUtils.DetermineEnumPrefix(engineEnum);

            // HARDCODED: The Error enum have the prefix 'ERR_' for everything except 'OK' and 'FAILED'.
            if (type.ContainingType is null && type.Name == "Error")
            {
                if (enumPrefix > 0)
                {
                    // Just in case it ever changes.
                    context.Logger.LogError($"Prefix for enum 'Error' is not empty.");
                }

                enumPrefix = 1; // 'ERR_'
            }

            NamingUtils.ApplyPrefixToEnumConstants(engineEnum, @enum, enumPrefix);
            NamingUtils.RemoveMaxConstant(engineEnum, @enum);
        }

        // Populate constants.
        foreach (var engineConstant in engineClass.Constants)
        {
            var constant = new FieldInfo(NamingUtils.SnakeToPascalCase(engineConstant.Name), context.TypeDB.GetTypeFromEngineName(engineConstant.Type))
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsLiteral = true,
                DefaultValue = engineConstant.Value,
            };

            if (engineConstant.Description is not null)
            {
                var comment = new StringBuilder();
                comment.WriteSummary(engineConstant.Description);
                constant.XMLComment = comment.ToString();
            }

            type.DeclaredFields.Add(constant);
        }

        // Check and fix member conflicts.
        {
            var usedNames = new HashSet<string>();

            // First collect all the names of properties, fields, members, events, and nested types
            // since they should not have conflicts. Constant fields are skipped in this step.
            foreach (var property in type.DeclaredProperties)
            {
                AddMemberNameOrThrow(usedNames, property);
            }
            foreach (var field in type.DeclaredFields.Where(f => !f.IsLiteral))
            {
                AddMemberNameOrThrow(usedNames, field);
            }
            foreach (var method in type.DeclaredMethods)
            {
                AddMemberNameOrThrow(usedNames, method);
            }
            foreach (var @event in type.DeclaredEvents)
            {
                AddMemberNameOrThrow(usedNames, @event);
            }
            foreach (var nestedType in type.NestedTypes.Where(t => !t.IsEnum))
            {
                AddMemberNameOrThrow(usedNames, nestedType);
            }

            // Then check enums and constants for conflicts and rename them if needed.
            foreach (var @enum in type.NestedTypes.Where(t => t.IsEnum))
            {
                AddMemberNameOrRename(usedNames, @enum, "Enum");
            }
            foreach (var constant in type.DeclaredFields.Where(f => f.IsLiteral))
            {
                AddMemberNameOrRename(usedNames, constant, "Constant");
            }

            static void AddMemberNameOrThrow(HashSet<string> usedNames, MemberInfo member)
            {
                if (!usedNames.Add(member.Name))
                {
                    throw new InvalidOperationException($"{member.GetType()} member with name '{member.Name}' conflicts with an existing member.");
                }
            }

            void AddMemberNameOrRename(HashSet<string> usedNames, MemberInfo member, string suffix)
            {
                if (usedNames.Contains(member.Name))
                {
                    context.Logger.LogWarning($"Found member with name '{member.Name}' that conflicts with an existing member, adding '{suffix}' suffix.");
                    member.Name += suffix;
                }
                AddMemberNameOrThrow(usedNames, member);
            }
        }

        // Add cached StringNames.
        {
            if (engineClass.Name == "Object" || engineClass.Methods.Length > 0)
            {
                var methodNamesType = new TypeInfo("MethodName")
                {
                    TypeAttributes = TypeAttributes.ReferenceType,
                    VisibilityAttributes = VisibilityAttributes.Public,
                    IsNew = engineClass.Name != "Object",
                    BaseType = engineClass.Name != "Object"
                        ? new TypeInfo("MethodName") { ContainingType = type.BaseType }
                        : null,
                };

                foreach (var engineMethod in engineClass.Methods)
                {
                    if (!ShouldGenerateMethod(type, engineClass, engineMethod))
                    {
                        continue;
                    }

                    AddCachedStringName(methodNamesType, engineMethod.Name);
                }

                type.NestedTypes.Add(methodNamesType);
            }

            if (engineClass.Name == "Object" || engineClass.Properties.Length > 0)
            {
                var propertyNamesType = new TypeInfo("PropertyName")
                {
                    TypeAttributes = TypeAttributes.ReferenceType,
                    VisibilityAttributes = VisibilityAttributes.Public,
                    IsNew = engineClass.Name != "Object",
                    BaseType = engineClass.Name != "Object"
                        ? new TypeInfo("PropertyName") { ContainingType = type.BaseType }
                        : null,
                };

                foreach (var engineProperty in engineClass.Properties)
                {
                    AddCachedStringName(propertyNamesType, engineProperty.Name);
                }

                type.NestedTypes.Add(propertyNamesType);
            }

            if (engineClass.Name == "Object" || engineClass.Signals.Length > 0)
            {
                var signalNamesType = new TypeInfo("SignalName")
                {
                    TypeAttributes = TypeAttributes.ReferenceType,
                    VisibilityAttributes = VisibilityAttributes.Public,
                    IsNew = engineClass.Name != "Object",
                    BaseType = engineClass.Name != "Object"
                        ? new TypeInfo("SignalName") { ContainingType = type.BaseType }
                        : null,
                };

                foreach (var engineSignal in engineClass.Signals)
                {
                    AddCachedStringName(signalNamesType, engineSignal.Name);
                }

                type.NestedTypes.Add(signalNamesType);
            }

            static void AddCachedStringName(TypeInfo cacheType, string engineName)
            {
                string name = NamingUtils.SnakeToPascalCase(engineName);

                var nameField = new FieldInfo($"_{engineName}_Value", KnownTypes.GodotStringName)
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsStatic = true,
                    IsInitOnly = true,
                    DefaultValue = $"global::Godot.StringName.CreateStaticStringNameFromAsciiLiteral(\"{engineName}\"u8)",
                };
                cacheType.DeclaredFields.Add(nameField);

                var nameProperty = new PropertyInfo(name, KnownTypes.GodotStringName)
                {
                    VisibilityAttributes = VisibilityAttributes.Public,
                    IsStatic = true,
                    Getter = new MethodInfo($"get_{name}")
                    {
                        ReturnParameter = ReturnInfo.FromType(KnownTypes.GodotStringName),
                        Body = MethodBody.Create(writer =>
                        {
                            writer.WriteLine($"return _{engineName}_Value;");
                        }),
                    },
                };
                cacheType.DeclaredProperties.Add(nameProperty);
            }
        }
    }

    private void AddEngineClassMethodByEngineName(TypeInfo type, string engineMethodName, MethodInfo method)
    {
        if (!_engineClassMethodsByEngineName.TryGetValue(type, out var methods))
        {
            _engineClassMethodsByEngineName[type] = methods = [];
        }
        methods[engineMethodName] = method;
    }

    private bool TryGetEngineClassMethodByEngineName(TypeInfo type, string engineMethodName, [NotNullWhen(true)] out MethodInfo? method)
    {
        if (_engineClassMethodsByEngineName.TryGetValue(type, out var methods))
        {
            if (methods.TryGetValue(engineMethodName, out method))
            {
                return true;
            }
        }

        if (type.BaseType is not null)
        {
            return TryGetEngineClassMethodByEngineName(type.BaseType, engineMethodName, out method);
        }

        method = null;
        return false;
    }

    private static bool ShouldGenerateMethod(TypeInfo type, GodotClassInfo engineClass, GodotMethodInfo engineMethod)
    {
        if (engineClass.Name == "Object")
        {
            if (engineMethod.Name == "to_string")
            {
                // Skipping GodotObject.ToString since we rather implement it manually.
                return false;
            }
            if (engineMethod.Name == "get_instance_id")
            {
                // Skipping GodotObject.GetInstanceId, we'll use the GDExtensionInterface method instead.
                return false;
            }
        }

        return true;
    }
}

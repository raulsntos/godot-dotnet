using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;
using Godot.Common;

namespace Godot.BindingsGenerator;

internal sealed class BuiltInClassesBindingsDataCollector : BindingsDataCollector
{
    // Stores the generated built-in classes by their engine name.
    private readonly Dictionary<string, TypeInfo> _builtinClasses = [];

    public override void Initialize(BindingsData.CollectionContext context)
    {
        foreach (var engineClass in context.Api.BuiltInClasses)
        {
            if (!ShouldGenerateBuiltInClass(engineClass.Name) && !ShouldGenerateBuiltInClassHelpers(engineClass.Name))
            {
                continue;
            }

            var type = new TypeInfo($"NativeGodot{NamingUtils.PascalToPascalCase(engineClass.Name)}", "Godot.NativeInterop")
            {
                VisibilityAttributes = VisibilityAttributes.Assembly,
                TypeAttributes = TypeAttributes.ByRefLikeType,
                IsPartial = true,
            };

            if (ShouldGenerateBuiltInClassHelpers(engineClass.Name))
            {
                // We're only generating helper methods for this class, so we'll make it a static class.
                type.TypeAttributes = TypeAttributes.ReferenceType;
                type.IsStatic = true;
            }

            context.AddGeneratedType($"BuiltInClasses/{type.Name}.cs", type);
            RegisterBuiltInType(engineClass.Name, type);
        }

        // Add Variant interop struct.
        {
            var type = KnownTypes.NativeGodotVariant;
            type.VisibilityAttributes = VisibilityAttributes.Assembly;
            type.TypeAttributes = TypeAttributes.ByRefLikeType;
            type.IsPartial = true;

            context.AddGeneratedType($"BuiltInClasses/{type.Name}.cs", type);
            RegisterBuiltInType("Variant", type);
        }

        void RegisterBuiltInType(string engineTypeName, TypeInfo type)
        {
            _builtinClasses.Add(engineTypeName, type);
        }
    }

    public override void Populate(BindingsData.CollectionContext context)
    {
        foreach (var engineClass in context.Api.BuiltInClasses)
        {
            if (!_builtinClasses.TryGetValue(engineClass.Name, out var type))
            {
                continue;
            }
            Debug.Assert(context.IsTypeGenerated(type));

            if (!TryGetVariantType(engineClass.Name, out string? variantTypeName))
            {
                throw new InvalidOperationException($"Could not find enum field in 'GDExtensionVariantType' for Variant type '{engineClass.Name}'.");
            }

            foreach (var engineConstructor in engineClass.Constructors)
            {
                if (!ShouldGenerateBuiltInClass(engineClass.Name))
                {
                    // Only generate the constructors for the built-in classes that
                    // we generate a native interop struct for.
                    break;
                }

                var method = new MethodInfo("Create")
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsStatic = true,
                    ReturnParameter = ReturnInfo.FromType(type),
                    Body = new CallBuiltInConstructor(variantTypeName, engineConstructor.Index, context.TypeDB),
                };

                foreach (var arg in engineConstructor.Arguments)
                {
                    string argName = NamingUtils.SnakeToCamelCase(arg.Name);
                    var argType = context.TypeDB.GetTypeFromEngineName(arg.Type, arg.Meta);
                    var argTypeUnmanaged = context.TypeDB.GetUnmanagedType(argType);
                    var parameter = new ParameterInfo(argName, argTypeUnmanaged)
                    {
                        ScopedKind = argTypeUnmanaged.IsByRefLike
                            ? ScopedKind.ScopedRef
                            : ScopedKind.None,
                    };
                    method.Parameters.Add(parameter);
                }

                type.DeclaredMethods.Add(method);

                type.DeclaredFields.Add(new FieldInfo($"_constructor{engineConstructor.Index}", new TypeInfo("delegate* unmanaged[Cdecl]<void*, void**, void>"))
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsStatic = true,
                    RequiresUnsafeCode = true,
                });
            }

            if (engineClass.HasDestructor)
            {
                Debug.Assert(!ShouldGenerateBuiltInClassHelpers(engineClass.Name), $"Built-in classes that are generated as static helpers must not have destructors. The built-in class was '{engineClass.Name}'.");

                var method = new MethodInfo("Destroy")
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsStatic = true,
                    Body = new CallBuiltInDestructor(variantTypeName),
                };

                // Add self parameter.
                {
                    var selfParameter = new ParameterInfo("self", type)
                    {
                        RefKind = RefKind.Ref,
                        ScopedKind = ScopedKind.ScopedRef,
                    };
                    method.Parameters.Add(selfParameter);
                }
                type.DeclaredMethods.Add(method);

                // If the type has a destructor, implement IDisposable.
                var disposeMethod = new MethodInfo("Dispose")
                {
                    VisibilityAttributes = VisibilityAttributes.Public,
                    Body = MethodBody.Create(writer =>
                    {
                        // To check if the type should be destroyed we use the IsAllocated property
                        // which must be implemented for all types that have a destructor.
                        // The IsAllocated property is automatically generated for Packed Arrays because
                        // they can all use the same implementation, for other types with a destructor
                        // it must be implemented manually.
                        writer.WriteLine("if (!IsAllocated)");
                        writer.OpenBlock();
                        writer.WriteLine("return;");
                        writer.CloseBlock();
                        writer.WriteLine("Destroy(ref this);");
                    }),
                };
                type.DeclaredMethods.Add(disposeMethod);

                type.DeclaredFields.Add(new FieldInfo("_destructor", new TypeInfo("delegate* unmanaged[Cdecl]<void*, void>"))
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsStatic = true,
                    RequiresUnsafeCode = true,
                });
            }

            // Generate packed array ptrw.
            if (TypeDB.IsTypePackedArray(engineClass.Name))
            {
                Debug.Assert(!string.IsNullOrEmpty(engineClass.IndexingReturnType), "Packed arrays must have an indexing return type.");

                // Some packed arrays must be special-cased because the extension_api.json
                // does not contain metadata for the indexing return type, or any way of
                // knowing what the real indexer type is.
                TypeInfo indexerType = engineClass.Name switch
                {
                    "PackedByteArray" => KnownTypes.SystemByte,
                    "PackedInt32Array" => KnownTypes.SystemInt32,
                    "PackedFloat32Array" => KnownTypes.SystemSingle,
                    _ => context.TypeDB.GetTypeFromEngineName(engineClass.IndexingReturnType),
                };
                TypeInfo indexerTypeUnmanaged = engineClass.Name switch
                {
                    "PackedByteArray" => KnownTypes.SystemByte,
                    "PackedInt32Array" => KnownTypes.SystemInt32,
                    "PackedFloat32Array" => KnownTypes.SystemSingle,
                    _ => context.TypeDB.GetUnmanagedType(indexerType),
                };

                var vectorField = new FieldInfo("_vector", KnownTypes.NativeGodotVectorOf(indexerTypeUnmanaged))
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    Attributes = { "[global::System.Runtime.InteropServices.FieldOffset(0)]" },
                    RequiresUnsafeCode = true,
                };
                type.DeclaredFields.Add(vectorField);

                var isAllocatedProperty = new PropertyInfo("IsAllocated", KnownTypes.SystemBoolean)
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsReadOnly = true,
                    Getter = new MethodInfo("get_IsAllocated")
                    {
                        Attributes = { "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]" },
                        ReturnParameter = ReturnInfo.FromType(KnownTypes.SystemBoolean),
                        Body = MethodBody.CreateUnsafe(writer =>
                        {
                            writer.WriteLine("return _vector.GetPtrw() is not null;");
                        }),
                    },
                };
                type.DeclaredProperties.Add(isAllocatedProperty);

                var sizeProperty = new PropertyInfo("Size", KnownTypes.SystemInt32)
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsReadOnly = true,
                    Getter = new MethodInfo("get_Size")
                    {
                        Attributes = { "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]" },
                        ReturnParameter = ReturnInfo.FromType(KnownTypes.SystemInt32),
                        Body = MethodBody.CreateUnsafe(writer =>
                        {
                            writer.WriteLine("return _vector.Size;");
                        }),
                    },
                };
                type.DeclaredProperties.Add(sizeProperty);

                var getPtrwMethod = new MethodInfo("GetPtrw")
                {
                    Attributes = { "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]" },
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsReadOnly = true,
                    ReturnParameter = ReturnInfo.FromType(indexerTypeUnmanaged.MakePointerType()),
                    Body = MethodBody.CreateUnsafe(writer =>
                    {
                        writer.WriteLine($"return _vector.GetPtrw();");
                    }),
                };
                type.DeclaredMethods.Add(getPtrwMethod);

                // Skip the AsSpan method for PackedStringArray because the generic T in Span<T> would need to be
                // NativeGodotString which is a ref struct and Span<T> doesn't support it.
                if (!indexerTypeUnmanaged.IsByRefLike)
                {
                    var spanType = KnownTypes.SystemSpanOf(indexerTypeUnmanaged);
                    var readonlySpanType = KnownTypes.SystemReadOnlySpanOf(indexerTypeUnmanaged);

                    var asSpanMethod = new MethodInfo("AsSpan")
                    {
                        VisibilityAttributes = VisibilityAttributes.Assembly,
                        IsReadOnly = true,
                        ReturnParameter = ReturnInfo.FromType(spanType),
                        Body = MethodBody.CreateUnsafe(writer =>
                        {
                            writer.WriteLine($"return new {spanType.FullNameWithGlobal}(_vector.GetPtrw(), Size);");
                        }),
                    };
                    type.DeclaredMethods.Add(asSpanMethod);

                    // Also add a Create overload to create the packed array from the Span type.
                    var createMethod = new MethodInfo("Create")
                    {
                        Attributes = { "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]" },
                        VisibilityAttributes = VisibilityAttributes.Assembly,
                        IsStatic = true,
                        Parameters = {
                            new ParameterInfo("value", readonlySpanType)
                            {
                                ScopedKind = ScopedKind.ScopedRef,
                            },
                        },
                        ReturnParameter = ReturnInfo.FromType(type),
                        Body = MethodBody.Create(writer =>
                        {
                            // Fast path if the packed array is empty.
                            writer.WriteLine("if (value.IsEmpty)");
                            writer.OpenBlock();
                            writer.WriteLine("return default;");
                            writer.CloseBlock();

                            writer.WriteLine($"{type.FullNameWithGlobal} destination = default;");
                            writer.WriteLine($"{type.FullNameWithGlobal}.Resize(ref destination, value.Length);");
                            writer.WriteLine("value.CopyTo(destination.AsSpan());");
                            writer.WriteLine("return destination;");
                        }),
                    };
                    type.DeclaredMethods.Add(createMethod);
                }
            }

            foreach (var engineMethod in engineClass.Methods)
            {
                // For StringNames we won't generate any of these methods because they're not "real".
                // These methods just implicitly convert to String and reuse the String methods,
                // but in C# we prefer to convert to String explicitly so we'll never use these methods.
                if (engineClass.Name == "StringName")
                {
                    break;
                }

                // These methods won't be exposed, we prefer to expose them manually
                // in separate public types allowing us to choose which ones make sense
                // to expose or reimplementing them to be more performant.
                var method = new MethodInfo(NamingUtils.SnakeToPascalCase(engineMethod.Name))
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsStatic = true,
                };

                // Hardcode renames to avoid conflicts with manually defined members.
                method.Name = method.Name switch
                {
                    "Size" => "GetSize",
                    "IsEmpty" => "GetIsEmpty",
                    "IsReadOnly" => "GetIsReadOnly",
                    _ => method.Name,
                };

                method.Body = new CallBuiltInMethod(variantTypeName, method, engineMethod, context.TypeDB);

                // We generate every method as static, so if the method needs an instance add a ref parameter.
                if (!engineMethod.IsStatic)
                {
                    var selfParameter = new ParameterInfo("self", type)
                    {
                        RefKind = engineMethod.IsConst
                            ? RefKind.RefReadOnly
                            : RefKind.Ref,
                        ScopedKind = ScopedKind.ScopedRef,
                    };
                    if (ShouldGenerateBuiltInClassHelpers(engineClass.Name))
                    {
                        selfParameter.Type = context.TypeDB.GetTypeFromEngineName(engineClass.Name);
                        selfParameter.RefKind = RefKind.None;
                        selfParameter.ScopedKind = ScopedKind.None;
                        Debug.Assert(selfParameter.Type.IsValueType, $"Built-in classes that are generated as static helpers must have a registered unmanaged type that can be used as the 'self' parameter. The built-in class was '{engineClass.Name}'.");
                    }
                    method.Parameters.Add(selfParameter);
                }

                if (!string.IsNullOrEmpty(engineMethod.ReturnType))
                {
                    var returnType = context.TypeDB.GetTypeFromEngineName(engineMethod.ReturnType);
                    var returnTypeUnmanaged = context.TypeDB.GetUnmanagedType(returnType);
                    method.ReturnParameter = ReturnInfo.FromType(returnTypeUnmanaged);
                }

                foreach (var arg in engineMethod.Arguments)
                {
                    string argName = NamingUtils.SnakeToCamelCase(arg.Name);
                    var argType = context.TypeDB.GetTypeFromEngineName(arg.Type, arg.Meta);
                    var argTypeUnmanaged = context.TypeDB.GetUnmanagedType(argType);
                    var parameter = new ParameterInfo(argName, argTypeUnmanaged)
                    {
                        ScopedKind = argTypeUnmanaged.IsByRefLike
                            ? ScopedKind.ScopedRef
                            : ScopedKind.None,
                    };
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

                    method.Body = new CallBuiltInMethodVararg(variantTypeName, method, engineMethod, context.TypeDB);
                }

                type.DeclaredMethods.Add(method);

                type.DeclaredFields.Add(new FieldInfo($"_{method.Name}_MethodBind", new TypeInfo("delegate* unmanaged[Cdecl]<void*, void**, void*, int, void>"))
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsStatic = true,
                    RequiresUnsafeCode = true,
                });
            }

            foreach (var engineOperator in engineClass.Operators)
            {
                if (ShouldGenerateBuiltInClassHelpers(engineClass.Name))
                {
                    // Only generate the operator methods for the built-in classes that
                    // we generate a native interop struct for.
                    break;
                }

                if (!TryGetOperatorName(engineOperator.Name, out string? operatorName))
                {
                    Debug.Fail($"Operator '{engineOperator.Name}' doesn't have a method name.");
                    continue;
                }
                if (!TryGetOperatorKind(engineOperator.Name, out string? operatorKind))
                {
                    throw new InvalidOperationException($"Could not find enum field in 'GDExtensionVariantOperator' for operator '{engineOperator.Name}'.");
                }

                var method = new MethodInfo(operatorName)
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsStatic = true,
                };

                var valueParameter = new ParameterInfo("value", type)
                {
                    RefKind = RefKind.In,
                    ScopedKind = ScopedKind.ScopedRef,
                };
                method.Parameters.Add(valueParameter);

                string? otherVariantTypeName = "GDEXTENSION_VARIANT_TYPE_NIL";
                if (!string.IsNullOrEmpty(engineOperator.RightType))
                {
                    // Change the first parameter's name to 'left', since there are two parameters
                    // and the convention is to name them 'left' and 'right'.
                    valueParameter.Name = "left";

                    var otherType = context.TypeDB.GetTypeFromEngineName(engineOperator.RightType);
                    var otherTypeUnmanaged = context.TypeDB.GetUnmanagedType(otherType);
                    var otherParameter = new ParameterInfo("right", otherTypeUnmanaged)
                    {
                        RefKind = otherTypeUnmanaged.IsByRefLike
                            ? RefKind.In
                            : RefKind.None,
                        ScopedKind = otherTypeUnmanaged.IsByRefLike
                            ? ScopedKind.ScopedRef
                            : ScopedKind.None,
                    };
                    method.Parameters.Add(otherParameter);

                    if (engineOperator.RightType == "Variant")
                    {
                        otherVariantTypeName = "GDEXTENSION_VARIANT_TYPE_NIL";
                    }
                    else if (!TryGetVariantType(engineOperator.RightType, out otherVariantTypeName))
                    {
                        throw new InvalidOperationException($"Could not find enum field in 'GDExtensionVariantType' for Variant type '{engineOperator.RightType}'.");
                    }
                }

                method.Body = new CallBuiltInOperator(operatorKind, variantTypeName, otherVariantTypeName, method, engineOperator, context.TypeDB);

                var returnType = context.TypeDB.GetTypeFromEngineName(engineOperator.ReturnType);
                var returnTypeUnmanaged = context.TypeDB.GetUnmanagedType(returnType);
                method.ReturnParameter = ReturnInfo.FromType(returnTypeUnmanaged);

                type.DeclaredMethods.Add(method);

                type.DeclaredFields.Add(new FieldInfo($"_{method.Name}_{engineOperator.RightType}_OperatorEvaluator", new TypeInfo("delegate* unmanaged[Cdecl]<void*, void*, void*, void>"))
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsStatic = true,
                    RequiresUnsafeCode = true,
                });
            }

            // Color constants.
            if (engineClass.Name == "Color")
            {
                var colorsType = new TypeInfo("NamedColors", "Godot")
                {
                    VisibilityAttributes = VisibilityAttributes.Public,
                    TypeAttributes = TypeAttributes.ReferenceType,
                    IsStatic = true,
                };
                context.AddGeneratedType($"BuiltInClasses/{colorsType.Name}.cs", colorsType);

                var dictionary = new FieldInfo("ByName", KnownTypes.SystemFrozenDictionaryOf(KnownTypes.SystemString, KnownTypes.GodotColor))
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsStatic = true,
                    IsInitOnly = true,
                };
                colorsType.DeclaredFields.Add(dictionary);

                var dictionaryType = KnownTypes.SystemDictionaryOf(KnownTypes.SystemString, KnownTypes.GodotColor);

                var dictionaryInitializer = new StringBuilder();
                dictionaryInitializer.Append("global::System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(");
                dictionaryInitializer.AppendLine(CultureInfo.InvariantCulture, $"new {dictionaryType.FullName}(capacity: {engineClass.Constants.Length}, global::System.StringComparer.OrdinalIgnoreCase)");
                dictionaryInitializer.AppendLine("    {");

                foreach (var engineConstant in engineClass.Constants)
                {
                    string constantName = NamingUtils.SnakeToPascalCase(engineConstant.Name);
                    var constantType = context.TypeDB.GetTypeFromEngineName(engineConstant.Type);
                    Debug.Assert(constantType == KnownTypes.GodotColor);

                    var constant = new PropertyInfo(constantName, constantType)
                    {
                        VisibilityAttributes = VisibilityAttributes.Public,
                        IsStatic = true,
                        Getter = new MethodInfo($"get_{constantName}")
                        {
                            ReturnParameter = ReturnInfo.FromType(constantType),
                            Body = MethodBody.Create(writer =>
                            {
                                string expression = context.TypeDB.GetDefaultValueExpression(constantType, engineConstant.Value);
                                writer.WriteLine($"return {expression};");
                            }),
                        },
                    };
                    colorsType.DeclaredProperties.Add(constant);

                    string key = engineConstant.Name.Replace("_", "");

                    dictionaryInitializer.Append("        ");
                    dictionaryInitializer.AppendLine(CultureInfo.InvariantCulture, $$"""{ "{{key}}", {{constantName}} },""");
                }

                dictionaryInitializer.Append("    }, global::System.StringComparer.OrdinalIgnoreCase)");
                dictionary.DefaultValue = dictionaryInitializer.ToString();
            }
        }

        // Populate Variant interop struct.
        {
            var type = KnownTypes.NativeGodotVariant;

            // Converters from/to Variant.
            foreach (var engineClass in context.Api.BuiltInClasses
                // Object is not one of the built-in classes, but we should also generate
                // conversions in the Variant interop struct.
                .Append(new GodotBuiltInClassInfo() { Name = "Object" }))
            {
                if (engineClass.Name == "Nil")
                {
                    // Ignore "Nil" type.
                    continue;
                }

                string targetTypeName = engineClass.Name switch
                {
                    // Avoid hardcoded rename to GodotObject.
                    "Object" => "Object",
                    _ => NamingUtils.PascalToPascalCase(engineClass.Name),
                };
                var targetType = context.TypeDB.GetTypeFromEngineName(engineClass.Name);
                var targetTypeUnmanaged = context.TypeDB.GetUnmanagedType(targetType);

                if (!TryGetVariantType(engineClass.Name, out string? variantTypeName))
                {
                    // Could not find the type in the enum.
                    Debug.Fail($"All the built-in types should exist in the enum, but '{engineClass.Name}' was not found.");
                    continue;
                }

                var variantToTypeConstructorField = new FieldInfo($"_variantTo{targetTypeName}Constructor", new TypeInfo("delegate* unmanaged[Cdecl]<void*, global::Godot.NativeInterop.NativeGodotVariant*, void>"))
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsInitOnly = true,
                    IsStatic = true,
                    RequiresUnsafeCode = true,
                    DefaultValue = $"global::Godot.Bridge.GodotBridge.GDExtensionInterface.get_variant_to_type_constructor(global::Godot.NativeInterop.GDExtensionVariantType.{variantTypeName})",
                };
                type.DeclaredFields.Add(variantToTypeConstructorField);

                var variantFromTypeConstructorField = new FieldInfo($"_variantFrom{targetTypeName}Constructor", new TypeInfo("delegate* unmanaged[Cdecl]<global::Godot.NativeInterop.NativeGodotVariant*, void*, void>"))
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsInitOnly = true,
                    IsStatic = true,
                    RequiresUnsafeCode = true,
                    DefaultValue = $"global::Godot.Bridge.GodotBridge.GDExtensionInterface.get_variant_from_type_constructor(global::Godot.NativeInterop.GDExtensionVariantType.{variantTypeName})",
                };
                type.DeclaredFields.Add(variantFromTypeConstructorField);

                if (!engineClass.HasDestructor)
                {
                    Debug.Assert(!targetTypeUnmanaged.IsByRefLike, "Target of Variant.CreateFrom method has no destructor so it must not be a ref struct.");

                    var convertTypeToVariantMethod = new MethodInfo($"ConvertTo{targetTypeName}")
                    {
                        VisibilityAttributes = VisibilityAttributes.Assembly,
                        IsStatic = true,
                        ReturnParameter = ReturnInfo.FromType(targetTypeUnmanaged),
                        Parameters =
                        {
                            new ParameterInfo("value", type)
                            {
                                RefKind = RefKind.In,
                                ScopedKind = ScopedKind.ScopedRef,
                            },
                        },
                        Body = new ConvertToVariantTakingOwnership(targetTypeName,
                            isTypePackedArray: TypeDB.IsTypePackedArray(engineClass.Name),
                            isTypeAPointerInVariant: IsTypeAPointerInVariant(engineClass.Name)),
                    };
                    type.DeclaredMethods.Add(convertTypeToVariantMethod);

                    var createVariantFromTypeMethod = new MethodInfo($"CreateFrom{targetTypeName}")
                    {
                        VisibilityAttributes = VisibilityAttributes.Assembly,
                        IsStatic = true,
                        ReturnParameter = ReturnInfo.FromType(type),
                        Parameters = { new ParameterInfo("value", targetTypeUnmanaged) },
                        Body = CanTypeBeCreatedWithoutInterop(engineClass.Name)
                            ? new CreateVariantTakingOwnership(targetTypeName)
                            : new CreateVariantCopying(targetTypeName, context.TypeDB),
                    };
                    type.DeclaredMethods.Add(createVariantFromTypeMethod);
                }
                else
                {
                    // When the Variant type has a destructor we have to be very careful about ownership
                    // of the disposable value, so we generate a pair of methods so we can convert from/to
                    // Variant copying the value or taking ownership of the value, and we name these methods
                    // with a very explicit name to be very clear about it.

                    {
                        var convertTypeToVariantMethod = new MethodInfo($"ConvertTo{targetTypeName}")
                        {
                            VisibilityAttributes = VisibilityAttributes.Assembly,
                            IsStatic = true,
                            ReturnParameter = ReturnInfo.FromType(targetTypeUnmanaged),
                            Parameters =
                            {
                                new ParameterInfo("value", type)
                                {
                                    RefKind = RefKind.In,
                                    ScopedKind = ScopedKind.ScopedRef,
                                },
                            },
                            Body = new ConvertToVariantCopying(targetTypeName),
                        };
                        type.DeclaredMethods.Add(convertTypeToVariantMethod);
                    }


                    if (CanTypeBeCreatedWithoutInterop(engineClass.Name))
                    {
                        var convertTypeToVariantMethod = new MethodInfo($"GetOrConvertTo{targetTypeName}")
                        {
                            VisibilityAttributes = VisibilityAttributes.Assembly,
                            IsStatic = true,
                            ReturnParameter = ReturnInfo.FromType(targetTypeUnmanaged),
                            Parameters =
                            {
                                new ParameterInfo("value", type)
                                {
                                    RefKind = RefKind.In,
                                    ScopedKind = ScopedKind.ScopedRef,
                                },
                            },
                            Body = new ConvertToVariantTakingOwnership(targetTypeName,
                                isTypePackedArray: TypeDB.IsTypePackedArray(engineClass.Name),
                                isTypeAPointerInVariant: IsTypeAPointerInVariant(engineClass.Name)),
                        };
                        type.DeclaredMethods.Add(convertTypeToVariantMethod);
                    }

                    {
                        var createVariantFromTypeMethod = new MethodInfo($"CreateFrom{targetTypeName}Copying")
                        {
                            VisibilityAttributes = VisibilityAttributes.Assembly,
                            IsStatic = true,
                            ReturnParameter = ReturnInfo.FromType(type),
                            Parameters =
                            {
                                new ParameterInfo("value", targetTypeUnmanaged)
                                {
                                    RefKind = targetTypeUnmanaged.IsByRefLike
                                        ? RefKind.In
                                        : RefKind.None,
                                    ScopedKind = targetTypeUnmanaged.IsByRefLike
                                        ? ScopedKind.ScopedRef
                                        : ScopedKind.None,
                                },
                            },
                            Body = new CreateVariantCopying(targetTypeName, context.TypeDB),
                        };
                        type.DeclaredMethods.Add(createVariantFromTypeMethod);
                    }

                    if (CanTypeBeCreatedWithoutInterop(engineClass.Name))
                    {
                        var createVariantFromTypeMethod = new MethodInfo($"CreateFrom{targetTypeName}TakingOwnership")
                        {
                            VisibilityAttributes = VisibilityAttributes.Assembly,
                            IsStatic = true,
                            ReturnParameter = ReturnInfo.FromType(type),
                            Parameters =
                            {
                                new ParameterInfo("value", targetTypeUnmanaged)
                                {
                                    RefKind = targetTypeUnmanaged.IsByRefLike
                                        ? RefKind.In
                                        : RefKind.None,
                                    ScopedKind = targetTypeUnmanaged.IsByRefLike
                                        ? ScopedKind.ScopedRef
                                        : ScopedKind.None,
                                },
                            },
                            Body = new CreateVariantTakingOwnership(targetTypeName),
                        };
                        type.DeclaredMethods.Add(createVariantFromTypeMethod);
                    }
                }
            }
        }

        var classSizes = context.Api.BuiltInClassSizes.First(sizes => sizes.BuildConfiguration == context.Options.BuildConfiguration).Sizes;
        foreach (var engineClass in classSizes)
        {
            if (!ShouldGenerateBuiltInClass(engineClass.Name))
            {
                // Only generate the struct layout members for the built-in classes that
                // we generate a native interop struct for.
                continue;
            }

            if (!_builtinClasses.TryGetValue(engineClass.Name, out var type))
            {
                continue;
            }
            Debug.Assert(context.IsTypeGenerated(type));

            TypeInfo? movableType = new TypeInfo("Movable")
            {
                IsReadOnly = true,
                VisibilityAttributes = VisibilityAttributes.Assembly,
                TypeAttributes = TypeAttributes.ValueType,
                ContainingType = type,
            };
            type.NestedTypes.Add(movableType);

            const string ExplicitLayoutAttr = "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";
            type.Attributes.Add(ExplicitLayoutAttr);
            movableType.Attributes.Add(ExplicitLayoutAttr);

            Debug.Assert(engineClass.Size >= 1, $"Built-in class '{engineClass.Name}' has an invalid size ({engineClass.Size}).");
            for (int i = 0; i < engineClass.Size; i++)
            {
                var dataField = new FieldInfo($"_data{i}", KnownTypes.SystemByte)
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    IsInitOnly = true,
                    Attributes =
                    {
                        $"[global::System.Runtime.InteropServices.FieldOffset({i})]"
                    },
                };
                type.DeclaredFields.Add(dataField);
                movableType.DeclaredFields.Add(dataField);
            }

            {
                var method = new MethodInfo("GetUnsafeAddress")
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsReadOnly = true,
                    ReturnParameter = ReturnInfo.FromType(type.MakePointerType()),
                    Body = MethodBody.CreateUnsafe(writer =>
                    {
                        writer.WriteLine($"return ({type.MakePointerType().FullNameWithGlobal})global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::System.Runtime.CompilerServices.Unsafe.AsRef(in _data0));");
                    }),
                };
                type.DeclaredMethods.Add(method);
            }

            {
                var dangerousSelfRefProperty = new PropertyInfo("DangerousSelfRef", type)
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsReadOnly = true,
                    Getter = new MethodInfo("get_DangerousSelfRef")
                    {
                        ReturnParameter = ReturnInfo.FromType(type, RefKind.Ref),
                        Body = MethodBody.CreateUnsafe(writer =>
                        {
                            writer.WriteLine($"return ref *({type.FullNameWithGlobal}*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::System.Runtime.CompilerServices.Unsafe.AsRef(in this));");
                        }),
                    },
                };
                movableType.DeclaredProperties.Add(dangerousSelfRefProperty);
            }

            {
                var method = new MethodInfo("AsMovable")
                {
                    VisibilityAttributes = VisibilityAttributes.Assembly,
                    IsReadOnly = true,
                    ReturnParameter = ReturnInfo.FromType(movableType),
                    Body = MethodBody.CreateUnsafe(writer =>
                    {
                        writer.WriteLine("return *(Movable*)GetUnsafeAddress();");
                    }),
                };
                type.DeclaredMethods.Add(method);
            }
        }
    }

    /// <summary>
    /// Determines if the type with the given engine name should be generated in C#.
    /// Some types are defined manually in C# or we use existing types from the BCL.
    /// </summary>
    /// <param name="engineTypeName">Name of the type in Godot.</param>
    /// <returns>Whether the type should be generated.</returns>
    private static bool ShouldGenerateBuiltInClass(string engineTypeName)
    {
        return engineTypeName is not ("Nil" or "void" or "bool" or "real_t"
            or "float" or "double" or "int" or "int8_t" or "uint8_t"
            or "int16_t" or "uint16_t" or "int32_t" or "int64_t"
            or "uint32_t" or "uint64_t" or "RID" or "AABB"
            or "Basis" or "Color" or "Plane" or "Projection"
            or "Quaternion" or "Rect2" or "Rect2i" or "Transform2D"
            or "Transform3D" or "Vector2" or "Vector2i" or "Vector3"
            or "Vector3i" or "Vector4" or "Vector4i");
    }

    /// <summary>
    /// Determines if the type with the given engine name should generate a static helper class in C#.
    /// These types don't generate a native interop struct because we already define an unmanaged
    /// type manually that can be used for interop, but we still need a static class to put the
    /// native methods because we may need some that we don't want to re-implement entirely in C#.
    /// </summary>
    /// <param name="engineTypeName">Name of the type in Godot.</param>
    /// <returns>Whether the static helper should be generated.</returns>
    private static bool ShouldGenerateBuiltInClassHelpers(string engineTypeName)
    {
        return engineTypeName is "Color";
    }

    /// <summary>
    /// Determines if the type with the given engine name is stored as a pointer
    /// in the Variant union type.
    /// </summary>
    /// <param name="engineTypeName">Name of the type in Godot.</param>
    /// <returns>Whether the type is stored as a pointer in Variant.</returns>
    private static bool IsTypeAPointerInVariant(string engineTypeName)
    {
        return engineTypeName is "Transform2D" or "Transform3D"
            or "AABB" or "Basis" or "Projection";
    }

    /// <summary>
    /// Determines if the type with the given engine name can be created without
    /// an interop call. These types can be created using the NativeGodotVariant
    /// type directly in C# because the struct layout of the Variant union type
    /// matches C++ (this is usually the case for types that aren't stored as
    /// a pointer, see <see cref="IsTypeAPointerInVariant(string)"/>).
    /// </summary>
    /// <param name="engineTypeName">Name of the type in Godot.</param>
    /// <returns>Whether the type is stored as a pointer.</returns>
    private static bool CanTypeBeCreatedWithoutInterop(string engineTypeName)
    {
        return engineTypeName is "bool" or "int" or "float"
            or "String" or "StringName" or "NodePath"
            or "Vector2" or "Vector2i" or "Rect2" or "Rect2i"
            or "Vector3" or "Vector3i" or "Vector4" or "Vector4i"
            or "Plane" or "Quaternion" or "Color"
            or "RID" or "Callable" or "Signal";
    }

    /// <summary>
    /// Try to get the field name in the <c>GDExtensionVariantType</c> enum
    /// for the variant type with the given engine name.
    /// </summary>
    /// <param name="engineTypeName">Name of the variant type.</param>
    /// <param name="variantType">Name of the enum field.</param>
    /// <returns>Whether the variant type was found in the enum.</returns>
    private static bool TryGetVariantType(string engineTypeName, [NotNullWhen(true)] out string? variantType)
    {
        variantType = engineTypeName switch
        {
            "Nil" => "GDEXTENSION_VARIANT_TYPE_NIL",
            "bool" => "GDEXTENSION_VARIANT_TYPE_BOOL",
            "int" => "GDEXTENSION_VARIANT_TYPE_INT",
            "float" => "GDEXTENSION_VARIANT_TYPE_FLOAT",
            "String" => "GDEXTENSION_VARIANT_TYPE_STRING",
            "Vector2" => "GDEXTENSION_VARIANT_TYPE_VECTOR2",
            "Vector2i" => "GDEXTENSION_VARIANT_TYPE_VECTOR2I",
            "Rect2" => "GDEXTENSION_VARIANT_TYPE_RECT2",
            "Rect2i" => "GDEXTENSION_VARIANT_TYPE_RECT2I",
            "Vector3" => "GDEXTENSION_VARIANT_TYPE_VECTOR3",
            "Vector3i" => "GDEXTENSION_VARIANT_TYPE_VECTOR3I",
            "Transform2D" => "GDEXTENSION_VARIANT_TYPE_TRANSFORM2D",
            "Vector4" => "GDEXTENSION_VARIANT_TYPE_VECTOR4",
            "Vector4i" => "GDEXTENSION_VARIANT_TYPE_VECTOR4I",
            "Plane" => "GDEXTENSION_VARIANT_TYPE_PLANE",
            "Quaternion" => "GDEXTENSION_VARIANT_TYPE_QUATERNION",
            "AABB" => "GDEXTENSION_VARIANT_TYPE_AABB",
            "Basis" => "GDEXTENSION_VARIANT_TYPE_BASIS",
            "Transform3D" => "GDEXTENSION_VARIANT_TYPE_TRANSFORM3D",
            "Projection" => "GDEXTENSION_VARIANT_TYPE_PROJECTION",
            "Color" => "GDEXTENSION_VARIANT_TYPE_COLOR",
            "StringName" => "GDEXTENSION_VARIANT_TYPE_STRING_NAME",
            "NodePath" => "GDEXTENSION_VARIANT_TYPE_NODE_PATH",
            "RID" => "GDEXTENSION_VARIANT_TYPE_RID",
            "Object" => "GDEXTENSION_VARIANT_TYPE_OBJECT",
            "Callable" => "GDEXTENSION_VARIANT_TYPE_CALLABLE",
            "Signal" => "GDEXTENSION_VARIANT_TYPE_SIGNAL",
            "Dictionary" => "GDEXTENSION_VARIANT_TYPE_DICTIONARY",
            "Array" => "GDEXTENSION_VARIANT_TYPE_ARRAY",
            "PackedByteArray" => "GDEXTENSION_VARIANT_TYPE_PACKED_BYTE_ARRAY",
            "PackedInt32Array" => "GDEXTENSION_VARIANT_TYPE_PACKED_INT32_ARRAY",
            "PackedInt64Array" => "GDEXTENSION_VARIANT_TYPE_PACKED_INT64_ARRAY",
            "PackedFloat32Array" => "GDEXTENSION_VARIANT_TYPE_PACKED_FLOAT32_ARRAY",
            "PackedFloat64Array" => "GDEXTENSION_VARIANT_TYPE_PACKED_FLOAT64_ARRAY",
            "PackedStringArray" => "GDEXTENSION_VARIANT_TYPE_PACKED_STRING_ARRAY",
            "PackedVector2Array" => "GDEXTENSION_VARIANT_TYPE_PACKED_VECTOR2_ARRAY",
            "PackedVector3Array" => "GDEXTENSION_VARIANT_TYPE_PACKED_VECTOR3_ARRAY",
            "PackedColorArray" => "GDEXTENSION_VARIANT_TYPE_PACKED_COLOR_ARRAY",
            "PackedVector4Array" => "GDEXTENSION_VARIANT_TYPE_PACKED_VECTOR4_ARRAY",
            _ => null,
        };
        return !string.IsNullOrEmpty(variantType);
    }

    /// <summary>
    /// Try to get the field name in the <c>GDExtensionVariantOperator</c> enum
    /// for the operator kind with the given engine name.
    /// </summary>
    /// <param name="engineOperatorName">Name of the operator kind.</param>
    /// <param name="operatorKind">Name of the enum field.</param>
    /// <returns>Whether the variant type was found in the enum.</returns>
    private static bool TryGetOperatorKind(string engineOperatorName, [NotNullWhen(true)] out string? operatorKind)
    {
        operatorKind = engineOperatorName switch
        {
            "==" => "GDEXTENSION_VARIANT_OP_EQUAL",
            "!=" => "GDEXTENSION_VARIANT_OP_NOT_EQUAL",
            "<" => "GDEXTENSION_VARIANT_OP_LESS",
            "<=" => "GDEXTENSION_VARIANT_OP_LESS_EQUAL",
            ">" => "GDEXTENSION_VARIANT_OP_GREATER",
            ">=" => "GDEXTENSION_VARIANT_OP_GREATER_EQUAL",
            "+" => "GDEXTENSION_VARIANT_OP_ADD",
            "-" => "GDEXTENSION_VARIANT_OP_SUBTRACT",
            "*" => "GDEXTENSION_VARIANT_OP_MULTIPLY",
            "/" => "GDEXTENSION_VARIANT_OP_DIVIDE",
            "unary-" => "GDEXTENSION_VARIANT_OP_NEGATE",
            "unary+" => "GDEXTENSION_VARIANT_OP_POSITIVE",
            "%" => "GDEXTENSION_VARIANT_OP_MODULE",
            "**" => "GDEXTENSION_VARIANT_OP_POWER",
            "<<" => "GDEXTENSION_VARIANT_OP_SHIFT_LEFT",
            ">>" => "GDEXTENSION_VARIANT_OP_SHIFT_RIGHT",
            "&" => "GDEXTENSION_VARIANT_OP_BIT_AND",
            "|" => "GDEXTENSION_VARIANT_OP_BIT_OR",
            "^" => "GDEXTENSION_VARIANT_OP_BIT_XOR",
            "~" => "GDEXTENSION_VARIANT_OP_BIT_NEGATE",
            "and" => "GDEXTENSION_VARIANT_OP_AND",
            "or" => "GDEXTENSION_VARIANT_OP_OR",
            "xor" => "GDEXTENSION_VARIANT_OP_XOR",
            "not" => "GDEXTENSION_VARIANT_OP_NOT",
            "in" => "GDEXTENSION_VARIANT_OP_IN",
            _ => null,
        };
        return !string.IsNullOrEmpty(operatorKind);
    }

    /// <summary>
    /// Try to get a valid identifier that can be used in C# for the given operator name.
    /// </summary>
    /// <param name="engineOperatorName">Original operator name in Godot.</param>
    /// <param name="operatorName">Valid C# identifier for the operator.</param>
    /// <returns>Whether an identifier was defined for the operator.</returns>
    private static bool TryGetOperatorName(string engineOperatorName, [NotNullWhen(true)] out string? operatorName)
    {
        operatorName = engineOperatorName switch
        {
            "==" => "OperatorEqual",
            "!=" => "OperatorNotEqual",
            "<" => "OperatorLess",
            "<=" => "OperatorLessEqual",
            ">" => "OperatorGreater",
            ">=" => "OperatorGreaterEqual",
            "+" => "OperatorAdd",
            "-" => "OperatorSubtract",
            "*" => "OperatorMultiply",
            "/" => "OperatorDivide",
            "unary-" => "OperatorNegate",
            "unary+" => "OperatorPositive",
            "%" => "OperatorModule",
            "**" => "OperatorPower",
            "<<" => "OperatorShiftLeft",
            ">>" => "OperatorShiftRight",
            "&" => "OperatorBitAnd",
            "|" => "OperatorBitOr",
            "^" => "OperatorBitXor",
            "~" => "OperatorBitNegate",
            "and" => "OperatorAnd",
            "or" => "OperatorOr",
            "xor" => "OperatorXor",
            "not" => "OperatorNot",
            "in" => "OperatorIn",
            _ => null,
        };

        return !string.IsNullOrEmpty(operatorName);
    }
}

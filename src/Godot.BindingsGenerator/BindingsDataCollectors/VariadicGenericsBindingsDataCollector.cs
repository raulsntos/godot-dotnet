using System;
using System.Collections.Generic;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class VariadicGenericsBindingsDataCollector : BindingsDataCollector
{
    public override void Populate(BindingsData.CollectionContext context)
    {
        // Generate GodotObject.TryCallVirtualMethod<T..> and GodotObject.CallVirtualMethod<T..>
        {
            var type = new TypeInfo("GodotObject", "Godot")
            {
                TypeAttributes = TypeAttributes.ReferenceType,
                IsPartial = true,
            };
            context.AddGeneratedType("GodotObject.CallVirtualMethod.cs", type, configuration =>
            {
                configuration.Nullable = true;
            });

            var tryCallVirtualMethodPrototype = new MethodInfo("TryCallVirtualMethod")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                Parameters = { new ParameterInfo("name", KnownTypes.GodotStringName) },
                ReturnParameter = ReturnInfo.FromType(KnownTypes.SystemBoolean),
            };

            var tryCallVirtualMethods = CreateGenericMethods(tryCallVirtualMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", typeParameter));
                }

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("global::System.ArgumentNullException.ThrowIfNull(name);");

                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine("return TryCallVirtualMethodCore(name, default, out _);");
                        return;
                    }

                    for (int i = 0; i < genericTypeArgumentCount; i++)
                    {
                        writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant __arg{i} = global::Godot.NativeInterop.Marshalling.ConvertToVariant<T{i}>(in parameter{i});");
                    }

                    writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant** args = stackalloc global::Godot.NativeInterop.NativeGodotVariant*[{genericTypeArgumentCount}]");
                    writer.WriteLine('{');
                    writer.Indent++;

                    for (int i = 0; i < genericTypeArgumentCount; i++)
                    {
                        writer.WriteLine($"__arg{i}.GetUnsafeAddress(),");
                    }

                    writer.Indent--;
                    writer.WriteLine("};");

                    writer.WriteLine($"return TryCallVirtualMethodCore(name, new(args, {genericTypeArgumentCount}), out _);");
                });
            });
            type.DeclaredMethods.AddRange(tryCallVirtualMethods);

            var tryCallVirtualMethodsWithReturn = CreateGenericMethods(tryCallVirtualMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", typeParameter));
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);
                method.Parameters.Add(new ParameterInfo("result", returnTypeParameter)
                {
                    Attributes = { "[global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)]" },
                    RefKind = RefKind.Out,
                });

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("global::System.ArgumentNullException.ThrowIfNull(name);");

                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine("if (TryCallVirtualMethodCore(name, default, out global::Godot.NativeInterop.NativeGodotVariant returnValue))");
                    }
                    else
                    {
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant __arg{i} = global::Godot.NativeInterop.Marshalling.ConvertToVariant<T{i}>(in parameter{i});");
                        }

                        writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant** args = stackalloc global::Godot.NativeInterop.NativeGodotVariant*[{genericTypeArgumentCount}]");
                        writer.WriteLine('{');
                        writer.Indent++;

                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.WriteLine($"__arg{i}.GetUnsafeAddress(),");
                        }

                        writer.Indent--;
                        writer.WriteLine("};");

                        writer.WriteLine($"if (TryCallVirtualMethodCore(name, new(args, {genericTypeArgumentCount}), out global::Godot.NativeInterop.NativeGodotVariant returnValue))");
                    }

                    writer.OpenBlock();
                    writer.WriteLine("result = global::Godot.NativeInterop.Marshalling.ConvertFromVariant<TResult>(in returnValue);");
                    writer.WriteLine("return true;");
                    writer.CloseBlock();

                    writer.WriteLine("result = default;");
                    writer.WriteLine("return false;");
                });
            });
            type.DeclaredMethods.AddRange(tryCallVirtualMethodsWithReturn);

            var callVirtualMethodPrototype = new MethodInfo("CallVirtualMethod")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                Parameters = { new ParameterInfo("name", KnownTypes.GodotStringName) },
            };

            var callVirtualMethods = CreateGenericMethods(callVirtualMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", typeParameter));
                }

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("global::System.ArgumentNullException.ThrowIfNull(name);");

                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine("_ = CallVirtualMethodCore(name, default);");
                        return;
                    }

                    for (int i = 0; i < genericTypeArgumentCount; i++)
                    {
                        writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant __arg{i} = global::Godot.NativeInterop.Marshalling.ConvertToVariant<T{i}>(in parameter{i});");
                    }

                    writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant** args = stackalloc global::Godot.NativeInterop.NativeGodotVariant*[{genericTypeArgumentCount}]");
                    writer.WriteLine('{');
                    writer.Indent++;

                    for (int i = 0; i < genericTypeArgumentCount; i++)
                    {
                        writer.WriteLine($"__arg{i}.GetUnsafeAddress(),");
                    }

                    writer.Indent--;
                    writer.WriteLine("};");

                    writer.WriteLine($"_ = CallVirtualMethodCore(name, new(args, {genericTypeArgumentCount}));");
                });
            });
            type.DeclaredMethods.AddRange(callVirtualMethods);

            var callVirtualMethodsWithReturn = CreateGenericMethods(callVirtualMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", typeParameter));
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);
                method.ReturnParameter = ReturnInfo.FromType(returnTypeParameter);

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("global::System.ArgumentNullException.ThrowIfNull(name);");

                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine("global::Godot.NativeInterop.NativeGodotVariant returnValue = CallVirtualMethodCore(name, default);");
                    }
                    else
                    {
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant __arg{i} = global::Godot.NativeInterop.Marshalling.ConvertToVariant<T{i}>(in parameter{i});");
                        }

                        writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant** args = stackalloc global::Godot.NativeInterop.NativeGodotVariant*[{genericTypeArgumentCount}]");
                        writer.WriteLine('{');
                        writer.Indent++;

                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.WriteLine($"__arg{i}.GetUnsafeAddress(),");
                        }

                        writer.Indent--;
                        writer.WriteLine("};");

                        writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant returnValue = CallVirtualMethodCore(name, new(args, {genericTypeArgumentCount}));");
                    }

                    writer.WriteLine("return global::Godot.NativeInterop.Marshalling.ConvertFromVariant<TResult>(in returnValue);");
                });
            });
            type.DeclaredMethods.AddRange(callVirtualMethodsWithReturn);
        }

        // Generate MethodBindInvoker.From<TInstance, T..> and MethodBindInvoker.FromStatic<T..>
        {
            var type = new TypeInfo("MethodBindInvoker", "Godot.Bridge")
            {
                TypeAttributes = TypeAttributes.ValueType,
                IsPartial = true,
            };
            context.AddGeneratedType("MethodBindInvoker.From.cs", type, configuration =>
            {
                configuration.Nullable = true;
            });

            var fromMethodPrototype = new MethodInfo("From")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsStatic = true,
                ReturnParameter = ReturnInfo.FromType(type),
            };

            var fromMethods = CreateGenericMethods(fromMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var instanceTypeParameter = new TypeParameterInfo("TInstance")
                {
                    ConstraintTypes = { KnownTypes.GodotObject },
                };
                method.TypeParameters.Add(instanceTypeParameter);
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                }

                var actionType = new TypeInfo("Action", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 1,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("action", actionType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("static void TrampolineWithPtrArgs(global::Godot.GodotObject instance, global::System.Delegate @delegate, void** args, void* outRet)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)((TInstance)instance);");
                    }
                    else
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        writer.WriteLine("(TInstance)instance,");
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"global::Godot.NativeInterop.Marshalling.ConvertFromUnmanaged<T{i}>(args[{i}])");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.CloseBlock();

                    writer.WriteLine("static void TrampolineWithVariantArgs(global::Godot.Bridge.MethodInfo methodInfo, global::Godot.GodotObject instance, global::System.Delegate @delegate, global::Godot.NativeInterop.NativeGodotVariantPtrSpan args, out global::Godot.NativeInterop.NativeGodotVariant ret)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)((TInstance)instance);");
                    }
                    else
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        writer.WriteLine("(TInstance)instance,");
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"GetArgOrDefault<T{i}>(methodInfo, args, {i})");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("ret = default;");
                    writer.CloseBlock();

                    writer.WriteLine("return CreateWithUnsafeTrampoline(action, &TrampolineWithPtrArgs, &TrampolineWithVariantArgs);");
                });
            });
            type.DeclaredMethods.AddRange(fromMethods);

            var fromMethodsWithReturn = CreateGenericMethods(fromMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var instanceTypeParameter = new TypeParameterInfo("TInstance")
                {
                    ConstraintTypes = { KnownTypes.GodotObject },
                };
                method.TypeParameters.Add(instanceTypeParameter);
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);

                var funcType = new TypeInfo("Func", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 2,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("func", funcType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("static void TrampolineWithPtrArgs(global::Godot.GodotObject instance, global::System.Delegate @delegate, void** args, void* outRet)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)((TInstance)instance);");
                    }
                    else
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        writer.WriteLine("(TInstance)instance,");
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"global::Godot.NativeInterop.Marshalling.ConvertFromUnmanaged<T{i}>(args[{i}])");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("global::Godot.NativeInterop.Marshalling.WriteUnmanaged<TResult>(outRet, in res);");
                    writer.CloseBlock();

                    writer.WriteLine("static void TrampolineWithVariantArgs(global::Godot.Bridge.MethodInfo methodInfo, global::Godot.GodotObject instance, global::System.Delegate @delegate, global::Godot.NativeInterop.NativeGodotVariantPtrSpan args, out global::Godot.NativeInterop.NativeGodotVariant ret)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)((TInstance)instance);");
                    }
                    else
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        writer.WriteLine("(TInstance)instance,");
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"GetArgOrDefault<T{i}>(methodInfo, args, {i})");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("ret = global::Godot.NativeInterop.Marshalling.ConvertToVariant<TResult>(in res);");
                    writer.CloseBlock();

                    writer.WriteLine("return CreateWithUnsafeTrampoline(func, &TrampolineWithPtrArgs, &TrampolineWithVariantArgs);");
                });
            });
            type.DeclaredMethods.AddRange(fromMethodsWithReturn);

            var fromStaticMethodPrototype = new MethodInfo("FromStatic")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsStatic = true,
                ReturnParameter = ReturnInfo.FromType(type),
            };

            var fromStaticMethods = CreateGenericMethods(fromStaticMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var actionType = new TypeInfo("Action", "System");
                if (genericTypeArgumentCount > 0)
                {
                    for (int i = 0; i < genericTypeArgumentCount; i++)
                    {
                        var typeParameter = new TypeParameterInfo($"T{i}")
                        {
                            Attributes = { "[global::Godot.MustBeVariant]" },
                        };
                        method.TypeParameters.Add(typeParameter);
                    }

                    actionType = new TypeInfo("Action", "System")
                    {
                        GenericTypeArgumentCount = genericTypeArgumentCount,
                    }.MakeGenericType(method.TypeParameters);
                }

                method.Parameters.Add(new ParameterInfo("action", actionType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("static void TrampolineWithPtrArgs(global::Godot.GodotObject instance, global::System.Delegate @delegate, void** args, void* outRet)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)();");
                    }
                    else
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"global::Godot.NativeInterop.Marshalling.ConvertFromUnmanaged<T{i}>(args[{i}])");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.CloseBlock();

                    writer.WriteLine("static void TrampolineWithVariantArgs(global::Godot.Bridge.MethodInfo methodInfo, global::Godot.GodotObject instance, global::System.Delegate @delegate, global::Godot.NativeInterop.NativeGodotVariantPtrSpan args, out global::Godot.NativeInterop.NativeGodotVariant ret)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)();");
                    }
                    else
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"GetArgOrDefault<T{i}>(methodInfo, args, {i})");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("ret = default;");
                    writer.CloseBlock();

                    writer.WriteLine("return CreateWithUnsafeTrampoline(action, &TrampolineWithPtrArgs, &TrampolineWithVariantArgs);");
                });
            });
            type.DeclaredMethods.AddRange(fromStaticMethods);

            var fromStaticMethodsWithReturn = CreateGenericMethods(fromStaticMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);

                var funcType = new TypeInfo("Func", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 1,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("func", funcType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("static void TrampolineWithPtrArgs(global::Godot.GodotObject instance, global::System.Delegate @delegate, void** args, void* outRet)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)();");
                    }
                    else
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"global::Godot.NativeInterop.Marshalling.ConvertFromUnmanaged<T{i}>(args[{i}])");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("global::Godot.NativeInterop.Marshalling.WriteUnmanaged<TResult>(outRet, in res);");
                    writer.CloseBlock();

                    writer.WriteLine("static void TrampolineWithVariantArgs(global::Godot.Bridge.MethodInfo methodInfo, global::Godot.GodotObject instance, global::System.Delegate @delegate, global::Godot.NativeInterop.NativeGodotVariantPtrSpan args, out global::Godot.NativeInterop.NativeGodotVariant ret)");
                    writer.OpenBlock();
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)();");
                    }
                    else
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"GetArgOrDefault<T{i}>(methodInfo, args, {i})");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("ret = global::Godot.NativeInterop.Marshalling.ConvertToVariant<TResult>(in res);");
                    writer.CloseBlock();

                    writer.WriteLine("return CreateWithUnsafeTrampoline(func, &TrampolineWithPtrArgs, &TrampolineWithVariantArgs);");
                });
            });
            type.DeclaredMethods.AddRange(fromStaticMethodsWithReturn);
        }

        // Generate Callable.From<T..>
        {
            var type = new TypeInfo("Callable", "Godot")
            {
                TypeAttributes = TypeAttributes.ValueType,
                IsPartial = true,
            };
            context.AddGeneratedType("Callable.From.cs", type, configuration =>
            {
                configuration.Nullable = true;
            });

            var fromMethodPrototype = new MethodInfo("From")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsStatic = true,
                ReturnParameter = ReturnInfo.FromType(type),
            };

            var fromMethods = CreateGenericMethods(fromMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var actionType = new TypeInfo("Action", "System");
                if (genericTypeArgumentCount > 0)
                {
                    for (int i = 0; i < genericTypeArgumentCount; i++)
                    {
                        var typeParameter = new TypeParameterInfo($"T{i}")
                        {
                            Attributes = { "[global::Godot.MustBeVariant]" },
                        };
                        method.TypeParameters.Add(typeParameter);
                    }

                    actionType = new TypeInfo("Action", "System")
                    {
                        GenericTypeArgumentCount = genericTypeArgumentCount,
                    }.MakeGenericType(method.TypeParameters);
                }

                method.Parameters.Add(new ParameterInfo("action", actionType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("static void Trampoline(object @delegate, global::Godot.NativeInterop.NativeGodotVariantPtrSpan args, out global::Godot.NativeInterop.NativeGodotVariant ret)");
                    writer.OpenBlock();
                    writer.WriteLine($"ThrowIfArgCountMismatch(args, {genericTypeArgumentCount});");
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)();");
                    }
                    else
                    {
                        writer.WriteLine($"(({actionType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"global::Godot.NativeInterop.Marshalling.ConvertFromVariant<T{i}>(in args[{i}])");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("ret = default;");
                    writer.CloseBlock();

                    writer.WriteLine("return CreateWithUnsafeTrampoline(action, &Trampoline);");
                });
            });
            type.DeclaredMethods.AddRange(fromMethods);

            var fromMethodsWithReturn = CreateGenericMethods(fromMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);

                var funcType = new TypeInfo("Func", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 1,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("func", funcType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("static void Trampoline(object @delegate, global::Godot.NativeInterop.NativeGodotVariantPtrSpan args, out global::Godot.NativeInterop.NativeGodotVariant ret)");
                    writer.OpenBlock();
                    writer.WriteLine($"ThrowIfArgCountMismatch(args, {genericTypeArgumentCount});");
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)();");
                    }
                    else
                    {
                        writer.WriteLine($"TResult res = (({funcType.FullNameWithGlobal})@delegate)(");
                        writer.Indent++;
                        for (int i = 0; i < genericTypeArgumentCount; i++)
                        {
                            writer.Write($"global::Godot.NativeInterop.Marshalling.ConvertFromVariant<T{i}>(in args[{i}])");
                            if (i < genericTypeArgumentCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                        }
                        writer.Indent--;
                        writer.WriteLine(");");
                    }
                    writer.WriteLine("ret = global::Godot.NativeInterop.Marshalling.ConvertToVariant<TResult>(in res);");
                    writer.CloseBlock();

                    writer.WriteLine("return CreateWithUnsafeTrampoline(func, &Trampoline);");
                });
            });
            type.DeclaredMethods.AddRange(fromMethodsWithReturn);
        }

        // Generate ClassDBRegistrationContext.BindMethod<TInstance, T..>, ClassDBRegistrationContext.BindStaticMethod<T..>, BindVirtualMethod<T>, and ClassDBRegistrationContext.BindVirtualMethodOverride<TInstance, T..>
        {
            var type = new TypeInfo("ClassDBRegistrationContext", "Godot.Bridge")
            {
                TypeAttributes = TypeAttributes.ReferenceType,
                IsPartial = true,
            };
            context.AddGeneratedType("ClassDBRegistrationContext.BindMethod.cs", type, configuration =>
            {
                configuration.Nullable = true;
            });

            var bindMethodMethodPrototype = new MethodInfo("BindMethod")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                Parameters = { new ParameterInfo("name", KnownTypes.GodotStringName) },
            };

            var bindMethodMethods = CreateGenericMethods(bindMethodMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var instanceTypeParameter = new TypeParameterInfo("TInstance")
                {
                    ConstraintTypes = { KnownTypes.GodotObject },
                };
                method.TypeParameters.Add(instanceTypeParameter);
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", new TypeInfo("ParameterInfo", "Godot.Bridge")));
                }

                var actionType = new TypeInfo("Action", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 1,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("action", actionType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine("BindMethod(new global::Godot.Bridge.MethodInfo(name, global::Godot.Bridge.MethodBindInvoker.From(action)));");
                    }
                    else
                    {
                        writer.WriteLine("BindMethod(new global::Godot.Bridge.MethodInfo(name, global::Godot.Bridge.MethodBindInvoker.From(action))");
                        writer.WriteLine('{');
                        writer.Indent++;
                        if (genericTypeArgumentCount == 1)
                        {
                            writer.WriteLine("Parameters = { parameter0 },");
                        }
                        else
                        {
                            writer.WriteLine("Parameters =");
                            writer.WriteLine('{');
                            writer.Indent++;
                            for (int i = 0; i < genericTypeArgumentCount; i++)
                            {
                                writer.WriteLine($"parameter{i},");
                            }
                            writer.Indent--;
                            writer.WriteLine("},");
                        }
                        writer.Indent--;
                        writer.WriteLine("});");
                    }
                });
            });
            type.DeclaredMethods.AddRange(bindMethodMethods);

            var bindMethodMethodsWithReturn = CreateGenericMethods(bindMethodMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var instanceTypeParameter = new TypeParameterInfo("TInstance")
                {
                    ConstraintTypes = { KnownTypes.GodotObject },
                };
                method.TypeParameters.Add(instanceTypeParameter);
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", new TypeInfo("ParameterInfo", "Godot.Bridge")));
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);
                method.Parameters.Add(new ParameterInfo("returnInfo", new TypeInfo("ReturnInfo", "Godot.Bridge")));

                var funcType = new TypeInfo("Func", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 2,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("func", funcType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("BindMethod(new global::Godot.Bridge.MethodInfo(name, global::Godot.Bridge.MethodBindInvoker.From(func))");
                    writer.WriteLine('{');
                    writer.Indent++;
                    writer.WriteLine("Return = returnInfo,");
                    if (genericTypeArgumentCount > 0)
                    {
                        if (genericTypeArgumentCount == 1)
                        {
                            writer.WriteLine("Parameters = { parameter0 },");
                        }
                        else
                        {
                            writer.WriteLine("Parameters =");
                            writer.WriteLine('{');
                            writer.Indent++;
                            for (int i = 0; i < genericTypeArgumentCount; i++)
                            {
                                writer.WriteLine($"parameter{i},");
                            }
                            writer.Indent--;
                            writer.WriteLine("},");
                        }
                    }
                    writer.Indent--;
                    writer.WriteLine("});");
                });
            });
            type.DeclaredMethods.AddRange(bindMethodMethodsWithReturn);

            var bindStaticMethodMethodPrototype = new MethodInfo("BindStaticMethod")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                Parameters = { new ParameterInfo("name", KnownTypes.GodotStringName) },
            };

            var bindStaticMethodMethods = CreateGenericMethods(bindStaticMethodMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var actionType = new TypeInfo("Action", "System");
                if (genericTypeArgumentCount > 0)
                {
                    for (int i = 0; i < genericTypeArgumentCount; i++)
                    {
                        var typeParameter = new TypeParameterInfo($"T{i}")
                        {
                            Attributes = { "[global::Godot.MustBeVariant]" },
                        };
                        method.TypeParameters.Add(typeParameter);
                        method.Parameters.Add(new ParameterInfo($"parameter{i}", new TypeInfo("ParameterInfo", "Godot.Bridge")));
                    }

                    actionType = new TypeInfo("Action", "System")
                    {
                        GenericTypeArgumentCount = genericTypeArgumentCount,
                    }.MakeGenericType(method.TypeParameters);
                }

                method.Parameters.Add(new ParameterInfo("action", actionType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("BindMethod(new global::Godot.Bridge.MethodInfo(name, global::Godot.Bridge.MethodBindInvoker.FromStatic(action))");
                    writer.WriteLine('{');
                    writer.Indent++;
                    writer.WriteLine("IsStatic = true,");
                    if (genericTypeArgumentCount > 0)
                    {
                        if (genericTypeArgumentCount == 1)
                        {
                            writer.WriteLine("Parameters = { parameter0 },");
                        }
                        else
                        {
                            writer.WriteLine("Parameters =");
                            writer.WriteLine('{');
                            writer.Indent++;
                            for (int i = 0; i < genericTypeArgumentCount; i++)
                            {
                                writer.WriteLine($"parameter{i},");
                            }
                            writer.Indent--;
                            writer.WriteLine("},");
                        }
                    }
                    writer.Indent--;
                    writer.WriteLine("});");
                });
            });
            type.DeclaredMethods.AddRange(bindStaticMethodMethods);

            var bindStaticMethodMethodsWithReturn = CreateGenericMethods(bindStaticMethodMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", new TypeInfo("ParameterInfo", "Godot.Bridge")));
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);
                method.Parameters.Add(new ParameterInfo("returnInfo", new TypeInfo("ReturnInfo", "Godot.Bridge")));

                var funcType = new TypeInfo("Func", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 1,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("func", funcType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("BindMethod(new global::Godot.Bridge.MethodInfo(name, global::Godot.Bridge.MethodBindInvoker.FromStatic(func))");
                    writer.WriteLine('{');
                    writer.Indent++;
                    writer.WriteLine("IsStatic = true,");
                    writer.WriteLine("Return = returnInfo,");
                    if (genericTypeArgumentCount > 0)
                    {
                        if (genericTypeArgumentCount == 1)
                        {
                            writer.WriteLine("Parameters = { parameter0 },");
                        }
                        else
                        {
                            writer.WriteLine("Parameters =");
                            writer.WriteLine('{');
                            writer.Indent++;
                            for (int i = 0; i < genericTypeArgumentCount; i++)
                            {
                                writer.WriteLine($"parameter{i},");
                            }
                            writer.Indent--;
                            writer.WriteLine("},");
                        }
                    }
                    writer.Indent--;
                    writer.WriteLine("});");
                });
            });
            type.DeclaredMethods.AddRange(bindStaticMethodMethodsWithReturn);

            var bindVirtualMethodMethodPrototype = new MethodInfo("BindVirtualMethod")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                Parameters = { new ParameterInfo("name", KnownTypes.GodotStringName) },
            };

            var bindVirtualMethodMethods = CreateGenericMethods(bindVirtualMethodMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", new TypeInfo("ParameterInfo", "Godot.Bridge")));
                }

                method.Body = MethodBody.Create(writer =>
                {
                    if (genericTypeArgumentCount == 0)
                    {
                        writer.WriteLine("BindVirtualMethod(new global::Godot.Bridge.VirtualMethodInfo(name));");
                        return;
                    }
                    else
                    {
                        writer.WriteLine("BindVirtualMethod(new global::Godot.Bridge.VirtualMethodInfo(name)");
                        writer.WriteLine('{');
                        writer.Indent++;
                        if (genericTypeArgumentCount == 1)
                        {
                            writer.WriteLine("Parameters = { parameter0 },");
                        }
                        else
                        {
                            writer.WriteLine("Parameters =");
                            writer.WriteLine('{');
                            writer.Indent++;
                            for (int i = 0; i < genericTypeArgumentCount; i++)
                            {
                                writer.WriteLine($"parameter{i},");
                            }
                            writer.Indent--;
                            writer.WriteLine("},");
                        }
                        writer.Indent--;
                        writer.WriteLine("});");
                    }
                });
            });
            type.DeclaredMethods.AddRange(bindVirtualMethodMethods);

            var bindVirtualMethodMethodsWithReturn = CreateGenericMethods(bindVirtualMethodMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                    method.Parameters.Add(new ParameterInfo($"parameter{i}", new TypeInfo("ParameterInfo", "Godot.Bridge")));
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);
                method.Parameters.Add(new ParameterInfo("returnInfo", new TypeInfo("ReturnInfo", "Godot.Bridge")));

                method.Body = MethodBody.Create(writer =>
                {
                    writer.WriteLine("BindVirtualMethod(new global::Godot.Bridge.VirtualMethodInfo(name)");
                    writer.WriteLine('{');
                    writer.Indent++;
                    writer.WriteLine("Return = returnInfo,");
                    if (genericTypeArgumentCount > 0)
                    {
                        if (genericTypeArgumentCount == 1)
                        {
                            writer.WriteLine("Parameters = { parameter0 },");
                        }
                        else
                        {
                            writer.WriteLine("Parameters =");
                            writer.WriteLine('{');
                            writer.Indent++;
                            for (int i = 0; i < genericTypeArgumentCount; i++)
                            {
                                writer.WriteLine($"parameter{i},");
                            }
                            writer.Indent--;
                            writer.WriteLine("},");
                        }
                    }
                    writer.Indent--;
                    writer.WriteLine("});");
                });
            });
            type.DeclaredMethods.AddRange(bindVirtualMethodMethodsWithReturn);

            var bindVirtualMethodOverrideMethodPrototype = new MethodInfo("BindVirtualMethodOverride")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                Parameters = { new ParameterInfo("name", KnownTypes.GodotStringName) },
            };

            var bindVirtualMethodOverrideMethods = CreateGenericMethods(bindVirtualMethodOverrideMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var instanceTypeParameter = new TypeParameterInfo("TInstance")
                {
                    ConstraintTypes = { KnownTypes.GodotObject },
                };
                method.TypeParameters.Add(instanceTypeParameter);
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                }

                var actionType = new TypeInfo("Action", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 1,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("action", actionType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("BindVirtualMethodOverride(new global::Godot.Bridge.VirtualMethodOverrideInfo(name, global::Godot.Bridge.MethodBindInvoker.From(action)));");
                });
            });
            type.DeclaredMethods.AddRange(bindVirtualMethodOverrideMethods);

            var bindVirtualMethodOverrideMethodsWithReturn = CreateGenericMethods(bindVirtualMethodOverrideMethodPrototype, static (MethodInfo method, int genericTypeArgumentCount) =>
            {
                var instanceTypeParameter = new TypeParameterInfo("TInstance")
                {
                    ConstraintTypes = { KnownTypes.GodotObject },
                };
                method.TypeParameters.Add(instanceTypeParameter);
                for (int i = 0; i < genericTypeArgumentCount; i++)
                {
                    var typeParameter = new TypeParameterInfo($"T{i}")
                    {
                        Attributes = { "[global::Godot.MustBeVariant]" },
                    };
                    method.TypeParameters.Add(typeParameter);
                }
                var returnTypeParameter = new TypeParameterInfo("TResult")
                {
                    Attributes = { "[global::Godot.MustBeVariant]" },
                };
                method.TypeParameters.Add(returnTypeParameter);

                var funcType = new TypeInfo("Func", "System")
                {
                    GenericTypeArgumentCount = genericTypeArgumentCount + 2,
                }.MakeGenericType(method.TypeParameters);

                method.Parameters.Add(new ParameterInfo("func", funcType));

                method.Body = MethodBody.CreateUnsafe(writer =>
                {
                    writer.WriteLine("BindVirtualMethodOverride(new global::Godot.Bridge.VirtualMethodOverrideInfo(name, global::Godot.Bridge.MethodBindInvoker.From(func)));");
                });
            });
            type.DeclaredMethods.AddRange(bindVirtualMethodOverrideMethodsWithReturn);
        }
    }

    private static IEnumerable<MethodInfo> CreateGenericMethods(MethodInfo prototype, Action<MethodInfo, int> configureOverload)
    {
        return CreateGenericMethods(prototype, (MethodInfo method, int genericTypeArgumentCount) =>
        {
            configureOverload(method, genericTypeArgumentCount);
            return true;
        });
    }

    private static IEnumerable<MethodInfo> CreateGenericMethods(MethodInfo prototype, Func<MethodInfo, int, bool> configureOverload)
    {
        // We generate methods with up to 11 generic type parameters because that's
        // the number of parameters of the method with the most parameters in the
        // bindings, so we need at least 11 parameters to be able to generate the
        // bindings.
        for (int i = 0; i < 11; i++)
        {
            var method = new MethodInfo(prototype.Name)
            {
                Attributes = [.. prototype.Attributes],
                VisibilityAttributes = prototype.VisibilityAttributes,
                ContractAttributes = prototype.ContractAttributes,
                IsStatic = prototype.IsStatic,
                IsReadOnly = prototype.IsReadOnly,
                Parameters = [.. prototype.Parameters],
                ReturnParameter = prototype.ReturnParameter,
                TypeParameters = [.. prototype.TypeParameters],
            };
            if (configureOverload(method, i))
            {
                yield return method;
            }
        }
    }
}

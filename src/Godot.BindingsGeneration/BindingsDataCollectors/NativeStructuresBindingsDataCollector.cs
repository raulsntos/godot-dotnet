using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot.BindingsGeneration.ApiDump;
using Godot.BindingsGeneration.Reflection;
using Godot.Common;

namespace Godot.BindingsGeneration;

internal sealed class NativeStructuresBindingsDataCollector : BindingsDataCollector
{
    // Stores the generated native structures by their engine name.
    private readonly Dictionary<string, TypeInfo> _nativeStructures = [];

    public override void Initialize(BindingsData.CollectionContext context)
    {
        if (context.IsExtension)
        {
            // Native structures are only generated for the core API, not for GDExtensions.
            return;
        }

        // Native structures format use C/C++ type names.
        RegisterPrimitiveType("uint8_t", KnownTypes.SystemByte);
        RegisterPrimitiveType("uint16_t", KnownTypes.SystemUInt16);
        RegisterPrimitiveType("uint32_t", KnownTypes.SystemUInt32);
        RegisterPrimitiveType("uint64_t", KnownTypes.SystemUInt64);
        RegisterPrimitiveType("int8_t", KnownTypes.SystemSByte);
        RegisterPrimitiveType("int16_t", KnownTypes.SystemInt16);
        RegisterPrimitiveType("int32_t", KnownTypes.SystemInt32);
        RegisterPrimitiveType("int64_t", KnownTypes.SystemInt64);
        RegisterPrimitiveType("real_t", context.Options.FloatPrecision == GodotFloatTypePrecision.Double ? KnownTypes.SystemDouble : KnownTypes.SystemSingle);
        RegisterPrimitiveType("ObjectID", KnownTypes.SystemUInt64);

        void RegisterPrimitiveType(string engineTypeName, TypeInfo typeInfo)
        {
            context.TypeDB.RegisterTypeName(engineTypeName, typeInfo);
        }

        foreach (var nativeStructure in context.Api.NativeStructures)
        {
            if (nativeStructure.Name == "ObjectID")
            {
                // Ignore "ObjectID", we use "ulong".
                continue;
            }

            var type = new TypeInfo(NamingUtils.PascalToPascalCase(nativeStructure.Name), context.Options.Namespace)
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                TypeAttributes = TypeAttributes.ValueType,
                IsPartial = true,
                Attributes =
                {
                    "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential)]",
                },
            };
            context.AddGeneratedType($"NativeStructures/{type.Name}.cs", type);
            context.TypeDB.RegisterTypeName(nativeStructure.Name, type);
            _nativeStructures.Add(nativeStructure.Name, type);
        }
    }

    public override void Populate(BindingsData.CollectionContext context)
    {
        if (context.IsExtension)
        {
            // Native structures are only generated for the core API, not for GDExtensions.
            return;
        }

        foreach (var nativeStructure in context.Api.NativeStructures)
        {
            if (!_nativeStructures.TryGetValue(nativeStructure.Name, out var type))
            {
                continue;
            }
            Debug.Assert(context.IsTypeGenerated(type));

            List<(string Name, string DefaultValue)> fieldDefaultValues = [];
            foreach (var structField in NativeStructureFormatParser.EnumerateFields(nativeStructure.Format))
            {
                string fieldName = NamingUtils.SnakeToPascalCase(structField.Name.ToString());
                string fieldTypeName = structField.Type.ToString().Replace("::", ".");
                TypeInfo fieldType = context.TypeDB.GetTypeFromEngineName(fieldTypeName, fieldTypeName);
                TypeInfo fieldTypeUnmanaged = fieldType;

                if (fieldType.IsPointerType && fieldType.PointedAtType.IsReferenceType)
                {
                    fieldType = fieldType.PointedAtType;
                }
                if (fieldType.IsReferenceType)
                {
                    fieldTypeUnmanaged = KnownTypes.SystemIntPtr;
                }

                // If the field type is one of our interop structs, we need to use 'Movable'
                // because we can't add a ref struct field to a regular struct.
                if (context.TypeDB.TryGetUnmanagedType(fieldType, out var unmanagedType) && unmanagedType.IsByRefLike)
                {
                    if (unmanagedType.Namespace == "Godot.NativeInterop")
                    {
                        fieldTypeUnmanaged = new TypeInfo("Movable")
                        {
                            TypeAttributes = TypeAttributes.ValueType,
                            ContainingType = unmanagedType,
                        };
                    }
                    else
                    {
                        fieldTypeUnmanaged = KnownTypes.SystemIntPtr;
                    }
                }

                if (structField.IsArray)
                {
                    var arrayType = context.GetOrAddInlineArray(structField.ArrayLength);
                    fieldType = arrayType.MakeGenericType([fieldType]);
                    fieldTypeUnmanaged = arrayType.MakeGenericType([fieldTypeUnmanaged]);
                }

                var field = new FieldInfo($"_{fieldName}", fieldTypeUnmanaged)
                {
                    VisibilityAttributes = VisibilityAttributes.Private,
                    RequiresUnsafeCode = fieldType.IsPointerType,
                };
                type.DeclaredFields.Add(field);

                if (structField.HasDefaultValue)
                {
                    string defaultValue = context.TypeDB.GetDefaultValueExpression(field.Type, structField.DefaultValue.ToString());
                    fieldDefaultValues.Add((field.Name, defaultValue));
                }

                var property = new PropertyInfo(fieldName, fieldType)
                {
                    VisibilityAttributes = VisibilityAttributes.Public,
                    Getter = new MethodInfo($"get_{fieldName}")
                    {
                        IsReadOnly = true,
                        ReturnParameter = ReturnInfo.FromType(fieldType),
                        Body = MethodBody.CreateUnsafe(writer =>
                        {
                            if (fieldType == fieldTypeUnmanaged)
                            {
                                writer.WriteLine($"return {field.Name};");
                                return;
                            }

                            if (fieldTypeUnmanaged.Name == "Movable")
                            {
                                writer.WriteLine($"return {fieldType.FullNameWithGlobal}.CreateCopying({field.Name}.DangerousSelfRef);");
                                return;
                            }

                            if (fieldType.IsReferenceType)
                            {
                                // If it's a reference type, and doesn't use 'Movable', it has to be a 'GodotObject'.
                                writer.Write("return ");
                                writer.Write("global::Godot.NativeInterop.Marshallers.GodotObjectMarshaller.GetOrCreateManagedInstance");
                                writer.WriteLine($"({field.Name});");
                                return;
                            }

                            throw new NotSupportedException($"Unsupported field type in native structure: {fieldType.FullNameWithGlobal}");
                        }),
                    },
                    Setter = new MethodInfo($"set_{fieldName}")
                    {
                        Parameters =
                        {
                            new ParameterInfo("value", fieldType),
                        },
                        Body = MethodBody.CreateUnsafe(writer =>
                        {
                            if (fieldType == fieldTypeUnmanaged)
                            {
                                writer.WriteLine($"{field.Name} = value;");
                                return;
                            }

                            if (fieldTypeUnmanaged.Name == "Movable")
                            {
                                writer.Write($"{field.Name} = value is not null");
                                writer.Write($" ? {fieldTypeUnmanaged.ContainingType!.FullNameWithGlobal}.Create(value.NativeValue.DangerousSelfRef).AsMovable()");
                                writer.WriteLine(" : default;");
                                return;
                            }

                            if (fieldType.IsReferenceType)
                            {
                                // If it's a reference type, and doesn't use 'Movable', it has to be a 'GodotObject'.
                                writer.Write($"{field.Name} = value is not null");
                                writer.Write(" ? value.NativePtr");
                                writer.WriteLine(" : default;");
                                return;
                            }

                            throw new NotSupportedException($"Unsupported field type in native structure: {fieldType.FullNameWithGlobal}");
                        }),
                    },
                };
                type.DeclaredProperties.Add(property);
            }

            // HARDCODED: This native structure has an invalid format in the API dump, it's missing a field
            // so we add it manually here.
            if (nativeStructure.Name == "ScriptLanguageExtensionProfilingInfo")
            {
                // First, make sure a newer version of the API dump didn't already fix this issue
                // and added the missing field.
                if (!nativeStructure.Format.Contains("internal_time"))
                {
                    type.DeclaredFields.Add(new FieldInfo("_InternalTime", KnownTypes.SystemUInt64)
                    {
                        VisibilityAttributes = VisibilityAttributes.Private,
                    });
                }
            }

            // C# structs usually default all their fields to zero-initialized values.
            // So we provide a 'Default' property that creates a struct instance with the
            // fields initialized to their default values according to the API dump.
            var defaultProperty = new PropertyInfo("Default", type)
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsStatic = true,
                Getter = new MethodInfo("get_Default")
                {
                    ReturnParameter = ReturnInfo.FromType(type),
                    Body = MethodBody.Create(writer =>
                    {
                        if (fieldDefaultValues.Count == 0)
                        {
                            writer.WriteLine("return new();");
                            return;
                        }

                        writer.WriteLine("return new()");
                        writer.WriteLine('{');
                        writer.Indent++;

                        foreach (var (fieldName, defaultValue) in fieldDefaultValues)
                        {
                            writer.WriteLine($"{fieldName} = {defaultValue},");
                        }

                        writer.Indent--;
                        writer.WriteLine("};");
                    }),
                },
            };
            type.DeclaredProperties.Add(defaultProperty);
        }
    }
}

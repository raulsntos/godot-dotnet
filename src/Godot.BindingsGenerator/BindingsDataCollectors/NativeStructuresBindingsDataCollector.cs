using System.Collections.Generic;
using System.Diagnostics;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class NativeStructuresBindingsDataCollector : BindingsDataCollector
{
    // Stores the generated native structures by their engine name.
    private readonly Dictionary<string, TypeInfo> _nativeStructures = [];

    public override void Initialize(BindingsData.CollectionContext context)
    {
        // Native structures format use C/C++ type names.
        context.TypeDB.RegisterTypeName("uint8_t", KnownTypes.SystemByte);
        context.TypeDB.RegisterTypeName("uint16_t", KnownTypes.SystemUInt16);
        context.TypeDB.RegisterTypeName("uint32_t", KnownTypes.SystemUInt32);
        context.TypeDB.RegisterTypeName("uint64_t", KnownTypes.SystemUInt64);
        context.TypeDB.RegisterTypeName("int8_t", KnownTypes.SystemSByte);
        context.TypeDB.RegisterTypeName("int16_t", KnownTypes.SystemInt16);
        context.TypeDB.RegisterTypeName("int32_t", KnownTypes.SystemInt32);
        context.TypeDB.RegisterTypeName("int64_t", KnownTypes.SystemInt64);
        context.TypeDB.RegisterTypeName("real_t", context.Options.FloatPrecision == BindingsGeneratorOptions.FloatTypePrecision.DoublePrecision ? KnownTypes.SystemDouble : KnownTypes.SystemSingle);
        context.TypeDB.RegisterTypeName("ObjectID", KnownTypes.SystemUInt64);

        foreach (var nativeStructure in context.Api.NativeStructures)
        {
            if (nativeStructure.Name == "ObjectID")
            {
                // Ignore "ObjectID", we use "ulong".
                continue;
            }

            var type = new TypeInfo(NamingUtils.PascalToPascalCase(nativeStructure.Name), "Godot")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                TypeAttributes = TypeAttributes.ValueType,
                IsPartial = true,
            };
            context.AddGeneratedType($"NativeStructures/{type.Name}.cs", type);
            context.TypeDB.RegisterTypeName(nativeStructure.Name, type);
            _nativeStructures.Add(nativeStructure.Name, type);
        }
    }

    public override void Populate(BindingsData.CollectionContext context)
    {
        foreach (var nativeStructure in context.Api.NativeStructures)
        {
            if (!_nativeStructures.TryGetValue(nativeStructure.Name, out var type))
            {
                return;
            }
            Debug.Assert(context.IsTypeGenerated(type));

            var fieldDefaultValues = new List<(string FieldName, string DefaultValue)>();
            foreach (var structField in NativeStructureFormatParser.EnumerateFields(nativeStructure.Format))
            {
                string fieldName = NamingUtils.SnakeToPascalCase(structField.Name.ToString());
                string fieldTypeName = structField.Type.ToString().Replace("::", ".");
                TypeInfo fieldType = context.TypeDB.GetTypeFromEngineName(fieldTypeName, fieldTypeName);

                if (structField.IsArray)
                {
                    fieldType = context.GetOrAddInlineArray(structField.ArrayLength).MakeGenericType([fieldType]);
                }

                var field = new FieldInfo($"_{fieldName}", fieldType)
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
                        ReturnParameter = ReturnInfo.FromType(fieldType),
                        Body = MethodBody.Create(writer =>
                        {
                            writer.WriteLine($"return _{fieldName};");
                        }),
                    },
                    Setter = new MethodInfo($"set_{fieldName}")
                    {
                        Parameters =
                        {
                            new ParameterInfo("value", fieldType),
                        },
                        Body = MethodBody.Create(writer =>
                        {
                            writer.WriteLine($"_{fieldName} = value;");
                        }),
                    },
                };
                type.DeclaredProperties.Add(property);
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

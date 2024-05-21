using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;
using Godot.BindingsGenerator.Logging;

namespace Godot.BindingsGenerator;

internal static partial class BindingsGenerator
{
    public static void Generate(GodotApi api, string outputDirectoryPath, BindingsGeneratorOptions? options = null, ILogger? logger = null)
    {
        options ??= new();
        logger ??= ConsoleLogger.Instance;

        var data = BindingsData.Create(api, options, logger);
        GenerateCore(api, outputDirectoryPath, options, data, logger);
    }

    private static void GenerateCore(GodotApi api, string outputDirectoryPath, BindingsGeneratorOptions options, BindingsData data, ILogger logger)
    {
        foreach (var generationData in data.Types)
        {
            var (type, path) = generationData;

            if (type.Name.Contains(':') || type.Name.Contains('/'))
            {
                logger.LogError($"Ignoring type '{type.Name}' with invalid name.");
                continue;
            }

            if (type.ContainingType is not null)
            {
                // Skip nested types, they will be generated as part of the generation of its containing type.
                continue;
            }

            string filePath = Path.Join(outputDirectoryPath, path);

            // Ensure that the output directory exists.
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using var stream = File.Create(filePath);
            using var streamWriter = new StreamWriter(stream);
            using var writer = new IndentedTextWriter(streamWriter);

            writer.WriteLine($"namespace {type.Namespace};");
            writer.WriteLine();

            if (!generationData.Nullable)
            {
                writer.WriteLine("#nullable disable");
                writer.WriteLine();
            }

            WriteType(writer, type);
        }
    }

    private static void WriteType(IndentedTextWriter writer, TypeInfo type)
    {
        if (type is DelegateInfo delegateType)
        {
            writer.WriteAttributes(delegateType);
            writer.WriteDelegateDeclaration(delegateType);
            writer.WriteLine(';');
        }
        else if (type is EnumInfo enumType)
        {
            writer.WriteXMLComment(enumType);
            writer.WriteAttributes(enumType);
            writer.WriteTypeDeclaration(enumType);
            writer.WriteLine();

            writer.OpenBlock();
            foreach (var (name, value, comment) in enumType.Values)
            {
                writer.WriteXMLComment(comment);
                writer.WriteLine($"{name} = {value},");
            }
            writer.CloseBlock();
        }
        else
        {
            writer.WriteXMLComment(type);
            writer.WriteAttributes(type);
            writer.WriteTypeDeclaration(type);
            writer.WriteLine();

            writer.OpenBlock();

            foreach (var nestedType in type.NestedTypes)
            {
                WriteType(writer, nestedType);
            }

            foreach (var @event in type.DeclaredEvents)
            {
                writer.WriteXMLComment(@event);
                writer.WriteAttributes(@event);
                writer.WriteEventDeclaration(@event);

                if (@event.AddAccessor is not null && @event.RemoveAccessor is not null)
                {
                    writer.WriteLine();
                    writer.OpenBlock();

                    writer.WriteLine("add");
                    writer.OpenBlock();
                    writer.WriteMethodBody(@event.AddAccessor);
                    writer.CloseBlock();

                    writer.WriteLine("remove");
                    writer.OpenBlock();
                    writer.WriteMethodBody(@event.RemoveAccessor);
                    writer.CloseBlock();

                    writer.CloseBlock();
                }
                else if (@event.AddAccessor is not null || @event.RemoveAccessor is not null)
                {
                    throw new InvalidOperationException($"Event '{@event.Name}' must have both add and remove accessors.");
                }
                else
                {
                    writer.WriteLine(';');
                }
            }

            foreach (var field in type.DeclaredFields)
            {
                writer.WriteXMLComment(field);
                writer.WriteAttributes(field);
                writer.WriteFieldDeclaration(field);
                writer.WriteLine(';');
            }

            foreach (var property in type.DeclaredProperties)
            {
                if (!property.CanRead && !property.CanWrite)
                {
                    throw new InvalidOperationException($"Property '{type.Name}.{property.Name}' must have at least a getter or a setter.");
                }

                writer.WriteXMLComment(property);
                writer.WriteAttributes(property);
                writer.WritePropertyDeclaration(property);
                writer.WriteLine();

                writer.OpenBlock();

                if (property.CanRead)
                {
                    writer.WriteAttributes(property.Getter);
                    if (property.Getter.IsReadOnly)
                    {
                        writer.Write("readonly ");
                    }
                    writer.WriteLine("get");
                    writer.OpenBlock();
                    writer.WriteMethodBody(property.Getter);
                    writer.CloseBlock();
                }

                if (property.CanWrite)
                {
                    writer.WriteAttributes(property.Setter);
                    if (property.Setter.IsReadOnly)
                    {
                        // This is likely a mistake, but we'll do as we are told and write the modifier.
                        writer.Write("readonly ");
                    }
                    writer.WriteLine("set");
                    writer.OpenBlock();
                    writer.WriteMethodBody(property.Setter);
                    writer.CloseBlock();
                }

                writer.CloseBlock();
            }

            foreach (var constructor in type.DeclaredConstructors)
            {
                writer.WriteXMLComment(constructor);
                writer.WriteAttributes(constructor);
                writer.WriteConstructorSignature(constructor, type);
                writer.WriteLine();
                writer.OpenBlock();
                writer.WriteMethodBody(constructor);
                writer.CloseBlock();
            }

            foreach (var method in type.DeclaredMethods)
            {
                Debug.Assert(!method.IsConstructor);

                writer.WriteXMLComment(method);
                writer.WriteAttributes(method);
                if (method.ReturnParameter is not null)
                {
                    Debug.Assert(method.ReturnParameter.Attributes.All(attr => attr.StartsWith("[return:", StringComparison.Ordinal)), $"Method '{method.Name}' has return attributes with the wrong target.");
                    writer.WriteAttributes(method.ReturnParameter);
                }
                writer.WriteMethodSignature(method);
                writer.WriteLine();
                writer.OpenBlock();
                writer.WriteMethodBody(method);
                writer.CloseBlock();
            }

            writer.CloseBlock();
        }
    }
}

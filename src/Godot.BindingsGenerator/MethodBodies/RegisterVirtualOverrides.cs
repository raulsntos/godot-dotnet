using System.CodeDom.Compiler;
using System.Collections.Generic;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class RegisterVirtualOverrides : MethodBody
{
    private readonly TypeInfo _type;

    private readonly List<(MethodInfo Method, GodotMethodInfo EngineMethod)> _virtualMethods;

    public override bool RequiresUnsafeCode => true;

    public RegisterVirtualOverrides(TypeInfo type, List<(MethodInfo Method, GodotMethodInfo EngineMethod)> virtualMethods)
    {
        _type = type;
        _virtualMethods = virtualMethods;
    }

    public override void Write(MethodBase owner, IndentedTextWriter writer)
    {
        if (_type.BaseType is not null)
        {
            writer.WriteLine($"{_type.BaseType.FullNameWithGlobal}.RegisterVirtualOverrides(context);");
        }
        foreach (var (method, engineMethod) in _virtualMethods)
        {
            writer.Write($"context.BindVirtualMethodOverride(MethodName.{method.Name}, ");
            writer.Write($"static ({_type.FullNameWithGlobal} __instance");
            if (method.Parameters.Count > 0)
            {
                writer.Write(", ");
            }

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var parameter = method.Parameters[i];
                string escapedParameterName = SourceCodeWriter.EscapeIdentifier(parameter.Name);
                if (parameter.Type.IsPointerType)
                {
                    writer.Write(KnownTypes.SystemIntPtr.FullNameWithGlobal);
                }
                else
                {
                    writer.Write(parameter.Type.FullNameWithGlobal);
                }
                writer.Write($" {escapedParameterName}");
                if (i < method.Parameters.Count - 1)
                {
                    writer.Write(", ");
                }
            }

            writer.WriteLine(") =>");
            writer.WriteLine('{');
            writer.Indent++;

            if (method.ReturnParameter is not null)
            {
                writer.Write("return ");
                if (method.ReturnParameter.Type.IsPointerType)
                {
                    writer.Write($"({KnownTypes.SystemIntPtr.FullNameWithGlobal})");
                }
            }
            writer.Write($"__instance.{method.Name}(");
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var parameter = method.Parameters[i];
                string escapedParameterName = SourceCodeWriter.EscapeIdentifier(parameter.Name);
                if (parameter.Type.IsPointerType)
                {
                    writer.Write($"({parameter.Type.FullNameWithGlobal})");
                }
                writer.Write(escapedParameterName);
                if (i < method.Parameters.Count - 1)
                {
                    writer.Write(", ");
                }
            }
            writer.WriteLine(");");

            writer.Indent--;
            writer.WriteLine("});");
        }
    }
}

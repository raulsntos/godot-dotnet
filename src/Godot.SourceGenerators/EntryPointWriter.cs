using System;

namespace Godot.SourceGenerators;

internal static class EntryPointWriter
{
    public static void Write(IndentedStringBuilder sb, AssemblySpec spec)
    {
        if (!spec.DisableGodotEntryPointGeneration)
        {
            // Only disable runtime marshalling if we're generating the entry-point;
            // otherwise, just let the user decide whether they want to or not.
            sb.AppendLine("[assembly: global::System.Runtime.CompilerServices.DisableRuntimeMarshalling]");
            sb.AppendLine();
        }

        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        sb.AppendLine($"namespace {spec.Name};");
        sb.AppendLine();

        if (!spec.DisableGodotEntryPointGeneration)
        {
            sb.AppendLine("static partial class Main");
            sb.OpenBlock();

            WriteInitializer(sb);
            WriteTerminator(sb);
            WriteInit(sb);

            sb.CloseBlock();
        }

        sb.AppendLine("internal static class ClassDBExtensions");
        sb.OpenBlock();

        WriteInitializeUserTypes(sb, spec);
        WriteDeinitializeUserTypes(sb, spec);

        sb.CloseBlock();
    }

    private static void WriteInitializer(IndentedStringBuilder sb)
    {
        sb.AppendLine("internal static void InitializeTypes(global::Godot.Bridge.InitializationLevel level)");
        sb.OpenBlock();

        sb.AppendLine("ClassDBExtensions.InitializeUserTypes(level);");

        sb.CloseBlock();
    }

    private static void WriteTerminator(IndentedStringBuilder sb)
    {

        sb.AppendLine("internal static void DeinitializeTypes(global::Godot.Bridge.InitializationLevel level)");
        sb.OpenBlock();

        sb.AppendLine("ClassDBExtensions.DeinitializeUserTypes(level);");

        sb.CloseBlock();
    }

    private static void WriteInit(IndentedStringBuilder sb)
    {
        sb.AppendLine("""[global::System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "init")]""");
        sb.AppendLine("internal static bool Init(nint getProcAddress, nint library, nint initialization)");
        sb.OpenBlock();

        sb.AppendLine("global::Godot.Bridge.GodotBridge.Initialize(getProcAddress, library, initialization, config =>");

        sb.AppendLine("{");
        sb.Indent++;

        sb.AppendLine("config.SetMinimumLibraryInitializationLevel(global::Godot.Bridge.InitializationLevel.Scene);");
        sb.AppendLine("config.RegisterInitializer(InitializeTypes);");
        sb.AppendLine("config.RegisterTerminator(DeinitializeTypes);");

        sb.Indent--;
        sb.AppendLine("});");

        sb.AppendLine("return true;");

        sb.CloseBlock();
    }

    private static void WriteInitializeUserTypes(IndentedStringBuilder sb, AssemblySpec spec)
    {
        sb.AppendLine("internal static void InitializeUserTypes(global::Godot.Bridge.InitializationLevel level)");
        sb.OpenBlock();

        if (spec.Types.Count > 0)
        {
            sb.AppendLine("if (level != global::Godot.Bridge.InitializationLevel.Scene)");
            sb.OpenBlock();
            sb.AppendLine("return;");
            sb.CloseBlock();

            foreach (var classSpec in spec.Types)
            {
                WriteRegisterClass(sb, classSpec);
            }
        }

        sb.CloseBlock();
    }

    private static void WriteDeinitializeUserTypes(IndentedStringBuilder sb, AssemblySpec spec)
    {
        sb.AppendLine("internal static void DeinitializeUserTypes(global::Godot.Bridge.InitializationLevel level)");
        sb.OpenBlock();

        if (spec.Types.Count > 0)
        {
            sb.AppendLine("if (level != global::Godot.Bridge.InitializationLevel.Scene)");
            sb.OpenBlock();
            sb.AppendLine("return;");
            sb.CloseBlock();

            // Classes are unregistered automatically.
        }

        sb.CloseBlock();
    }

    private static void WriteRegisterClass(IndentedStringBuilder sb, GodotRegistrationSpec spec)
    {
        sb.Append("global::Godot.Bridge.GodotRegistry.");
        sb.Append(spec.RegistrationKind switch
        {
            GodotRegistrationSpec.Kind.Class => "RegisterClass",
            GodotRegistrationSpec.Kind.RuntimeClass => "RegisterRuntimeClass",
            GodotRegistrationSpec.Kind.VirtualClass => "RegisterVirtualClass",
            GodotRegistrationSpec.Kind.AbstractClass => "RegisterAbstractClass",
            GodotRegistrationSpec.Kind.InternalClass => "RegisterInternalClass",
            var kind => throw new InvalidOperationException($"Invalid registration kind '{kind}'."),
        });
        sb.AppendLine($"<{spec.FullyQualifiedSymbolName}>({spec.FullyQualifiedSymbolName}.BindMembers);");
    }
}

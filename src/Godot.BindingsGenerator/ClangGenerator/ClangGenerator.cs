using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using ClangSharp;
using ClangSharp.Interop;
using Godot.BindingsGenerator.Logging;

namespace Godot.BindingsGenerator;

internal static class ClangGenerator
{
    public static void Generate(string inputFilePath, string outputDirectoryPath, string? testOutputDirectoryPath, ILogger? logger = null)
    {
        logger ??= ConsoleLogger.Instance;

        logger.LogInformation($"Clang version: {clang.getClangVersion()}");
        logger.LogInformation($"ClangSharp version: {clangsharp.getVersion()}");

        const string Language = "c";
        const string Std = "";

        const string NamespaceName = "Godot.NativeInterop";

        const PInvokeGeneratorConfigurationOptions ConfigOptions =
            PInvokeGeneratorConfigurationOptions.GenerateLatestCode |
            PInvokeGeneratorConfigurationOptions.GenerateDisableRuntimeMarshalling |
            PInvokeGeneratorConfigurationOptions.GenerateMultipleFiles |
            PInvokeGeneratorConfigurationOptions.GenerateUnixTypes |
            PInvokeGeneratorConfigurationOptions.GenerateFileScopedNamespaces |
            PInvokeGeneratorConfigurationOptions.GenerateHelperTypes |
            PInvokeGeneratorConfigurationOptions.GenerateTestsXUnit |
            PInvokeGeneratorConfigurationOptions.LogExclusions;

        string outputLocation = Path.Join(outputDirectoryPath, "GDExtensionInterface");
        string testOutputLocation = !string.IsNullOrWhiteSpace(testOutputDirectoryPath)
            ? Path.Join(testOutputDirectoryPath, "GDExtensionInterface")
            : "";

        // Ensure directories exist.
        Directory.CreateDirectory(outputLocation);
        if (!string.IsNullOrWhiteSpace(testOutputLocation))
        {
            Directory.CreateDirectory(testOutputLocation);
        }

        var config = new PInvokeGeneratorConfiguration(Language, Std, NamespaceName, outputLocation, null, PInvokeGeneratorOutputMode.CSharp, ConfigOptions)
        {
            TestOutputLocation = testOutputLocation,
            WithAccessSpecifiers = new Dictionary<string, AccessSpecifier>()
            {
                ["*"] = AccessSpecifier.Internal,
            },
            RemappedNames = new Dictionary<string, string>()
            {
                ["char"] = "byte",
                ["char32_t"] = "uint",
                ["GDExtensionVariantPtr"] = "Godot.NativeInterop.NativeGodotVariant*",
                ["GDExtensionConstVariantPtr"] = "Godot.NativeInterop.NativeGodotVariant*",
                ["GDExtensionUninitializedVariantPtr"] = "Godot.NativeInterop.NativeGodotVariant*",
                ["GDExtensionStringNamePtr"] = "Godot.NativeInterop.NativeGodotStringName*",
                ["GDExtensionConstStringNamePtr"] = "Godot.NativeInterop.NativeGodotStringName*",
                ["GDExtensionUninitializedStringNamePtr"] = "Godot.NativeInterop.NativeGodotStringName*",
                ["GDExtensionStringPtr"] = "Godot.NativeInterop.NativeGodotString*",
                ["GDExtensionConstStringPtr"] = "Godot.NativeInterop.NativeGodotString*",
                ["GDExtensionUninitializedStringPtr"] = "Godot.NativeInterop.NativeGodotString*",
                ["GDExtensionObjectPtr"] = "void*",
                ["GDExtensionConstObjectPtr"] = "void*",
                ["GDExtensionUninitializedObjectPtr"] = "void*",
                ["GDExtensionTypePtr"] = "void*",
                ["GDExtensionConstTypePtr"] = "void*",
                ["GDExtensionUninitializedTypePtr"] = "void*",
                ["GDExtensionMethodBindPtr"] = "void*",
                ["GDExtensionInt"] = "long",
                ["GDExtensionBool"] = "bool",
                ["GDObjectInstanceID"] = "ulong",
                ["GDExtensionRefPtr"] = "void*",
                ["GDExtensionConstRefPtr"] = "void*",
                ["GDExtensionClassLibraryPtr"] = "void*",
            },
        };

        const CXTranslationUnit_Flags TranslationUnitFlags =
            CXTranslationUnit_Flags.CXTranslationUnit_IncludeAttributedTypes |
            CXTranslationUnit_Flags.CXTranslationUnit_VisitImplicitAttributes;

        List<string> clangCommandLineArgsList =
        [
            $"--language={Language}",
        ];
        if (!string.IsNullOrWhiteSpace(Std))
        {
            clangCommandLineArgsList.Add($"--std={Std}");
        }
        clangCommandLineArgsList.Add("-Wno-pragma-once-outside-header");

        if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "clang",
                    ArgumentList = { "-print-resource-dir" },
                    RedirectStandardOutput = true,
                },
            };
            process.Start();
            process.WaitForExit();

            string clangResourceDir = process.StandardOutput.ReadToEnd().Trim();
            logger.LogDebug($"Clang resource dir: {clangResourceDir}");

            clangCommandLineArgsList.Add($"--include-directory={clangResourceDir}/include");
        }

        if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "xcrun",
                    ArgumentList = { "--show-sdk-path" },
                    RedirectStandardOutput = true,
                },
            };
            process.Start();
            process.WaitForExit();

            string sdkPath = process.StandardOutput.ReadToEnd().Trim();
            logger.LogDebug($"Mac OS SDK Path: {sdkPath}");

            clangCommandLineArgsList.Add($"--include-directory={sdkPath}/usr/include");
        }

        string[] clangCommandLineArgs = clangCommandLineArgsList.ToArray();

        using var generator = new PInvokeGenerator(config);

        int parseErrorCount = 0;

        var translationUnitError = CXTranslationUnit.TryParse(generator.IndexHandle, inputFilePath, clangCommandLineArgs, [], TranslationUnitFlags, out var handle);

        if (translationUnitError != CXErrorCode.CXError_Success)
        {
            throw new InvalidOperationException($"Parsing failed for '{inputFilePath}' due to '{translationUnitError}'.");
        }
        else if (handle.NumDiagnostics != 0)
        {
            for (uint i = 0; i < handle.NumDiagnostics; ++i)
            {
                using var diagnostic = handle.GetDiagnostic(i);

                string message = diagnostic.Format(CXDiagnostic.DefaultDisplayOptions).ToString();

                switch (diagnostic.Severity)
                {
                    case CXDiagnosticSeverity.CXDiagnostic_Note:
                        logger.LogInformation(message);
                        break;

                    case CXDiagnosticSeverity.CXDiagnostic_Warning:
                        logger.LogWarning(message);
                        break;

                    case CXDiagnosticSeverity.CXDiagnostic_Error:
                        logger.LogError(message);
                        parseErrorCount++;
                        break;

                    case CXDiagnosticSeverity.CXDiagnostic_Fatal:
                        logger.LogCritical(message);
                        parseErrorCount++;
                        break;
                }
            }
        }

        if (parseErrorCount > 0)
        {
            throw new InvalidOperationException($"Parsing finished with {parseErrorCount} errors for '{inputFilePath}'. Check for errors above.");
        }

        using var translationUnit = TranslationUnit.GetOrCreate(handle);
        Debug.Assert(translationUnit is not null);

        int generateErrorCount = 0;

        generator.GenerateBindings(translationUnit, inputFilePath, clangCommandLineArgs, TranslationUnitFlags);

        if (generator.Diagnostics.Count != 0)
        {
            foreach (var diagnostic in generator.Diagnostics)
            {
                switch (diagnostic.Level)
                {
                    case DiagnosticLevel.Info:
                        logger.LogInformation(diagnostic.ToString());
                        break;

                    case DiagnosticLevel.Warning:
                        logger.LogWarning(diagnostic.ToString());
                        break;

                    case DiagnosticLevel.Error:
                        logger.LogError(diagnostic.ToString());
                        generateErrorCount++;
                        break;
                }
            }
        }

        if (generateErrorCount > 0)
        {
            throw new InvalidOperationException($"Generating bindings finished with {generateErrorCount} errors for '{inputFilePath}'. Check for errors above.");
        }

        // Generate GDExtensionInterface struct that contains all the interop methods
        // and the method in GodotBridge that initializes it using the LoadProcAddress method.
        GenerateGDExtensionInterfaceMethods(translationUnit, generator, outputLocation);
    }

    private static void GenerateGDExtensionInterfaceMethods(TranslationUnit translationUnit, PInvokeGenerator generator, string outputLocation)
    {
        using var stream = File.Create(Path.Join(outputLocation, "GDExtensionInterface.cs"));
        using var streamWriter = new StreamWriter(stream);
        using var writer = new IndentedTextWriter(streamWriter);

        using var stream2 = File.Create(Path.Join(outputLocation, "GodotBridge.GDExtensionInterface.cs"));
        using var streamWriter2 = new StreamWriter(stream2);
        using var writer2 = new IndentedTextWriter(streamWriter2);

        writer.WriteLine(generator.Config.HeaderText);
        writer2.WriteLine(generator.Config.HeaderText);

        writer2.WriteLine("using Godot.NativeInterop;");
        writer2.WriteLine();

        writer.WriteLine($"namespace {generator.Config.DefaultNamespace};");
        writer.WriteLine();

        writer2.WriteLine($"namespace Godot.Bridge;");
        writer2.WriteLine();

        writer.WriteLine("internal unsafe partial struct GDExtensionInterface");
        writer.WriteLine('{');
        writer.Indent++;

        writer2.WriteLine("partial class GodotBridge");
        writer2.WriteLine('{');
        writer2.Indent++;

        writer2.WriteLine("private unsafe static void InitializeGDExtensionInterface()");
        writer2.WriteLine('{');
        writer2.Indent++;

        writer2.WriteLine("// Clear the interface, in case it was previously initialized.");
        writer2.WriteLine("_gdextensionInterface = default;");
        writer2.WriteLineNoTabs("");

        foreach (var cursor in translationUnit.TranslationUnitDecl.CursorChildren)
        {
            if (cursor is TypeDecl typedef
                && typedef.TypeForDecl is { IsPointerType: true, PointeeType.Kind: CXTypeKind.CXType_FunctionProto }
                && typedef.TypeForDecl.PointeeType is FunctionProtoType functionType)
            {
                string comment = cursor.Handle.RawCommentText.CString;
                if (string.IsNullOrWhiteSpace(comment))
                {
                    // If the typedef has no comment we can't determine the name of the function, so skipping.
                    continue;
                }

                // Parse doxygen comment.
                ReadOnlySpan<char> functionName = default;
                bool isDeprecated = false;
                ReadOnlySpan<char> deprecationMessage = default;
                foreach (var line in comment.AsSpan().EnumerateLines())
                {
                    if (line.StartsWith(" * @name "))
                    {
                        functionName = line.Slice(" * @name ".Length).Trim();
                    }
                    else if (line.StartsWith(" * @deprecated "))
                    {
                        isDeprecated = true;
                        deprecationMessage = line.Slice(" * @deprecated ".Length).Trim();
                    }
                }

                if (functionName.IsEmpty)
                {
                    // Could not find the name of the function in the comment.
                    continue;
                }

                if (isDeprecated)
                {
                    if (!deprecationMessage.IsEmpty)
                    {
                        writer.WriteLine($"""[global::System.Obsolete("Deprecated {deprecationMessage}")]""");
                    }
                    else
                    {
                        writer.WriteLine("[global::System.Obsolete]");
                    }
                }
                writer.WriteLine($"""[NativeTypeName("{typedef.Name}")]""");
                writer.Write("public ");
                AppendFunctionPointerType(writer, cursor, functionType, generator);
                writer.Write(' ');
                writer.Write(functionName);
                writer.WriteLine(';');

                if (isDeprecated)
                {
                    writer2.WriteLineNoTabs("#pragma warning disable CS0618 // Method is obsolete but we still need to load the function pointer.");
                }
                writer2.Write($"_gdextensionInterface.{functionName} = (");
                AppendFunctionPointerType(writer2, cursor, functionType, generator);
                writer2.WriteLine($")LoadProcAddress(\"{functionName}\"u8);");
                if (isDeprecated)
                {
                    writer2.WriteLineNoTabs("#pragma warning restore CS0618");
                }
            }
        }

        writer2.Indent--;
        writer2.WriteLine('}');

        writer2.Indent--;
        writer2.WriteLine('}');

        writer.Indent--;
        writer.WriteLine('}');

        static void AppendFunctionPointerType(TextWriter writer, Cursor cursor, FunctionProtoType functionType, PInvokeGenerator generator)
        {
            writer.Write("delegate* unmanaged");
            switch (functionType.CallConv)
            {
                case CXCallingConv.CXCallingConv_C:
                    writer.Write("[Cdecl]");
                    break;

                default:
                    throw new NotSupportedException($"Calling convention '{functionType.CallConv}' not supported.");
            }

            writer.Write('<');
            foreach (var paramType in functionType.ParamTypes)
            {
                writer.Write(GetTypeName(generator, cursor, paramType));
                writer.Write(", ");
            }
            writer.Write(GetTypeName(generator, cursor, functionType.ReturnType));
            writer.Write('>');
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetTypeName")]
    private extern static string GetTypeNameInternal(PInvokeGenerator generator, Cursor? cursor, Cursor? context, ClangSharp.Type type, bool ignoreTransparentStructsWhereRequired, bool isTemplate, out string nativeTypeName);

    private static string GetTypeName(PInvokeGenerator generator, Cursor cursor, ClangSharp.Type type)
    {
        return GetTypeNameInternal(generator, cursor, null, type, false, false, out _);
    }
}

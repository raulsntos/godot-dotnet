using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot.BindingsGeneration;
using Godot.BindingsGeneration.ApiDump;
using Godot.Common.CodeAnalysis;
using Godot.Tasks.Logging;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Godot.Tasks;

/// <summary>
/// MSBuild task to generate bindings for GDExtensions used by the project.
/// </summary>
public class GenerateGDExtensionBindingsTask : Task
{
    /// <summary>
    /// Paths and names of the extension API files to generate bindings for.
    /// </summary>
    [Required]
    public required ITaskItem[] ExtensionApiFiles { get; set; }

    /// <summary>
    /// References to add to the assembly compilation. It must include 'Godot.Bindings'.
    /// </summary>
    [Required]
    public required ITaskItem[] ReferencePath { get; set; }

    /// <summary>
    /// Path to the directory where the C# bindings will be generated.
    /// </summary>
    [Required]
    public required string OutputPath { get; set; }

    /// <summary>
    /// Path to the generated assembly containing the bindings for the GDExtensions.
    /// This assembly will be referenced by the project to allow using the GDExtension APIs in C# code.
    /// If the assembly is not generated due to errors or because no GDExtension APIs were found,
    /// this property will be <see langword="null"/>.
    /// </summary>
    [Output]
    public string? GeneratedAssemblyPath { get; set; }

    /// <summary>
    /// Write the generated source files to disk.
    /// </summary>
    public bool EmitGeneratedFiles { get; set; }

    /// <summary>
    /// Execute the MSBuild task.
    /// </summary>
    /// <returns><see langword="true"/> if the task was successful.</returns>
    public override bool Execute()
    {
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new ThrowingAssertionListener());

        try
        {
            List<SyntaxTree> syntaxTrees = [];

            var coreApi = LoadEmbeddedCoreApi();
            if (coreApi is null)
            {
                Log.LogError("Failed to load embedded 'extension_api.json' for core API.");
                return false;
            }

            foreach (var extensionApiFile in ExtensionApiFiles)
            {
                string extensionApiPath = extensionApiFile.GetMetadata("FullPath");
                string extensionName = extensionApiFile.GetMetadata("ExtensionName");

                if (!File.Exists(extensionApiPath))
                {
                    Log.LogError($"Extension API file does not exist: '{extensionApiPath}'.");
                    return false;
                }

                if (string.IsNullOrEmpty(extensionName))
                {
                    Log.LogError("Extension name must not be empty.");
                    return false;
                }

                var logger = new MSBuildTaskLogger(Log);

                var extensionApi = LoadExtensionApi(extensionApiPath);
                if (extensionApi is null)
                {
                    Log.LogError($"Failed to deserialize extension API from '{extensionApiPath}'.");
                    return false;
                }

                Log.LogMessage(MessageImportance.High, $"Generating bindings for GDExtension '{extensionName}' ({extensionApiPath}).");

                // Ensure the name is a valid identifier because it will be used as the namespace for the generated code.
                extensionName = IdentifierUtils.SanitizeName(extensionName);

                BindingsGeneratorOptions options = new()
                {
                    Namespace = $"GDExtensionBindings.{extensionName}",
                };

                string projectOutputPath = Path.Combine(OutputPath, extensionName);

                // Clean output directory first.
                if (Directory.Exists(projectOutputPath))
                {
                    Directory.Delete(projectOutputPath, recursive: true);
                }

                var writerFactory = new GDExtensionGeneratorWriterFactory(syntaxTrees)
                {
                    // Only provide the project output path if emitting generated files is enabled.
                    // Otherwise, we don't want the writer to write any files to disk, only generate
                    //  the syntax trees for compilation
                    ProjectOutputPath = EmitGeneratedFiles ? projectOutputPath : null,
                };

                BindingsGenerator.GenerateGDExtensionBindings(coreApi, extensionApi, writerFactory, options, logger);
            }

            if (syntaxTrees.Count == 0)
            {
                Log.LogWarning("No GDExtension APIs were processed, skipping bindings generation.");
                return true;
            }

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithAllowUnsafe(true)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithDeterministic(true)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
                {
                    // Module initializer is only intended to be used in application code
                    // or advanced source generator scenarios. This generated assembly
                    // matches the advanced source generator scenario.
                    ["CA2255"] = ReportDiagnostic.Suppress,
                });

            var references = ReferencePath.Select(r => MetadataReference.CreateFromFile(r.ItemSpec));

            var compilation = CSharpCompilation.Create("GDExtensionBindings", syntaxTrees, references, compilationOptions);

            // Ensure output directory exists.
            Directory.CreateDirectory(OutputPath);

            GeneratedAssemblyPath = Path.Combine(OutputPath, "GDExtensionBindings.dll");

            string tempAssemblyPath = $"{GeneratedAssemblyPath}.tmp";

            // Emit to a temporary file first, then atomically replace the destination.
            // This avoids races with consumers reading the final path during compilation.
            using (var assemblyStream = File.Create(tempAssemblyPath))
            {
                var result = compilation.Emit(assemblyStream);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        switch (diagnostic.Severity)
                        {
                            case DiagnosticSeverity.Warning:
                                Log.LogWarning($"Warning reported when compiling GDExtensionBindings assembly: {diagnostic}");
                                break;

                            case DiagnosticSeverity.Error:
                                Log.LogError($"Error reported when compiling GDExtensionBindings assembly: {diagnostic}");
                                break;

                            case DiagnosticSeverity.Info:
                                Log.LogMessage(MessageImportance.High, $"Info reported when compiling GDExtensionBindings assembly: {diagnostic}");
                                break;
                        }
                    }

                    return false;
                }
            }

            File.Move(tempAssemblyPath, GeneratedAssemblyPath, overwrite: true);

            return !Log.HasLoggedErrors;
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to generate bindings for GDExtension: {e}");
            return false;
        }
    }

    private static GodotApi? LoadExtensionApi(string path)
    {
        using var stream = File.OpenRead(path);
        return GodotApi.Deserialize(stream);
    }

    private static GodotApi? LoadEmbeddedCoreApi()
    {
        var assembly = typeof(GenerateGDExtensionBindingsTask).Assembly;
        using var stream = assembly.GetManifestResourceStream("Godot.Tasks.extension_api.json");
        if (stream is null)
        {
            return null;
        }

        return GodotApi.Deserialize(stream);
    }

    private sealed class GDExtensionGeneratorWriterFactory : IBindingsGeneratorWriterFactory
    {
        public bool SupportsTestOutput => false;
        public string? ProjectOutputPath { get; init; }

        private readonly List<SyntaxTree> _syntaxTrees;

        public GDExtensionGeneratorWriterFactory(List<SyntaxTree> syntaxTrees)
        {
            _syntaxTrees = syntaxTrees;
        }

        public TextWriter CreateWriter(string pathHint, bool isTestOutput)
        {
            // If we have a valid output path, write the generated file to disk.
            // Otherwise, we'll just generate the syntax trees to compile the assembly.
            string? filePath = !string.IsNullOrEmpty(ProjectOutputPath)
                ? Path.Combine(ProjectOutputPath, pathHint)
                : null;

            return new GDExtensionGeneratorTextWriter(_syntaxTrees, filePath);
        }

        private sealed class GDExtensionGeneratorTextWriter : StringWriter
        {
            private readonly List<SyntaxTree> _syntaxTrees;
            private readonly string? _filePath;
            private bool _disposed;

            public GDExtensionGeneratorTextWriter(List<SyntaxTree> syntaxTrees, string? filePath)
            {
                _syntaxTrees = syntaxTrees;
                _filePath = filePath;
            }

            private void WriteType()
            {
                string text = ToString();

                _syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Latest)));

                // If a file path was provided, write the generated text to disk.
                if (!string.IsNullOrEmpty(_filePath))
                {
                    // Ensure the output directory exists.
                    string? directoryPath = Path.GetDirectoryName(_filePath);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    File.WriteAllText(_filePath, text);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                // When the writer is disposed, we consider the generator finished writing the type
                // and we can take the in-memory text from this StringWriter, and use it to create
                // the syntax trees for compilation, and optionally write the generated code to disk.
                WriteType();

                base.Dispose(disposing);
            }
        }
    }

    private sealed class ThrowingAssertionListener : TraceListener
    {
        public override void Write(string? message) { }
        public override void WriteLine(string? message) { }

        public override void Fail(string? message, string? detailMessage)
        {
            throw new InvalidOperationException($"Assertion failed: {message} {detailMessage}");
        }
    }
}

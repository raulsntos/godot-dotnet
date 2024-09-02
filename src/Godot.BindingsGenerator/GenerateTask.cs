using System.IO;
using Godot.BindingsGenerator.Logging;
using Godot.BindingsGenerator.ApiDump;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Godot.BindingsGenerator;

/// <summary>
/// MSBuild task to execute the Godot bindings generator.
/// </summary>
public class GenerateTask : Task
{
    /// <summary>
    /// Path to the extension API dump JSON file.
    /// </summary>
    [Required]
    public required string ExtensionApiPath { get; set; }

    /// <summary>
    /// Path to the extension interface header file.
    /// </summary>
    [Required]
    public required string ExtensionInterfacePath { get; set; }

    /// <summary>
    /// Path to the directory where the C# bindings will be generated.
    /// </summary>
    [Required]
    public required string OutputPath { get; set; }

    /// <summary>
    /// Path to the directory where the C# tests will be generated.
    /// </summary>
    public string? TestOutputPath { get; set; }

    public override bool Execute()
    {
        // Clean output directories first.
        if (Directory.Exists(OutputPath))
        {
            Directory.Delete(OutputPath, recursive: true);
        }
        if (Directory.Exists(TestOutputPath))
        {
            Directory.Delete(TestOutputPath, recursive: true);
        }

        var logger = new MSBuildTaskLogger(Log);

        ClangGenerator.Generate(ExtensionInterfacePath, OutputPath, TestOutputPath, logger);

        using var stream = File.OpenRead(ExtensionApiPath);
        var api = GodotApi.Deserialize(stream);

        if (api is null || string.IsNullOrWhiteSpace(api.Header.VersionFullName))
        {
            Log.LogError("Error parsing the Godot extension API dump.");
            return false;
        }

        Log.LogMessage(MessageImportance.High, $"Generating C# bindings for '{api.Header.VersionFullName}'.");

        BindingsGenerator.Generate(api, OutputPath, logger: logger);

        return !Log.HasLoggedErrors;
    }
}

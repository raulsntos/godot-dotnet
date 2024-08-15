using Godot.EditorIntegration.Build;

namespace Godot.EditorIntegration.Export;

internal sealed class ExportBuildOptions
{
    /// <summary>
    /// The options to use when building the .NET project.
    /// </summary>
    public required PublishOptions PublishOptions { get; init; }

    /// <summary>
    /// Name of the .NET assembly for the main project.
    /// </summary>
    public required string AssemblyName { get; init; }

    /// <summary>
    /// Name of the target platform, using the Godot OS names.
    /// </summary>
    public required string GodotPlatform { get; init; }

    /// <summary>
    /// Name of the target architecture, using the Godot architecture names.
    /// </summary>
    public required string GodotArchitecture { get; init; }

    /// <summary>
    /// Get the name of the directory that contains the exported data (i.e.: .NET assemblies).
    /// Matches the method <c>get_exported_data_directory_name</c> in the <c>dotnet</c> module,
    /// because that's where the module will lookup for the DLLs in an exported game.
    /// </summary>
    public string GetExportedDataDirectoryName()
    {
        return $"data_{AssemblyName}_{GodotPlatform}_{GodotArchitecture}";
    }
}

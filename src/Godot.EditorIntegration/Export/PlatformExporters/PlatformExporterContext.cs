using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Godot.EditorIntegration.Build;

namespace Godot.EditorIntegration.Export;

internal sealed class PlatformExporterContext
{
    /// <summary>
    /// The <see cref="EditorExportPlugin"/> that is invoked by Godot
    /// to begin the export process of .NET projects.
    /// Use it to add files to the PCK during the export process.
    /// </summary>
    public required DotNetExportPlugin ExportPlugin { get; init; }

    private readonly List<ExportBuildOptions> _buildOptions = [];

    /// <summary>
    /// List of build options that will be used to build the .NET project
    /// in the export process.
    /// Some platforms require multiple builds because they need to export to
    /// multiple processor architectures or runtime identifiers.
    /// </summary>
    internal IReadOnlyList<ExportBuildOptions> BuildOptions => _buildOptions.AsReadOnly();

    /// <summary>
    /// List of temporary directories created for this export process that need
    /// to be removed after exporting.
    /// </summary>
    internal List<string> TemporaryDirectories { get; } = [];

    /// <summary>
    /// Name of the Godot platform that is the target of this export process.
    /// Use <see cref="GodotPlatform"/> to determine which platform it is from this name.
    /// </summary>
    public required string TargetPlatform { get; init; }

    /// <summary>
    /// Godot features enabled by the active export preset for this export process.
    /// </summary>
    public required ReadOnlySet<string> PresetFeatures { get; init; }

    /// <summary>
    /// Build configuration requested by the export preset.
    /// </summary>
    public required string BuildConfiguration { get; init; }

    /// <summary>
    /// Indicates whether the export preset requested to include debug symbols.
    /// </summary>
    public required bool IncludeDebugSymbols { get; init; }

    /// <summary>
    /// Add the build options that will be used to build the .NET project as part of
    /// the export process.
    /// Some platform exporters may add multiple exports if they need to build the project
    /// multiple times for different configurations or architectures.
    /// </summary>
    /// <param name="options">The options for the added export build.</param>
    public void AddBuild(ExportBuildOptions options)
    {
        _buildOptions.Add(options);
    }

    /// <summary>
    /// Create a temporary directory and add it to <see cref="TemporaryDirectories"/>.
    /// </summary>
    /// <returns>The absolute path to the created temporary directory.</returns>
    public string CreateTemporaryDirectory(string? prefix = null)
    {
        string directory = Directory.CreateTempSubdirectory(prefix).FullName;
        TemporaryDirectories.Add(directory);
        return directory;
    }
}

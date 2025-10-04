namespace Godot.UpgradeAssistant.Cli.Services;

internal sealed class UpgradeServiceConfiguration
{
    /// <summary>
    /// Path to the 'project.godot' file.
    /// </summary>
    public required string GodotProjectFilePath { get; init; }

    /// <summary>
    /// Path to the .NET solution file.
    /// </summary>
    public required string DotNetSolutionFilePath { get; init; }

    /// <summary>
    /// Path to the .NET project file.
    /// </summary>
    public required string DotNetProjectFilePath { get; init; }

    /// <summary>
    /// Target Godot version to upgrade to.
    /// </summary>
    public required SemVer TargetGodotVersion { get; init; }

    /// <summary>
    /// Indicates whether the Godot .NET packages should be used instead of GodotSharp
    /// for a target Godot version that supports both.
    /// </summary>
    public bool EnableGodotDotNetPreview { get; init; }

    /// <summary>
    /// Exporter that will write the results summary.
    /// </summary>
    public IExporter? Exporter { get; init; }

    /// <summary>
    /// The path to the exported results summary.
    /// If <see langword="null"/> the export step is skipped.
    /// </summary>
    public string? ExportFilePath { get; init; }

    /// <summary>
    /// When enabled only an analysis is performed, skipping the upgrade step.
    /// </summary>
    public bool IsDryRun { get; init; }
}

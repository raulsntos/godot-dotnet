using System.IO;

namespace Godot.UpgradeAssistant.Cli.Commands;

internal sealed class AssistantCommandOptions
{
    public required FileInfo GodotProject { get; init; }

    public required FileInfo DotNetSolution { get; init; }

    public required FileInfo DotNetProject { get; init; }

    public SemVer TargetGodotVersion { get; init; }

    public bool EnableGodotDotNetPreview { get; init; }

    public FileInfo? ExportFilePath { get; init; }
}

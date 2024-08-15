namespace Godot.EditorIntegration.Build;

internal sealed class BuildLoggerOption
{
    public required string TypeFullName { get; set; }

    public required string AssemblyPath { get; set; }

    public required string LogsPath { get; set; }
}

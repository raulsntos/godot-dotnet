namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes a Godot project.
/// </summary>
public readonly struct GodotProject
{
    /// <summary>
    /// Path to the <c>project.godot</c> file that this instance was parsed from.
    /// </summary>
    public string ProjectFilePath { get; init; }

    /// <summary>
    /// Godot version that the project uses.
    /// </summary>
    public SemVer GodotVersion { get; init; }
}

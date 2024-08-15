namespace Godot.EditorIntegration.Build;

/// <summary>
/// Describes a diagnostic produced during a compilation.
/// </summary>
internal sealed class BuildDiagnostic
{
    /// <summary>
    /// The diagnostic level or severity.
    /// </summary>
    public DiagnosticSeverity Severity { get; set; }

    /// <summary>
    /// The absolute path to the file that the diagnostic originates from.
    /// </summary>
    public string? File { get; set; }

    /// <summary>
    /// Line number with 1-based indexing.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Column number with 1-based indexing.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// The diagnostic's unique identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// A message that describes the diagnostic.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// The absolute path to the project file that the diagnostic originates from.
    /// </summary>
    public string? ProjectFile { get; set; }
}

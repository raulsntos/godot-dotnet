namespace Godot.UpgradeAssistant;

/// <summary>
/// Action that reports an analysis result that needs to be fixed to successfully
/// upgrade the project.
/// </summary>
/// <param name="analysis">The analysis result to be reported.</param>
public delegate void ReportAnalysisAction(AnalysisResult analysis);

/// <summary>
/// Context for an active analysis. An analyzer can use the context to register results.
/// </summary>
public class AnalysisContext
{
    private readonly ReportAnalysisAction _reporter;

    /// <summary>
    /// Workspace that is being analyzed.
    /// </summary>
    public required WorkspaceInfo Workspace { get; init; }

    /// <summary>
    /// The target Godot version that the workspace will be upgraded to.
    /// It must be within the range of <see cref="Constants.MinSupportedGodotVersion"/>
    /// and <see cref="Constants.LatestGodotVersion"/>.
    /// </summary>
    public required SemVer TargetGodotVersion { get; init; }

    /// <summary>
    /// Indicates if the target should use the new GDExtension-based Godot .NET bindings.
    /// </summary>
    public required bool IsGodotDotNetEnabled { get; init; }

    /// <summary>
    /// Construct a <see cref="AnalysisContext"/>.
    /// </summary>
    /// <param name="reporter">Action to report analysis results that need to be fixed.</param>
    internal AnalysisContext(ReportAnalysisAction reporter)
    {
        _reporter = reporter;
    }

    /// <summary>
    /// Report an analysis result that needs to be fixed to successfully upgrade the project.
    /// </summary>
    /// <param name="analysis">The analysis result to be reported.</param>
    public void ReportAnalysis(AnalysisResult analysis)
    {
        _reporter(analysis);
    }
}

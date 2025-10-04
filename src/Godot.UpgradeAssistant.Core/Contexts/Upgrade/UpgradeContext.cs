using System;
using System.Collections.Immutable;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Action that registers an upgrade action to the list of actions that will be used
/// to upgrade the project.
/// </summary>
/// <param name="upgrade">The upgrade action that upgrades the project.</param>
/// <param name="analysis">
/// The subset of the provided anlysis results being handled by <paramref name="upgrade"/>.
/// </param>
public delegate void RegisterUpgradeAction(UpgradeAction upgrade, ImmutableArray<AnalysisResult> analysis);

/// <summary>
/// Context for an active upgrade. An upgrader can use the context to report fixes.
/// </summary>
public class UpgradeContext
{
    private readonly RegisterUpgradeAction _reporter;

    /// <summary>
    /// Workspace that is being upgraded.
    /// </summary>
    public required WorkspaceInfo Workspace { get; init; }

    /// <summary>
    /// The target Godot version that the workspace is being upgraded to.
    /// It must be within the range of <see cref="Constants.MinSupportedGodotVersion"/>
    /// and <see cref="Constants.LatestGodotVersion"/>.
    /// </summary>
    public required SemVer TargetGodotVersion { get; init; }

    /// <summary>
    /// Indicates if the target should use the new GDExtension-based Godot .NET bindings.
    /// </summary>
    public required bool IsGodotDotNetEnabled { get; init; }

    /// <summary>
    /// Construct a <see cref="UpgradeContext"/>.
    /// </summary>
    /// <param name="reporter">Action to report upgrade actions that fix the encountered analysis results.</param>
    internal UpgradeContext(RegisterUpgradeAction reporter)
    {
        _reporter = reporter;
    }

    /// <summary>
    /// Add an upgrade action to the list of actions that will be used to upgrade the project.
    /// </summary>
    /// <param name="upgrade">The upgrade action that upgrades the project.</param>
    /// <param name="analysis">
    /// The subset of the provided anlysis results being handled by <paramref name="upgrade"/>.
    /// </param>
    public void RegisterUpgrade(UpgradeAction upgrade, ImmutableArray<AnalysisResult> analysis)
    {
        if (analysis.IsDefaultOrEmpty)
        {
            throw new ArgumentException(SR.Argument_UpgradeActionMustFixAtLeastOneAnalysisResult, nameof(analysis));
        }

        _reporter(upgrade, analysis);
    }

    /// <summary>
    /// Add an upgrade action to the list of actions that will be used to upgrade the project.
    /// </summary>
    /// <param name="upgrade">The upgrade that upgrades the project.</param>
    /// <param name="analysis">
    /// The provided analysis result being handled by <paramref name="upgrade"/>.
    /// </param>
    public void RegisterUpgrade(UpgradeAction upgrade, AnalysisResult analysis)
    {
        _reporter(upgrade, [analysis]);
    }
}

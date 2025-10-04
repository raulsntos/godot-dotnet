using System.Collections.Immutable;
using System.Diagnostics;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes an upgrade action to fix the result of the analysis.
/// The upgrade action may fix one or multiple analysis results.
/// </summary>
public sealed class UpgradeFix
{
    /// <summary>
    /// The upgrade action that applies the fix.
    /// </summary>
    public UpgradeAction UpgradeAction { get; }

    /// <summary>
    /// The set of analysis results that are fixed by the upgrade action.
    /// </summary>
    public ImmutableArray<AnalysisResult> AnalysisResults { get; }

    /// <summary>
    /// Constructs a <see cref="UpgradeFix"/> for the specified upgrade action
    /// that corresponds to the specified analysis result.
    /// </summary>
    /// <param name="upgrade">The upgrade action to execute when applying the fix.</param>
    /// <param name="analysisResult">The analysis result that the fix corresponds to.</param>
    public UpgradeFix(UpgradeAction upgrade, AnalysisResult analysisResult) : this(upgrade, [analysisResult]) { }

    /// <summary>
    /// Constructs a <see cref="UpgradeFix"/> for the specified upgrade action
    /// that corresponds to the specified analysis results.
    /// </summary>
    /// <param name="upgrade">The upgrade action to execute when applying the fix.</param>
    /// <param name="analysisResults">The analysis results that the fix corresponds to.</param>
    public UpgradeFix(UpgradeAction upgrade, ImmutableArray<AnalysisResult> analysisResults)
    {
        Debug.Assert(!analysisResults.IsDefaultOrEmpty);
        UpgradeAction = upgrade;
        AnalysisResults = analysisResults;
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes a problem encountered by the assistant.
/// Contains the analysis result that describes the problem, the upgrade actions
/// that could fix it, and the one that was applied to fix it (if any).
/// </summary>
public readonly struct ProblemSummaryData
{
    /// <summary>
    /// The analysis result that reported this problem.
    /// </summary>
    public AnalysisResult AnalysisResult { get; }

    /// <summary>
    /// Upgrade actions that can handle this problem.
    /// </summary>
    public IReadOnlyCollection<UpgradeAction> UpgradeActions { get; }

    /// <summary>
    /// The upgrade action that was applied to fix this problem
    /// or <see langword="null"/> if no upgrade action was applied.
    /// </summary>
    public UpgradeAction? UpgradeActionApplied { get; }

    /// <summary>
    /// Indicates if this analysis result can be fixed by at least one upgrade action.
    /// </summary>
    public bool HasFixAvailable => UpgradeActions.Count > 0;

    /// <summary>
    /// Indicates if this analysis result has been fixed by applying an upgrade action.
    /// </summary>
    [MemberNotNullWhen(true, nameof(UpgradeActionApplied))]
    public bool HasFixApplied => UpgradeActionApplied is not null;

    /// <summary>
    /// Constructs a <see cref="ProblemSummaryData"/>.
    /// </summary>
    /// <param name="analysisResult">The analysis result that reported the problem.</param>
    /// <param name="upgradeActions">The upgrade actions that can handle this problem.</param>
    /// <param name="upgradeActionApplied">The upgrade action that was applied to fix the problem.</param>
    public ProblemSummaryData(AnalysisResult analysisResult, IReadOnlyCollection<UpgradeAction> upgradeActions, UpgradeAction? upgradeActionApplied = null)
    {
        AnalysisResult = analysisResult;
        UpgradeActions = upgradeActions;
        UpgradeActionApplied = upgradeActionApplied;
    }
}

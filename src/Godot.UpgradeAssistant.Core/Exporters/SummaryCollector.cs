using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Builder that adds data from the assistant steps to create a <see cref="Summary"/>.
/// </summary>
internal sealed class SummaryCollector
{
    private readonly Dictionary<AnalysisResult, List<UpgradeAction>> _analysisSummaries = [];
    private readonly Dictionary<UpgradeAction, List<AnalysisResult>> _upgradeSummaries = [];
    private readonly Dictionary<AnalysisResult, UpgradeAction> _appliedUpgrades = [];

    private readonly DateTime _timeStamp;

    private readonly SemVer _targetGodotVersion;

    /// <summary>
    /// Constructs a <see cref="SummaryCollector"/>.
    /// </summary>
    /// <param name="timeStamp">
    /// Date and time when the assistant was executed.
    /// </param>
    /// <param name="targetGodotVersion">
    /// The target version of Godot that the assistant tried to upgrade to.
    /// </param>
    public SummaryCollector(DateTime timeStamp, SemVer targetGodotVersion)
    {
        _timeStamp = timeStamp;
        _targetGodotVersion = targetGodotVersion;
    }

    /// <summary>
    /// Adds an <see cref="AnalysisResult"/> to the summary.
    /// </summary>
    /// <param name="analysisResult">The analysis result to include in the summary.</param>
    public void AddAnalysisResult(AnalysisResult analysisResult)
    {
        AddEmptySummary(_analysisSummaries, analysisResult);
    }

    /// <summary>
    /// Adds an <see cref="UpgradeAction"/> to the summary.
    /// </summary>
    /// <param name="upgradeAction">The upgrade action to include in the summary.</param>
    /// <param name="analysisResults">The analysis results that the upgrade action fixes.</param>
    public void AddUpgradeAction(UpgradeAction upgradeAction, ImmutableArray<AnalysisResult> analysisResults)
    {
        foreach (var analysisResult in analysisResults)
        {
            UpdateSummaries(_analysisSummaries, analysisResult, upgradeAction);
        }

        UpdateSummaries(_upgradeSummaries, upgradeAction, analysisResults);
    }

    /// <summary>
    /// Adds an <see cref="UpgradeAction"/> that was applied to the summary.
    /// </summary>
    /// <param name="upgradeAction">The upgrade action to include in the summary.</param>
    /// <param name="analysisResult">The analysis result that was fixed by the upgrade action.</param>
    public void ApplyUpgradeAction(UpgradeAction upgradeAction, AnalysisResult analysisResult)
    {
        if (_appliedUpgrades.ContainsKey(analysisResult))
        {
            throw new ArgumentException(SR.Argument_AnalysisResultAlreadyFixed, nameof(analysisResult));
        }

        _appliedUpgrades[analysisResult] = upgradeAction;
    }

    private static void AddEmptySummary<TKey, TValue>(Dictionary<TKey, List<TValue>> summaries, TKey key) where TKey : notnull
    {
        if (!summaries.ContainsKey(key))
        {
            summaries[key] = [];
        }
    }

    private static void UpdateSummaries<TKey, TValue>(Dictionary<TKey, List<TValue>> summaries, TKey key, IReadOnlyCollection<TValue> values) where TKey : notnull
    {
        if (!summaries.TryGetValue(key, out var storingValues))
        {
            storingValues = [];
        }
        storingValues.AddRange(values);
        summaries[key] = storingValues;
    }

    private static void UpdateSummaries<TKey, TValue>(Dictionary<TKey, List<TValue>> summaries, TKey key, TValue value) where TKey : notnull
    {
        if (!summaries.TryGetValue(key, out var storingValues))
        {
            storingValues = [];
        }
        storingValues.Add(value);
        summaries[key] = storingValues;
    }

    /// <summary>
    /// Builds the <see cref="Summary"/> from the data collected.
    /// </summary>
    /// <returns>The built summary.</returns>
    public Summary ToSummary()
    {
        int i = 0;
        var problems = new ProblemSummaryData[_analysisSummaries.Count];
        foreach (var (analysis, upgrades) in _analysisSummaries)
        {
            _appliedUpgrades.TryGetValue(analysis, out var appliedUpgrade);
            var summary = new ProblemSummaryData(analysis, upgrades, appliedUpgrade);
            summary = FilterUpgradeActionsThatAreNotAFix(summary);
            problems[i++] = summary;
        }

        return new Summary(_timeStamp, _targetGodotVersion, problems);
    }

    private static ProblemSummaryData FilterUpgradeActionsThatAreNotAFix(ProblemSummaryData summary)
    {
        List<UpgradeAction> filteredUpgrades = [];

        foreach (var upgrade in summary.UpgradeActions)
        {
            if (!upgrade.FixesDiagnostic)
            {
                continue;
            }

            filteredUpgrades.Add(upgrade);
        }

        UpgradeAction? appliedUpgrade = summary.UpgradeActionApplied;
        if (appliedUpgrade is not { FixesDiagnostic: true })
        {
            appliedUpgrade = null;
        }

        return new ProblemSummaryData(summary.AnalysisResult, filteredUpgrades, appliedUpgrade);
    }
}

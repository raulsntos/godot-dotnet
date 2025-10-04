using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Step that collects upgrade actions to fix the problems encountered in <see cref="AnalyzeStep"/>.
/// </summary>
public sealed class CollectUpgradesStep : AssistantStepBase<CollectUpgradesStep.Configuration>
{
    /// <summary>
    /// Configuration for the collect upgrades step.
    /// </summary>
    public sealed class Configuration : AssistantStepConfiguration
    {
        /// <summary>
        /// The providers that will be used to obtain upgrade fixes for the provided analysis results.
        /// </summary>
        public required IReadOnlyCollection<IUpgradeProvider> UpgradeProviders { get; init; }

        /// <summary>
        /// The analysis results collected in <see cref="AnalyzeStep"/>.
        /// </summary>
        public required IReadOnlyCollection<AnalysisResult> AnalysisResults { get; init; }
    }

    /// <summary>
    /// Event arguments for the <see cref="UpgradeActionRegistered"/> event.
    /// </summary>
    public sealed class UpgradeActionReportedEventArgs : EventArgs
    {
        /// <summary>
        /// The upgrade action that was reported to fix analysis results.
        /// </summary>
        public required UpgradeAction UpgradeAction { get; init; }

        /// <summary>
        /// The collection of analysis results that will be fixed by <see cref="UpgradeAction"/>.
        /// </summary>
        public required ImmutableArray<AnalysisResult> AnalysisResults { get; init; }
    }

    private readonly UpgradeContext _context;

    private readonly ConcurrentBag<UpgradeFix> _fixes = [];

    /// <summary>
    /// Event raised when an upgrade provider registers an upgrade action as a fix
    /// for one the provided analysis results.
    /// </summary>
    public event EventHandler<UpgradeActionReportedEventArgs>? UpgradeActionRegistered;

    /// <summary>
    /// Constructs a <see cref="CollectUpgradesStep"/>.
    /// </summary>
    /// <param name="configuration">Provides the configuration for the step.</param>
    public CollectUpgradesStep(Configuration configuration) : base(configuration)
    {
        _context = new UpgradeContext(ReportUpgrade)
        {
            Workspace = Config.Workspace,
            TargetGodotVersion = Config.TargetGodotVersion,
            IsGodotDotNetEnabled = Config.IsGodotDotNetEnabled,
        };
    }

    /// <summary>
    /// Gets the upgrade fixes collected when executing the step.
    /// </summary>
    /// <returns>The collected upgrade fixes.</returns>
    public IReadOnlyCollection<UpgradeFix> GetAllRegisteredUpgradeFixes()
    {
        return _fixes;
    }

    private void ReportUpgrade(UpgradeAction upgrade, ImmutableArray<AnalysisResult> analysis)
    {
        Debug.Assert(!analysis.IsDefaultOrEmpty);

        UpgradeActionRegistered?.Invoke(this, new UpgradeActionReportedEventArgs()
        {
            UpgradeAction = upgrade,
            AnalysisResults = analysis,
        });

        _fixes.Add(new UpgradeFix(upgrade, analysis));
    }

    /// <inheritdoc/>
    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _fixes.Clear();

        foreach (var upgrader in Config.UpgradeProviders)
        {
            var analysisResults = Config.AnalysisResults
                .Where(result => upgrader.CanHandle(result.Id))
                .ToArray();
            if (analysisResults.Length == 0)
            {
                continue;
            }

            var previousUpgrader = UpgradeAction.CurrentUpgradeProvider;
            try
            {
                UpgradeAction.CurrentUpgradeProvider = upgrader;
                await upgrader.UpgradeAsync(_context, analysisResults, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                UpgradeAction.CurrentUpgradeProvider = previousUpgrader;
            }
        }
    }

    /// <summary>
    /// Executes the step and returns the collected upgrade fixes.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the step.
    /// </param>
    /// <returns>Task that completes when the step finishes its execution.</returns>
    public async Task<IReadOnlyCollection<UpgradeFix>> RunAndGetFixesAsync(CancellationToken cancellationToken = default)
    {
        await RunAsync(cancellationToken).ConfigureAwait(false);
        return GetAllRegisteredUpgradeFixes();
    }
}

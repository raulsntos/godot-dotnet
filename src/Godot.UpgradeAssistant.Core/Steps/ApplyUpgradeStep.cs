using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Step that applies the upgrade fixes collected in <see cref="CollectUpgradesStep"/>.
/// </summary>
public sealed class ApplyUpgradeStep : AssistantStepBase<ApplyUpgradeStep.Configuration>
{
    /// <summary>
    /// Configuration for the apply upgrade step.
    /// </summary>
    public sealed class Configuration : AssistantStepConfiguration
    {
        /// <summary>
        /// The upgraded fixes collected in <see cref="CollectUpgradesStep"/> that will be applied.
        /// </summary>
        public IReadOnlyCollection<UpgradeFix> UpgradeFixes { get; init; } = [];
    }

    private readonly ConcurrentBag<UpgradeFix> _appliedFixes = [];

    /// <summary>
    /// Constructs a <see cref="ApplyUpgradeStep"/>.
    /// </summary>
    /// <param name="configuration">Provides the configuration for the step.</param>
    public ApplyUpgradeStep(Configuration configuration) : base(configuration) { }

    /// <summary>
    /// Gets the upgrade fixes applied when executing the step.
    /// </summary>
    /// <returns>The applied upgrade fixes.</returns>
    public IReadOnlyCollection<UpgradeFix> GetAllAppliedUpgradeFixes()
    {
        return _appliedFixes;
    }

    /// <inheritdoc/>
    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var upgradeAllContext = new UpgradeAllContext();

        foreach (var fix in Config.UpgradeFixes)
        {
            // It's preferable to apply fixes in bulk, check if the current fix can be
            // applied in bulk and if so register it with the upgrade-all context.
            if (upgradeAllContext.TryRegisterUpgrade(fix))
            {
                // Skip individual apply changes, the fix will be applied by the upgrade-all context,
                // but we still add it to the collection of applied fixes.
                _appliedFixes.Add(fix);
                continue;
            }

            await fix.UpgradeAction.ApplyChanges(Config.Workspace, cancellationToken).ConfigureAwait(false);
            _appliedFixes.Add(fix);
        }

        // Apply all the fixes that were registered to apply in bulk.
        await upgradeAllContext.ApplyAllChanges(Config.Workspace, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the step and returns the applied upgrade fixes.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the step.
    /// </param>
    /// <returns>Task that completes when the step finishes its execution.</returns>
    public async Task<IReadOnlyCollection<UpgradeFix>> RunAndGetAppliedFixesAsync(CancellationToken cancellationToken = default)
    {
        await RunAsync(cancellationToken).ConfigureAwait(false);
        return GetAllAppliedUpgradeFixes();
    }
}

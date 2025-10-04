using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Context for an active upgrade to register fixes to be applied in bulk.
/// </summary>
internal class UpgradeAllContext
{
    private readonly Dictionary<IUpgradeAllProvider, Queue<UpgradeFix>> _registeredFixes = [];

    /// <summary>
    /// Check if the given upgrade fix can be applied in bulk.
    /// </summary>
    /// <param name="fix">The upgrade fix to check.</param>
    /// <returns><see langword="true"/> if the upgrade fix can be applied in bulk.</returns>
    public static bool CanHandle(UpgradeFix fix)
    {
        return fix.UpgradeAction.UpgradeProvider is IUpgradeAllProvider;
    }

    /// <summary>
    /// If the upgrade fix can be applied in bulk, register it within the context
    /// to merge it with other upgrade fixes using its upgrade provider.
    /// </summary>
    /// <param name="fix">The upgrade fix to register.</param>
    /// <returns>Whether the fix can be applied in bulk and was registered.</returns>
    public bool TryRegisterUpgrade(UpgradeFix fix)
    {
        if (!CanHandle(fix))
        {
            return false;
        }

        RegisterUpgradeCore(fix);
        return true;
    }

    /// <summary>
    /// Register the upgrade fix within the context to merge it with other upgrade fixes
    /// using its upgrade provider.
    /// </summary>
    /// <param name="fix">The upgrade fix to register.</param>
    /// <exception cref="ArgumentException">
    /// The upgrade fix can't be applied in bulk. Check with <see cref="CanHandle(UpgradeFix)"/>.
    /// </exception>
    public void RegisterUpgrade(UpgradeFix fix)
    {
        if (!CanHandle(fix))
        {
            throw new ArgumentException(SR.Argument_UpgradeFixDoesNotSupportMerging, nameof(fix));
        }

        RegisterUpgradeCore(fix);
    }

    private void RegisterUpgradeCore(UpgradeFix fix)
    {
        var upgrader = fix.UpgradeAction.UpgradeProvider as IUpgradeAllProvider;
        Debug.Assert(upgrader is not null);

        if (!_registeredFixes.TryGetValue(upgrader, out var fixesForProvider))
        {
            _registeredFixes[upgrader] = fixesForProvider = [];
        }

        fixesForProvider.Enqueue(fix);
    }

    /// <summary>
    /// Merge the registered upgrade fixes using their upgrade provider,
    /// resulting in an upgrade action for each group of merged fixes.
    /// Then, apply all the upgrade actions obtained.
    /// </summary>
    /// <param name="workspace">Workspace that is being upgraded.</param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>Task that completes when the changes have been applied.</returns>
    public async Task ApplyAllChanges(WorkspaceInfo workspace, CancellationToken cancellationToken = default)
    {
        foreach (var (upgradeProvider, fixesForProvider) in _registeredFixes)
        {
            var upgradeAction = upgradeProvider.MergeFixes(workspace, fixesForProvider);
            if (upgradeAction is not null)
            {
                await upgradeAction.ApplyChanges(workspace, cancellationToken).ConfigureAwait(false);
                continue;
            }

            // We couldn't merge the fixes, just apply the changes individually.
            foreach (var fix in fixesForProvider)
            {
                await fix.UpgradeAction.ApplyChanges(workspace, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

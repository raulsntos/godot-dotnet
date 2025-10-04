using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="UpgradeAction"/> constructed for a .NET workspace.
/// </summary>
public abstract class DotNetWorkspaceUpgradeAction : DotNetUpgradeAction
{
    /// <summary>
    /// Construct a <see cref="DotNetWorkspaceUpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    protected DotNetWorkspaceUpgradeAction(string id, string title) : base(id, title) { }

    /// <inheritdoc/>
    public sealed override async Task ApplyChanges(WorkspaceInfo workspace, CancellationToken cancellationToken = default)
    {
        var changedSolution = await GetChangedSolutionAsync(cancellationToken).ConfigureAwait(false);
        if (changedSolution is null)
        {
            // No changes to apply.
            return;
        }

        if (workspace.DotNetWorkspace is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_DotNetWorkspaceUpgradeActionCannotBeAppliedWithoutWorkspace);
        }

        workspace.DotNetWorkspace.Workspace.TryApplyChanges(changedSolution);
    }

    /// <summary>
    /// Applies the changes to the solution and returns the changed solution.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>Task that completes after the changes have been applied.</returns>
    public abstract Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken = default);
}

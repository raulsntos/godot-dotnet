using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="UpgradeAction"/> constructed for a .NET workspace
/// that applies fixes to a <see cref="ProjectRootElement"/> instance.
/// </summary>
internal sealed class DotNetProjectRootUpgradeAction : DotNetUpgradeAction
{
    private readonly Func<CancellationToken, Task<ProjectRootElement?>> _createChangedProjectRoot;

    /// <summary>
    /// Construct a <see cref="DotNetProjectRootUpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    /// <param name="createChangedProjectRoot">
    /// Function that applies the fix and returns the changed project root.
    /// </param>
    public DotNetProjectRootUpgradeAction(string id, string title, Func<CancellationToken, Task<ProjectRootElement?>> createChangedProjectRoot) : base(id, title)
    {
        _createChangedProjectRoot = createChangedProjectRoot;
    }

    /// <inheritdoc/>
    public override async Task ApplyChanges(WorkspaceInfo workspace, CancellationToken cancellationToken = default)
    {
        var changedProjectRoot = await _createChangedProjectRoot(cancellationToken).ConfigureAwait(false);
        if (changedProjectRoot is null)
        {
            // No changes to apply.
            return;
        }

        if (workspace.DotNetWorkspace is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_DotNetWorkspaceUpgradeActionCannotBeAppliedWithoutWorkspace);
        }

        workspace.DotNetWorkspace.SaveProjectRootElement(changedProjectRoot);
    }
}

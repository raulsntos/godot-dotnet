using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="UpgradeAction"/> constructed for a .NET workspace
/// that applies fixes to a <see cref="SolutionModel"/> instance.
/// </summary>
internal sealed class DotNetSolutionModelUpgradeAction : DotNetUpgradeAction
{
    private readonly Func<CancellationToken, Task<SolutionModel?>> _createChangedSolution;

    private readonly ISolutionSerializer? _solutionSerializer;

    /// <summary>
    /// Construct a <see cref="DotNetSolutionModelUpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    /// <param name="createChangedSolution">
    /// Function that applies the fix and returns the changed solution model.
    /// </param>
    /// <param name="solutionSerializer">
    /// Serializer that will be used to save the solution. If <see langword="null"/>,
    /// the serializer will be retrieved from the solution moniker.
    /// </param>
    public DotNetSolutionModelUpgradeAction(string id, string title, Func<CancellationToken, Task<SolutionModel?>> createChangedSolution, ISolutionSerializer? solutionSerializer = null) : base(id, title)
    {
        _createChangedSolution = createChangedSolution;
        _solutionSerializer = solutionSerializer;
    }

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

        string? solutionMoniker = workspace.DotNetWorkspace.Solution.FilePath;
        Debug.Assert(!string.IsNullOrEmpty(solutionMoniker));

        var serializer = _solutionSerializer
            ?? SolutionSerializers.GetSerializerByMoniker(solutionMoniker);

        if (serializer is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_DotNetSolutionModelUpgradeActionCannotBeAppliedWithoutSolutionSerializer);
        }

        await workspace.DotNetWorkspace.SaveSolutionModelAsync(changedSolution, serializer, cancellationToken).ConfigureAwait(false);
    }

    private async Task<SolutionModel?> GetChangedSolutionAsync(CancellationToken cancellationToken = default)
    {
        var changedSolution = await _createChangedSolution(cancellationToken).ConfigureAwait(false);
        return changedSolution;
    }
}

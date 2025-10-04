using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="UpgradeAction"/> constructed for a .NET workspace
/// that applies fixes to a .NET solution.
/// </summary>
public sealed class DotNetSolutionUpgradeAction : DotNetWorkspaceUpgradeAction
{
    private readonly Func<CancellationToken, Task<Solution?>> _createChangedSolution;

    /// <summary>
    /// Construct a <see cref="DotNetSolutionUpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    /// <param name="createChangedSolution">
    /// Function that applies the fix and returns the changed solution.
    /// </param>
    public DotNetSolutionUpgradeAction(string id, string title, Func<CancellationToken, Task<Solution?>> createChangedSolution) : base(id, title)
    {
        _createChangedSolution = createChangedSolution;
    }

    /// <inheritdoc/>
    public override async Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken = default)
    {
        var changedSolution = await _createChangedSolution(cancellationToken).ConfigureAwait(false);
        return changedSolution;
    }
}

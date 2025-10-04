using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="UpgradeAction"/> constructed for a .NET workspace
/// that applies fixes to a .NET document.
/// </summary>
public sealed class DotNetDocumentUpgradeAction : DotNetWorkspaceUpgradeAction
{
    private readonly Func<CancellationToken, Task<Document?>> _createChangedDocument;

    /// <summary>
    /// Construct a <see cref="DotNetDocumentUpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    /// <param name="createChangedDocument">
    /// Function that applies the fix and returns the changed document.
    /// </param>
    public DotNetDocumentUpgradeAction(string id, string title, Func<CancellationToken, Task<Document?>> createChangedDocument) : base(id, title)
    {
        _createChangedDocument = createChangedDocument;
    }

    /// <inheritdoc/>
    public override async Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken = default)
    {
        var changedDocument = await _createChangedDocument(cancellationToken).ConfigureAwait(false);
        if (changedDocument is null)
        {
            return null;
        }

        return changedDocument.Project.Solution;
    }
}

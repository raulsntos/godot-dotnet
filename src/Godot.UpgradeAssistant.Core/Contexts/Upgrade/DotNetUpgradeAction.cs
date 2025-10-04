using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="UpgradeAction"/> reported for a .NET workspace.
/// </summary>
public abstract class DotNetUpgradeAction : UpgradeAction
{
    /// <summary>
    /// Constructs a <see cref="DotNetUpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    protected DotNetUpgradeAction(string id, string title) : base(id, title) { }

    /// <summary>
    /// Constructs a <see cref="DotNetWorkspaceUpgradeAction"/> from a .NET code action.
    /// </summary>
    /// <param name="action">The .NET code action to create the upgrade action for.</param>
    public static DotNetWorkspaceUpgradeAction FromCodeAction(CodeAction action)
    {
        action.TryDeconstruct(out action, out var metadata);

        Debug.Assert(action.EquivalenceKey is not null);

        return metadata is null
            ? new DotNetSolutionUpgradeAction(action.EquivalenceKey, action.Title, CreateChangedSolution)
            : new DotNetSolutionUpgradeAction(action.EquivalenceKey, action.Title, CreateChangedSolution)
            {
                FixesDiagnostic = metadata.FixesDiagnostic,
            };

        async Task<Solution?> CreateChangedSolution(CancellationToken cancellationToken = default)
        {
            // The code action application is expected to apply the changes by executing the operations in
            // 'GetOperationsAsync'. We only care about changes where the first operation is an 'ApplyChangesOperation'
            // to change the text in the solution, and any remaining changes are deferred computation changes so
            // they can be ignored.
            var operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
            var applyChangesOperation = operations.OfType<ApplyChangesOperation>().SingleOrDefault();
            if (applyChangesOperation is null)
            {
                return null;
            }

            return applyChangesOperation.ChangedSolution;
        }
    }
}

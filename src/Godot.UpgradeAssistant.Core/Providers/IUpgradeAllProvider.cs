using System.Collections.Generic;
using System.Threading;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Implements a provider that can apply changes to a project to upgrade it
/// and can merge all the registered fixes into a single upgrade action.
/// </summary>
internal interface IUpgradeAllProvider : IUpgradeProvider
{
    /// <summary>
    /// Merge all the fixes registered by the provider in
    /// <see cref="IUpgradeProvider.UpgradeAsync(UpgradeContext, IEnumerable{AnalysisResult}, CancellationToken)"/>.
    /// </summary>
    /// <param name="workspace">Workspace that is being upgraded.</param>
    /// <param name="allFixes">The collection of fixes to merge.</param>
    /// <returns>An upgrade action that applies all fixes at once.</returns>
    public UpgradeAction? MergeFixes(WorkspaceInfo workspace, IReadOnlyCollection<UpgradeFix> allFixes);
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Implements a provider that can apply changes to a project to upgrade it.
/// </summary>
public interface IUpgradeProvider
{
    /// <summary>
    /// Check if the upgrader can handle the rule with the given id.
    /// </summary>
    /// <param name="ruleId">Rule Id to check for.</param>
    /// <returns><see langword="true"/> if the upgrader can handle the rule.</returns>
    public bool CanHandle(string ruleId);

    /// <summary>
    /// Apply the necessary changes to the project to upgrade.
    /// </summary>
    /// <param name="context">Context for the upgrade operation.</param>
    /// <param name="analysisResults">
    /// The results of the analysis that can be handled by this upgrader.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>Task that completes when the upgrade has been applied.</returns>
    public Task UpgradeAsync(UpgradeContext context, IEnumerable<AnalysisResult> analysisResults, CancellationToken cancellationToken = default);
}

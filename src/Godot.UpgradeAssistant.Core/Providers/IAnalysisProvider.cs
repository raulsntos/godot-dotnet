using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Implements a provider that can analyze a project to find parts that
/// need to be changed to upgrade it.
/// </summary>
public interface IAnalysisProvider
{
    /// <summary>
    /// Analyze the project to find instances that need to be upgraded.
    /// If the analyzer finds something that needs to be upgraded it
    /// reports the result of the analysis so it can be upgraded by an
    /// upgrade provider.
    /// </summary>
    /// <param name="context">Context for the analysis operation.</param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>Task that completes when the upgrade has been applied.</returns>
    public Task AnalyzeAsync(AnalysisContext context, CancellationToken cancellationToken = default);
}

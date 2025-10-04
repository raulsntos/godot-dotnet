using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Represents a writer that can write the summary of the results to an output file.
/// </summary>
public interface IExporter
{
    /// <summary>
    /// Write the specified <paramref name="summary"/> to the output files in the specified
    /// <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="summary">
    /// The summary of the results that will be written to the output files.
    /// </param>
    /// <param name="outputPath">
    /// The path to store the output files written by the exporter.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>Task that completes when the exporter finishes writing the summary.</returns>
    public Task ExportAsync(Summary summary, string outputPath, CancellationToken cancellationToken = default);
}

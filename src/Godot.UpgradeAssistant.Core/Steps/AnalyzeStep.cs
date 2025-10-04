using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Step that analyzes the project in search of problems that need to be fix to upgrade.
/// </summary>
public sealed class AnalyzeStep : AssistantStepBase<AnalyzeStep.Configuration>
{
    /// <summary>
    /// Configuration for the analysis step.
    /// </summary>
    public sealed class Configuration : AssistantStepConfiguration
    {
        /// <summary>
        /// The providers that will be used to analyze the project.
        /// </summary>
        public required IReadOnlyCollection<IAnalysisProvider> AnalysisProviders { get; init; }
    }

    /// <summary>
    /// Event arguments for the <see cref="AnalysisResultReported"/> event.
    /// </summary>
    public sealed class AnalysisResultReportedEventArgs : EventArgs
    {
        /// <summary>
        /// The analysis result that was reported.
        /// </summary>
        public required AnalysisResult AnalysisResult { get; init; }
    }

    private readonly AnalysisContext _context;

    private readonly ConcurrentBag<AnalysisResult> _results = [];

    /// <summary>
    /// Event raised when an analysis provider reports an analysis result.
    /// </summary>
    public event EventHandler<AnalysisResultReportedEventArgs>? AnalysisResultReported;

    /// <summary>
    /// Constructs a <see cref="AnalyzeStep"/>.
    /// </summary>
    /// <param name="configuration">Provides the configuration for the step.</param>
    public AnalyzeStep(Configuration configuration) : base(configuration)
    {
        _context = new AnalysisContext(ReportAnalysis)
        {
            Workspace = Config.Workspace,
            TargetGodotVersion = Config.TargetGodotVersion,
            IsGodotDotNetEnabled = Config.IsGodotDotNetEnabled,
        };
    }

    /// <summary>
    /// Gets the analysis results encountered when running the step.
    /// </summary>
    /// <returns>The collected analysis results.</returns>
    public IReadOnlyCollection<AnalysisResult> GetAllReportedAnalysisResults()
    {
        return _results;
    }

    private void ReportAnalysis(AnalysisResult analysis)
    {
        AnalysisResultReported?.Invoke(this, new AnalysisResultReportedEventArgs()
        {
            AnalysisResult = analysis,
        });

        _results.Add(analysis);
    }

    /// <inheritdoc/>
    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _results.Clear();

        foreach (var analyzer in Config.AnalysisProviders)
        {
            await analyzer.AnalyzeAsync(_context, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes the step and returns the collected analysis results.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the step.
    /// </param>
    /// <returns>Task that completes when the step finishes its execution.</returns>
    public async Task<IReadOnlyCollection<AnalysisResult>> RunAndGetResultsAsync(CancellationToken cancellationToken = default)
    {
        await RunAsync(cancellationToken).ConfigureAwait(false);
        return GetAllReportedAnalysisResults();
    }
}

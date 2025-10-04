using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Step that writes a summary of the results to an output file.
/// </summary>
public sealed class ExportStep : AssistantStepBase<ExportStep.Configuration>
{
    /// <summary>
    /// Configuration for the export step.
    /// </summary>
    public sealed class Configuration : AssistantStepConfiguration
    {
        /// <summary>
        /// The exporter that will write the summary of the results.
        /// </summary>
        /// <value></value>
        public required IExporter Exporter { get; init; }

        /// <summary>
        /// The path to the file that will be written by <see cref="Exporter"/>.
        /// </summary>
        /// <value></value>
        public required string OutputPath { get; init; }

        /// <summary>
        /// Date and time at which the assistant was executed.
        /// </summary>
        public DateTime TimeStamp { get; init; }

        /// <summary>
        /// The analysis results found by the assistant.
        /// </summary>
        public IReadOnlyCollection<AnalysisResult> AnalysisResults { get; init; } = [];

        /// <summary>
        /// The upgrade fixes reported by the assistant.
        /// </summary>
        public IReadOnlyCollection<UpgradeFix> UpgradeFixes { get; init; } = [];

        /// <summary>
        /// The upgrade fixes that were applied by the assistant.
        /// </summary>
        public IReadOnlyCollection<UpgradeFix> AppliedUpgradeFixes { get; init; } = [];
    }

    /// <summary>
    /// Constructs a <see cref="ExportStep"/>.
    /// </summary>
    /// <param name="configuration">Provides the configuration for the step.</param>
    public ExportStep(Configuration configuration) : base(configuration) { }

    /// <inheritdoc/>
    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var collector = new SummaryCollector(Config.TimeStamp, Config.TargetGodotVersion);

        foreach (var result in Config.AnalysisResults)
        {
            collector.AddAnalysisResult(result);
        }

        foreach (var fix in Config.UpgradeFixes)
        {
            collector.AddUpgradeAction(fix.UpgradeAction, fix.AnalysisResults);
        }

        foreach (var fix in Config.AppliedUpgradeFixes)
        {
            foreach (var analysis in fix.AnalysisResults)
            {
                collector.ApplyUpgradeAction(fix.UpgradeAction, analysis);
            }
        }

        await Config.Exporter.ExportAsync(collector.ToSummary(), Config.OutputPath, cancellationToken).ConfigureAwait(false);
    }
}

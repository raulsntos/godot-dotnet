using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Godot.UpgradeAssistant.Cli.Logging;
using Microsoft.Build.Execution;
using Microsoft.CodeAnalysis;
using Serilog;

namespace Godot.UpgradeAssistant.Cli.Services;

internal sealed class UpgradeService
{
    private readonly ProviderService _providers;

    public UpgradeService(ProviderService providers)
    {
        _providers = providers;
    }

    public async Task RunAsync(UpgradeServiceConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Log.Information(SR.FormatLog_UpgradingGodotProject(configuration.GodotProjectFilePath, configuration.TargetGodotVersion));

        using var workspace = await OpenGodotWorkspaceAsync(configuration.GodotProjectFilePath, configuration.DotNetSolutionFilePath, configuration.DotNetProjectFilePath, cancellationToken);
        if (workspace is null)
        {
            Log.Error(SR.Log_FailedToOpenWorkspace);
            return;
        }

        // Ensure the providers can't be modified while running the upgrade service.
        _providers.MakeReadOnly();

        IReadOnlyCollection<AnalysisResult> analysisResults = [];
        IReadOnlyCollection<UpgradeFix> upgradeFixes = [];
        IReadOnlyCollection<UpgradeFix> appliedFixes = [];

        var timeStamp = DateTime.Now;

        // Restore Packages step.
        if (workspace.DotNetWorkspace is not null)
        {
            Log.Verbose(SR.Log_RestorePackagesStepStarting);

            var step = new RestorePackagesStep(new()
            {
                Workspace = workspace,
                TargetGodotVersion = configuration.TargetGodotVersion,
                EnableGodotDotNetPreview = configuration.EnableGodotDotNetPreview,
                Logger = new BuildLogger(),
            });
            var result = await step.RunAndGetResultAsync(cancellationToken);

            if (result.Exception is not null)
            {
                Log.Error(SR.FormatLog_RestorePackagesStepThrewAnException(result.Exception.Message));
            }

            if (result.OverallResult == BuildResultCode.Failure)
            {
                Log.Error(SR.Log_RestorePackagesStepFailed);
            }
            else
            {
                Log.Verbose(SR.Log_RestorePackagesStepSuccessful);
            }
        }

        // Analysis step.
        {
            Log.Verbose(SR.FormatLog_AnalyzeStepStarting(_providers.AnalysisProviders.Count));

            var step = new AnalyzeStep(new()
            {
                Workspace = workspace,
                TargetGodotVersion = configuration.TargetGodotVersion,
                EnableGodotDotNetPreview = configuration.EnableGodotDotNetPreview,
                AnalysisProviders = _providers.AnalysisProviders,
            });
            step.AnalysisResultReported += (_, e) =>
            {
                Log.Verbose(SR.FormatLog_AnalyzeStepReportedResult(e.AnalysisResult));
            };

            analysisResults = await step.RunAndGetResultsAsync(cancellationToken);

            Log.Verbose(SR.FormatLog_AnalyzeStepReportedResultsCount(analysisResults.Count));

            if (analysisResults.Count == 0)
            {
                // Nothing to upgrade.
                goto export;
            }
        }

        // Collect Upgrades step.
        {
            Log.Verbose(SR.FormatLog_UpgradeStepStarting(_providers.UpgradeProviders.Count));

            var step = new CollectUpgradesStep(new()
            {
                Workspace = workspace,
                TargetGodotVersion = configuration.TargetGodotVersion,
                EnableGodotDotNetPreview = configuration.EnableGodotDotNetPreview,
                UpgradeProviders = _providers.UpgradeProviders,
                AnalysisResults = analysisResults,
            });
            step.UpgradeActionRegistered += (_, e) =>
            {
                Log.Verbose(SR.FormatLog_UpgradeStepRegisteredAction(e.UpgradeAction));
            };

            upgradeFixes = await step.RunAndGetFixesAsync(cancellationToken);

            Log.Verbose(SR.FormatLog_UpgradeStepRegisteredActionsCount(upgradeFixes.Count));

            if (upgradeFixes.Count == 0)
            {
                // Nothing to upgrade.
                goto export;
            }
        }

        // Apply Upgrade step.
        if (!configuration.IsDryRun)
        {
            Log.Verbose(SR.FormatLog_ApplyStepStarting(upgradeFixes.Count));

            var step = new ApplyUpgradeStep(new()
            {
                Workspace = workspace,
                TargetGodotVersion = configuration.TargetGodotVersion,
                EnableGodotDotNetPreview = configuration.EnableGodotDotNetPreview,
                UpgradeFixes = upgradeFixes,
            });
            appliedFixes = await step.RunAndGetAppliedFixesAsync(cancellationToken);

            Log.Verbose(SR.Log_ApplyStepFinished);
        }

    export:
        // Export step.
        if (configuration.ExportFilePath is not null)
        {
            string outputPath = configuration.ExportFilePath
                .Replace("{TimeStamp}", timeStamp.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture))
                .Replace("{TargetGodotVersion}", configuration.TargetGodotVersion.ToString());

            Log.Verbose(SR.Log_ExportStepStarting);

            var step = new ExportStep(new()
            {
                Workspace = workspace,
                TargetGodotVersion = configuration.TargetGodotVersion,
                EnableGodotDotNetPreview = configuration.EnableGodotDotNetPreview,
                Exporter = configuration.Exporter ?? new HtmlExporter(),
                OutputPath = outputPath,
                TimeStamp = timeStamp,
                AnalysisResults = analysisResults,
                UpgradeFixes = upgradeFixes,
                AppliedUpgradeFixes = appliedFixes,
            });
            await step.RunAsync(cancellationToken);

            Log.Verbose(SR.FormatLog_ExportStepSummaryExportedToFilePath(outputPath));
        }
    }

    private static async Task<WorkspaceInfo?> OpenGodotWorkspaceAsync(string godotProjectFilePath, string dotnetSolutionFilePath, string dotnetProjectFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(godotProjectFilePath))
        {
            Log.Error(SR.FormatLog_GodotProjectFileNotFoundInPath(godotProjectFilePath));
            return null;
        }

        WorkspaceInfo.DotNetWorkspaceDiagnostic += LogWorkspaceDiagnostics;

        try
        {
            return await WorkspaceInfo.OpenAsync(godotProjectFilePath, dotnetSolutionFilePath, dotnetProjectFilePath, cancellationToken);
        }
        catch (Exception e)
        {
            Log.Error(SR.FormatLog_GodotProjectFileUnableToOpen(godotProjectFilePath));
            Log.Debug(e, "Failed to open Godot project workspace.");
            return null;
        }
        finally
        {
            WorkspaceInfo.DotNetWorkspaceDiagnostic -= LogWorkspaceDiagnostics;
        }

        static void LogWorkspaceDiagnostics(object? _, WorkspaceDiagnosticEventArgs e)
        {
            switch (e.Diagnostic.Kind)
            {
                case WorkspaceDiagnosticKind.Failure:
                {
                    Log.Error(e.Diagnostic.Message);
                    break;
                }

                case WorkspaceDiagnosticKind.Warning:
                {
                    Log.Warning(e.Diagnostic.Message);
                    break;
                }

                default:
                {
                    Log.Information(e.Diagnostic.Message);
                    break;
                }
            }
        }
    }
}

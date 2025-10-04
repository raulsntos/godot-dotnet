using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetSolutionConfigurationAnalysisProvider : IAnalysisProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA0005_DotNetSolutionConfiguration;

    public async Task AnalyzeAsync(AnalysisContext context, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Analyzer only applies to .NET workspaces and there isn't one.
            return;
        }

        var solutionModel = await context.Workspace.DotNetWorkspace.OpenSolutionModelAsync(out _, cancellationToken).ConfigureAwait(false);

        if (SolutionDefinesExpectedConfigurations(solutionModel.BuildTypes))
        {
            return;
        }

        context.ReportAnalysis(AnalysisResult.Create(
            id: Rule.Id,
            title: Rule.Title,
            location: new Location(0, 0, 0, 0, context.Workspace.DotNetWorkspace.Solution.FilePath!),
            description: Rule.Description,
            helpUri: null,
            messageFormat: Rule.MessageFormat));
    }

    private static bool SolutionDefinesExpectedConfigurations(IReadOnlyList<string> buildTypes)
    {
        // Build types renamed in https://github.com/godotengine/godot/pull/36865.

        if (buildTypes.Contains("Tools"))
        {
            // Solution still contains a build type from before the renames.
            return false;
        }

        if (buildTypes.Contains("Debug")
         && buildTypes.Contains("ExportDebug")
         && buildTypes.Contains("ExportRelease"))
        {
            // Solution contains all the build types that should be defined after the renames.
            return true;
        }

        return false;
    }
}

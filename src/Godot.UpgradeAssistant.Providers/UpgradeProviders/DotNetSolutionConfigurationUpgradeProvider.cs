using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.SolutionPersistence.Model;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetSolutionConfigurationUpgradeProvider : IUpgradeProvider
{
    public bool CanHandle(string ruleId)
    {
        return ruleId == Descriptors.GUA0005_DotNetSolutionConfiguration.Id;
    }

    public async Task UpgradeAsync(UpgradeContext context, IEnumerable<AnalysisResult> analysisResults, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Upgrader only applies to .NET workspaces and there isn't one.
            return;
        }

        // There should only be one analysis result reported since there should only be one solution.
        var analysisResult = analysisResults.Single();

        var solutionModel = await context.Workspace.DotNetWorkspace.OpenSolutionModelAsync(out var serializer, cancellationToken).ConfigureAwait(false);

        context.RegisterUpgrade(new DotNetSolutionModelUpgradeAction(
            id: nameof(DotNetProjectTfmUpgradeProvider),
            title: SR.GUA0005_DotNetSolutionConfiguration_CodeFix,
            createChangedSolution: _ =>
            {
                // Remove old configuration names if they are defined.
                solutionModel.RemoveBuildType("Tools");
                solutionModel.RemoveBuildType("Release");

                // Ensure all of these configuration names are defined.
                // It's fine if the solution already defines them, duplicates are ignored.
                solutionModel.AddBuildType("Debug");
                solutionModel.AddBuildType("ExportDebug");
                solutionModel.AddBuildType("ExportRelease");

                return Task.FromResult<SolutionModel?>(solutionModel);
            }), analysisResult);
    }
}

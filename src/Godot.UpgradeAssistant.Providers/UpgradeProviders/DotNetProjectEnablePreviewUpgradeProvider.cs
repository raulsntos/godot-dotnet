using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetProjectEnablePreviewUpgradeProvider : IUpgradeProvider
{
    private const string EnablePreviewPropertyName = DotNetProjectEnablePreviewAnalysisProvider.EnablePreviewPropertyName;

    public bool CanHandle(string ruleId)
    {
        return ruleId == Descriptors.GUA0006_DotNetProjectEnablePreview.Id;
    }

    public Task UpgradeAsync(UpgradeContext context, IEnumerable<AnalysisResult> analysisResults, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Upgrader only applies to .NET workspaces and there isn't one.
            return Task.CompletedTask;
        }

        // There should only be one analysis result reported since we only handle the Godot project.
        var analysisResult = analysisResults.Single();

        var projectRoot = context.Workspace.DotNetWorkspace.OpenProjectRootElement();

        var enablePreviewProperties = projectRoot.PropertyGroups
            .Where(pg => string.IsNullOrEmpty(pg.Condition))
            .SelectMany(pg => pg.Properties)
            .Where(p => p.ElementName.Equals(EnablePreviewPropertyName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        context.RegisterUpgrade(new DotNetProjectRootUpgradeAction(
            id: nameof(DotNetProjectTfmUpgradeProvider),
            title: SR.GUA0006_DotNetProjectEnablePreview_CodeFix,
            createChangedProjectRoot: _ =>
            {
                if (enablePreviewProperties.Length > 0)
                {
                    var enablePreviewNode = enablePreviewProperties[^1];
                    enablePreviewNode.Value = "true";
                    return Task.FromResult<ProjectRootElement?>(enablePreviewNode.ContainingProject);
                }

                {
                    var pg = projectRoot.AddPropertyGroup();
                    var enablePreviewNode = pg.AddProperty(EnablePreviewPropertyName, "true");
                    return Task.FromResult<ProjectRootElement?>(enablePreviewNode.ContainingProject);
                }
            }), analysisResult);

        return Task.CompletedTask;
    }
}

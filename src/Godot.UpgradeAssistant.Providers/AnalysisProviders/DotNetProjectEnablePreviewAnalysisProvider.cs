using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetProjectEnablePreviewAnalysisProvider : IAnalysisProvider
{
    internal const string EnablePreviewPropertyName = "EnableGodotDotNetPreview";

    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA0006_DotNetProjectEnablePreview;

    public Task AnalyzeAsync(AnalysisContext context, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Analyzer only applies to .NET workspaces and there isn't one.
            return Task.CompletedTask;
        }

        if (!context.IsGodotDotNetEnabled)
        {
            // Analyzer only applies when Godot .NET is enabled.
            return Task.CompletedTask;
        }

        if (context.TargetGodotVersion > Constants.LastSupportedGodotSharpVersion)
        {
            // The target version only supports Godot .NET, so there's no need
            // to explicitly enable the preview packages anymore.
            return Task.CompletedTask;
        }

        var project = context.Workspace.DotNetWorkspace.Project;
        var projectRoot = context.Workspace.DotNetWorkspace.OpenProjectRootElement();

        var enablePreviewProperties = projectRoot.PropertyGroups
            .Where(pg => string.IsNullOrEmpty(pg.Condition))
            .SelectMany(pg => pg.Properties)
            .Where(p => p.ElementName.Equals(EnablePreviewPropertyName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var property in enablePreviewProperties)
        {
            if (property.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                // New bindings are already enabled.
                return Task.CompletedTask;
            }
        }

        context.ReportAnalysis(AnalysisResult.Create(
            id: Rule.Id,
            title: Rule.Title,
            location: projectRoot.Location,
            description: Rule.Description,
            helpUri: null,
            messageFormat: Rule.MessageFormat,
            // Message Format parameters.
            project.Name));

        return Task.CompletedTask;
    }
}

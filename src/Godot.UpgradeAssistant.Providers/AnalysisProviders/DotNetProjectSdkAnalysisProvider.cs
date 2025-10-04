using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetProjectSdkAnalysisProvider : IAnalysisProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA0002_DotNetProjectSdk;

    public Task AnalyzeAsync(AnalysisContext context, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Analyzer only applies to .NET workspaces and there isn't one.
            return Task.CompletedTask;
        }

        var project = context.Workspace.DotNetWorkspace.Project;
        var projectRoot = context.Workspace.DotNetWorkspace.OpenProjectRootElement();

        if (!context.Workspace.DotNetWorkspace.TryGetGodotSdkVersion(out var sdkVersion))
        {
            // Couldn't get the SDK version, assume unsupported.
            context.ReportAnalysis(AnalysisResult.Create(
                id: Rule.Id,
                title: Rule.Title,
                location: projectRoot.SdkLocation,
                description: Rule.Description,
                helpUri: null,
                messageFormat: Rule.MessageFormat,
                // Message Format parameters.
                project.Name,
                projectRoot.Sdk,
                context.TargetGodotVersion));

            return Task.CompletedTask;
        }

        if (IsSdkSupported(sdkVersion, context.TargetGodotVersion))
        {
            // SDK version already matches the expected version.
            return Task.CompletedTask;
        }

        context.ReportAnalysis(AnalysisResult.Create(
            id: Rule.Id,
            title: Rule.Title,
            location: projectRoot.SdkLocation,
            description: Rule.Description,
            helpUri: null,
            messageFormat: Rule.MessageFormat,
            // Message Format parameters.
            project.Name,
            $"Godot.NET.Sdk/{sdkVersion}",
            context.TargetGodotVersion));

        return Task.CompletedTask;
    }

    private static bool IsSdkSupported(SemVer currentSdkVersion, SemVer requiredSdkVersion)
    {
        // Same or greater versions of Godot.NET.Sdk are supported.
        return currentSdkVersion >= requiredSdkVersion;
    }
}

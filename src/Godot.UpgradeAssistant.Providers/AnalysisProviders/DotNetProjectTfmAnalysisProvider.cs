using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NuGet.Frameworks;
using Serilog;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetProjectTfmAnalysisProvider : IAnalysisProvider
{
    private static DiagnosticDescriptor MainRule =>
        Descriptors.GUA0001_DotNetProjectTfm;

    private static DiagnosticDescriptor PlatformRule =>
        Descriptors.GUA0001_DotNetProjectTfm_Platform;

    public async Task AnalyzeAsync(AnalysisContext context, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Analyzer only applies to .NET workspaces and there isn't one.
            return;
        }

        var project = context.Workspace.DotNetWorkspace.Project;
        var projectRoot = context.Workspace.DotNetWorkspace.OpenProjectRootElement();

        // Get the minimum required TFM for all platforms.
        var minimumRequiredTfm = await TargetFrameworkUtils.GetRequiredTargetFrameworkAsync(context.TargetGodotVersion, null, context.IsGodotDotNetEnabled, cancellationToken).ConfigureAwait(false);
        if (minimumRequiredTfm is null)
        {
            // We could not find the required TFM, let the user fix it manually.
            Log.Warning(SR.FormatLog_UnableToGetRequiredTfm(context.TargetGodotVersion));
            return;
        }

        // To get the base TFM we need to find the TFM property without conditions.
        // But if the value will be upgraded, then take the new minimum required TFM as the base TFM.
        var baseTfmNode = projectRoot.GetTargetFrameworkProperties()
            .FirstOrDefault(tfmNode => !tfmNode.IsConditioned());
        var baseTfm = baseTfmNode is not null
            ? NuGetFramework.Parse(baseTfmNode.Value)
            : null;
        if (baseTfm is null || !DefaultCompatibilityProvider.Instance.IsCompatible(baseTfm, minimumRequiredTfm))
        {
            baseTfm = minimumRequiredTfm;
        }

        var tfmProperties = projectRoot.GetTargetFrameworkProperties();
        foreach (var tfmNode in tfmProperties)
        {
            // Check if the TFM property (or the PropertyGroup it belongs to) has a condition.
            // We only know how to handle the ones without conditions or with a condition that
            // matches the GodotTargetPlatform condition we added in GodotSharp for some platforms,
            // but we still need to report if the TFM is not compatible because it may require
            // user attention.
            bool hasCondition = tfmNode.IsConditioned();
            string? platform = null;
            if (hasCondition)
            {
                tfmNode.ConditionMatchesGodotPlatform(out platform);
            }

            var requiredTfm = await TargetFrameworkUtils.GetRequiredTargetFrameworkAsync(context.TargetGodotVersion, platform, context.IsGodotDotNetEnabled, cancellationToken).ConfigureAwait(false);
            if (requiredTfm is null)
            {
                // We could not find a required TFM,
                // assume we don't need to upgrade it.
                continue;
            }

            var tfms = TargetFrameworkUtils.GetTargetFrameworks(tfmNode);
            foreach (var tfm in tfms)
            {
                // All targeted frameworks must be compatible with the required TFM,
                // otherwise the Godot API package can't be referenced.
                // Also, if the TFM property has a condition but matches the required
                // TFM and the base TFM is or would be upgraded to the required TFM,
                // then it can be removed since it becomes redundant, so we also report
                // in that case.
                if (!DefaultCompatibilityProvider.Instance.IsCompatible(tfm, requiredTfm)
                 || (hasCondition && tfm == requiredTfm && baseTfm == requiredTfm))
                {
                    if (!hasCondition)
                    {
                        context.ReportAnalysis(AnalysisResult.Create(
                            id: MainRule.Id,
                            title: MainRule.Title,
                            location: tfmNode.Location,
                            description: MainRule.Description,
                            helpUri: null,
                            messageFormat: MainRule.MessageFormat,
                            // Message Format parameters.
                            project.Name,
                            tfmNode.Value,
                            context.TargetGodotVersion,
                            requiredTfm));
                    }
                    else
                    {
                        context.ReportAnalysis(AnalysisResult.Create(
                            id: PlatformRule.Id,
                            title: PlatformRule.Title,
                            location: tfmNode.Location,
                            description: PlatformRule.Description,
                            helpUri: null,
                            messageFormat: PlatformRule.MessageFormat,
                            // Message Format parameters.
                            project.Name,
                            tfmNode.Value,
                            context.TargetGodotVersion,
                            requiredTfm,
                            platform ?? "???"));
                    }

                    // This loop iterates all the values in a TFM property in case it's multi-targeting,
                    // but we must only report one diagnostic for each TFM property.
                    break;
                }
            }
        }
    }
}

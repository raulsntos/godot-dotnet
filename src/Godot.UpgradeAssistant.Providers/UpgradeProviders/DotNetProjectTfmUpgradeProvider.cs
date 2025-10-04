using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using NuGet.Frameworks;
using Serilog;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetProjectTfmUpgradeProvider : IUpgradeProvider
{
    public bool CanHandle(string ruleId)
    {
        return ruleId == Descriptors.GUA0001_DotNetProjectTfm.Id;
    }

    public async Task UpgradeAsync(UpgradeContext context, IEnumerable<AnalysisResult> analysisResults, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Upgrader only applies to .NET workspaces and there isn't one.
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
            // This loop iterates all the TFM properties but we only care about the ones that
            // were reported by the analyzer, so check if the current TFM property has an
            // associated analysis result.
            var analysisResult = analysisResults.FirstOrDefault(analysisResult =>
            {
                return analysisResult.Location.FilePath == tfmNode.Location.File
                    && analysisResult.Location.StartLine == (tfmNode.Location.Line - 1)
                    && analysisResult.Location.StartColumn == (tfmNode.Location.Column - 1);
            });
            if (analysisResult is null)
            {
                // No matching analysis result found for this TFM property.
                continue;
            }

            // Check if the TFM property (or the PropertyGroup it belongs to) has a condition.
            // If so, we only want to register an upgrade for the properties we know how to handle,
            // the ones without conditions or with a condition that matches the GodotTargetPlatform
            // condition we added in GodotSharp for some platforms.
            bool hasCondition = tfmNode.IsConditioned();
            string? platform = null;
            if (hasCondition)
            {
                if (!tfmNode.ConditionMatchesGodotPlatform(out platform))
                {
                    // The property or its parent has an unrecognized condition, skip it.
                    continue;
                }
            }

            // The minimum required TFM for this property is the common mainRequiredTfm,
            // unless the property is platform-specific.
            var platformRequiredTfm = minimumRequiredTfm;
            if (!string.IsNullOrEmpty(platform))
            {
                platformRequiredTfm = await TargetFrameworkUtils.GetRequiredTargetFrameworkAsync(context.TargetGodotVersion, platform, context.IsGodotDotNetEnabled, cancellationToken).ConfigureAwait(false);
                if (platformRequiredTfm is null)
                {
                    // We could not find the required TFM, let the user fix it manually.
                    Log.Warning(SR.FormatLog_UnableToGetRequiredTfm(context.TargetGodotVersion));
                    continue;
                }
            }

            if (tfmNode.IsMultiTargeting())
            {
                var tfms = TargetFrameworkUtils.GetTargetFrameworks(tfmNode);
                context.RegisterUpgrade(new DotNetProjectRootUpgradeAction(
                    id: nameof(DotNetProjectTfmUpgradeProvider),
                    title: SR.FormatGUA0001_DotNetProjectTfm_CodeFix(platformRequiredTfm),
                    createChangedProjectRoot: _ =>
                    {
                        // Remove incompatible TFMs and/or add required TFM.
                        var compatibleTfms = tfms
                            .Where(tfm => DefaultCompatibilityProvider.Instance.IsCompatible(tfm, platformRequiredTfm))
                            .ToArray();

                        if (compatibleTfms.Length > 0)
                        {
                            tfmNode.Value = string.Join(';', compatibleTfms.Select(tfm => tfm.GetShortFolderName()));
                        }
                        else
                        {
                            RemoveOrUpdateTfm(tfmNode, platformRequiredTfm, baseTfm);
                        }

                        return Task.FromResult<ProjectRootElement?>(tfmNode.ContainingProject);
                    }), analysisResult);
            }
            else
            {
                var tfm = TargetFrameworkUtils.GetTargetFramework(tfmNode);
                context.RegisterUpgrade(new DotNetProjectRootUpgradeAction(
                    id: nameof(DotNetProjectTfmUpgradeProvider),
                    title: SR.FormatGUA0001_DotNetProjectTfm_CodeFix(platformRequiredTfm),
                    createChangedProjectRoot: _ =>
                    {
                        // Remove incompatible TFM property or set value to the required TFM.
                        RemoveOrUpdateTfm(tfmNode, platformRequiredTfm, baseTfm);
                        return Task.FromResult<ProjectRootElement?>(tfmNode.ContainingProject);
                    }), analysisResult);
            }

            static void RemoveOrUpdateTfm(ProjectPropertyElement tfmNode, NuGetFramework platformRequiredTfm, NuGetFramework baseTfm)
            {
                bool hasCondition = tfmNode.IsConditioned();
                if (hasCondition)
                {
                    // The TFM property has a known condition, so it's platform specific.
                    // We can remove this TFM property when there is another TFM property
                    // without conditions that already matches the minimum required TFM
                    // version for the platform.
                    if (baseTfm == platformRequiredTfm)
                    {
                        var parent = tfmNode.Parent;

                        parent.RemoveChild(tfmNode);

                        if (parent.Count == 0)
                        {
                            // Remove the parent PropertyGroup if it has no other children.
                            parent.Parent.RemoveChild(parent);
                        }

                        return;
                    }
                }

                tfmNode.Value = platformRequiredTfm.GetShortFolderName();
            }
        }
    }
}

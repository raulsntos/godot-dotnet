using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Serilog;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetProjectSdkUpgradeProvider : IUpgradeProvider
{
    public bool CanHandle(string ruleId)
    {
        return ruleId == Descriptors.GUA0002_DotNetProjectSdk.Id;
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

        if (!projectRoot.Sdk.StartsWith(Constants.GodotSdkAssemblyName, StringComparison.OrdinalIgnoreCase))
        {
            // Unknown SDK so we won't provide a fix, let the user fix it manually.
            Log.Warning(SR.FormatLog_FoundUnknownSdk(projectRoot.Sdk));
            return Task.CompletedTask;
        }

        if (projectRoot.Sdk.Contains('/'))
        {
            // The SDK in the project is versioned, update it.

            string sdk = $"{Constants.GodotSdkAssemblyName}/{context.TargetGodotVersion}";

            context.RegisterUpgrade(new DotNetProjectRootUpgradeAction(
                id: nameof(DotNetProjectSdkUpgradeProvider),
                title: SR.GUA0002_DotNetProjectSdk_CodeFix,
                createChangedProjectRoot: _ =>
                {
                    projectRoot.Sdk = sdk;
                    return Task.FromResult<ProjectRootElement?>(projectRoot);
                }), analysisResult);
        }
        else
        {
            // The SDK in the project is not versioned, so the analyzer must have found
            // an outdated version in 'global.json' and we should update it there.

            context.RegisterUpgrade(new GlobalJsonUpgradeAction(
                id: nameof(DotNetProjectSdkUpgradeProvider),
                title: SR.GUA0002_DotNetProjectSdk_CodeFix,
                applyChanges: (jsonNode, _) =>
                {
                    var sdksNode = jsonNode["msbuild-sdks"]!;
                    sdksNode["Godot.NET.Sdk"] = context.TargetGodotVersion.ToString();
                    return Task.FromResult(jsonNode);
                }), analysisResult);
        }

        return Task.CompletedTask;
    }
}

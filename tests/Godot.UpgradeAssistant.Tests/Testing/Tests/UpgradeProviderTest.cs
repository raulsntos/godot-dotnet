using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

namespace Godot.UpgradeAssistant.Tests;

internal class UpgradeProviderTest<TUpgradeProvider, TAnalysisProvider> : AnalysisProviderTest<TAnalysisProvider>
    where TUpgradeProvider : IUpgradeProvider, new()
    where TAnalysisProvider : IAnalysisProvider, new()
{
    public string? FixedSolution { get; set; }

    public string? FixedProject { get; set; }

    public string? FixedGlobalJson { get; set; }

    private readonly List<UpgradeFix> _fixes = [];

    public IReadOnlyList<UpgradeFix> UpgradeFixes => _fixes;

    protected override async Task RunAsyncCore(MemoryWorkspaceInfo workspace, DiagnosticResult[] processedExpectedDiagnostics, CancellationToken cancellationToken = default)
    {
        await VerifyAnalysis(workspace, processedExpectedDiagnostics, cancellationToken);
        await VerifyUpgrade(workspace, cancellationToken);
    }

    protected async Task VerifyUpgrade(MemoryWorkspaceInfo workspace, CancellationToken cancellationToken = default)
    {
        _fixes.Clear();

        var verifier = new DefaultVerifier();

        var fixer = new TUpgradeProvider();

        var context = new UpgradeContext(ReportUpgrade)
        {
            Workspace = workspace,
            TargetGodotVersion = TargetGodotVersion,
            IsGodotDotNetEnabled = IsGodotDotNetEnabled,
        };

        // Upgrade providers expect at least one analysis result to be present,
        // because they are only invoked for specific analysis results.
        if (AnalysisResults.Count > 0)
        {
            await fixer.UpgradeAsync(context, AnalysisResults, cancellationToken);
        }

        var upgradeAllContext = new UpgradeAllContext();

        foreach (var fix in _fixes)
        {
            // It's preferable to apply fixes in bulk, check if the current fix can be
            // applied in bulk and if so register it with the upgrade-all context.
            if (upgradeAllContext.TryRegisterUpgrade(fix))
            {
                // Skip individual apply changes, the fix will be applied by the upgrade-all context.
                continue;
            }

            await fix.UpgradeAction.ApplyChanges(workspace, cancellationToken).ConfigureAwait(false);
        }

        // Apply all the fixes that were registered to apply in bulk.
        await upgradeAllContext.ApplyAllChanges(workspace, cancellationToken).ConfigureAwait(false);

        verifier.True(workspace.DotNetWorkspace is not null);

        // Verify the project matches the expected fixed project.
        {
            string message = "The upgraded project does not match the expected fixed project.";
            string expected = (FixedProject ?? TestProject ?? DefaultProject).ReplaceLineEndings();
            string actual = workspace.DotNetWorkspace.ProjectFile.Content.ReplaceLineEndings();
            verifier.EqualOrDiff(expected, actual, message);
        }

        // Verify the solution matches the expected fixed solution.
        {
            string message = "The upgraded solution does not match the expected fixed solution.";
            string expected = (FixedSolution ?? TestSolution ?? DefaultSolution).ReplaceLineEndings();
            string actual = workspace.DotNetWorkspace.SolutionFile.Content.ReplaceLineEndings();
            verifier.EqualOrDiff(expected, actual, message);
        }

        // Verify the 'global.json' matches the expected fixed 'global.json'.
        {
            if (FixedGlobalJson is not null)
            {
                string message = "The upgraded global.json does not match the expected fixed global.json.";
                string expected = (FixedGlobalJson ?? TestGlobalJson ?? "").ReplaceLineEndings();
                string actual = (workspace.DotNetWorkspace.GlobalJsonFile?.Content ?? "").ReplaceLineEndings();
                verifier.EqualOrDiff(expected, actual, message);
            }
            else
            {
                verifier.False(workspace.DotNetWorkspace.TryOpenGlobalJsonStream(out _), "The workspace should not have a global.json file.");
            }
        }

        void ReportUpgrade(UpgradeAction upgrade, ImmutableArray<AnalysisResult> analysis)
        {
            _fixes.Add(new UpgradeFix(upgrade, analysis));
        }
    }
}

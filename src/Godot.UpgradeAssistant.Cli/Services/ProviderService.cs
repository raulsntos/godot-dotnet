using System;
using System.Collections.Generic;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Cli.Services;

internal sealed class ProviderService
{
    private bool _isReadOnly;

    private readonly List<IAnalysisProvider> _analysisProviders =
    [
        new DotNetProjectTfmAnalysisProvider(),
        new DotNetProjectSdkAnalysisProvider(),
        new DotNetProjectReferencesAnalysisProvider(),
        new DotNetProjectEnablePreviewAnalysisProvider(),
        new DotNetSolutionConfigurationAnalysisProvider(),
        new DotNetAnalyzersAnalysisProvider(),
    ];

    private readonly List<IUpgradeProvider> _upgradeProviders =
    [
        new DotNetProjectTfmUpgradeProvider(),
        new DotNetProjectSdkUpgradeProvider(),
        new DotNetProjectEnablePreviewUpgradeProvider(),
        new DotNetSolutionConfigurationUpgradeProvider(),
        new DotNetCodeFixersUpgradeProvider(),
    ];

    /// <summary>
    /// Get the collection of registered analyzers that can be used to analyze the project.
    /// </summary>
    public IReadOnlyCollection<IAnalysisProvider> AnalysisProviders => _analysisProviders;

    /// <summary>
    /// Get the collection of registered upgraders that can be used to upgrade the project.
    /// </summary>
    public IReadOnlyCollection<IUpgradeProvider> UpgradeProviders => _upgradeProviders;

    public void AddAnalysisProvider(IAnalysisProvider provider)
    {
        ThrowIfReadOnly();
        _analysisProviders.Add(provider);
    }

    public void AddUpgradeProvider(IUpgradeProvider provider)
    {
        ThrowIfReadOnly();
        _upgradeProviders.Add(provider);
    }

    public void MakeReadOnly()
    {
        _isReadOnly = true;
    }

    private void ThrowIfReadOnly()
    {
        if (_isReadOnly)
        {
            throw new InvalidOperationException(SR.InvalidOperation_ProviderServiceIsReadOnly);
        }
    }
}

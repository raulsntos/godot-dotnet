namespace Godot.UpgradeAssistant.Tests;

internal static class UpgradeProviderVerifier<TUpgradeProvider, TAnalysisProvider>
    where TUpgradeProvider : IUpgradeProvider, new()
    where TAnalysisProvider : IAnalysisProvider, new()
{
    public sealed class Test : UpgradeProviderTest<TUpgradeProvider, TAnalysisProvider> { }

    public static Test MakeVerifier()
    {
        return new Test();
    }
}

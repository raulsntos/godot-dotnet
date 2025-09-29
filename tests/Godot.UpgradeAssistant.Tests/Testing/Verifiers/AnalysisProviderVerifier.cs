namespace Godot.UpgradeAssistant.Tests;

internal static class AnalysisProviderVerifier<TAnalysisProvider>
    where TAnalysisProvider : IAnalysisProvider, new()
{
    public sealed class Test : AnalysisProviderTest<TAnalysisProvider> { }

    public static Test MakeVerifier()
    {
        return new Test();
    }
}

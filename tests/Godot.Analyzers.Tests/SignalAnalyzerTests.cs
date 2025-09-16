using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<SignalDelegateParameterTypesAnalyzer>;

public class SignalAnalyzerTests
{
    [Fact]
    public async Task SignalParameterTypesAreSupported()
    {
        await Verifier.Verify("GODOT0701_SignalParameterTypesAreSupported.cs");
    }

    [Fact]
    public async Task SignalReturnsVoid()
    {
        await Verifier.Verify("GODOT0702_SignalReturnsVoid.cs");
    }

    [Fact]
    public async Task SignalHasCorrectSuffix()
    {
        await CSharpAnalyzerVerifier<SignalDelegateNameSuffixAnalyzer>.Verify("GODOT0704_SignalHasCorrectSuffix.cs");
    }
}

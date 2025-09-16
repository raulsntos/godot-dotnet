using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<BindConstantAnalyzer>;

public class BindConstantAnalyzerTests
{
    [Fact]
    public async Task ConstantTypeIsSupported()
    {
        await Verifier.Verify("GODOT0301_ConstantTypeIsSupported.cs");
    }

    [Fact]
    public async Task ConstantIsConst()
    {
        await Verifier.Verify("GODOT0302_ConstantIsConst.cs");
    }
}

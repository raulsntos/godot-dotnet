using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<GodotClassAnalyzer>;

public class GodotClassAnalyzerTests
{
    [Fact]
    public async Task GodotClassMustNotBeStatic()
    {
        await Verifier.Verify("GODOT0101_GodotClassMustNotBeStatic.cs");
    }

    [Fact]
    public async Task GodotClassMustNotBeGeneric()
    {
        await Verifier.Verify("GODOT0102_GodotClassMustNotBeGeneric.cs");
    }
}

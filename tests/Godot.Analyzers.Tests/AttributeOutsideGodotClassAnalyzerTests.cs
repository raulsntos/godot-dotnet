using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<AttributeOutsideGodotClassAnalyzer>;

public class AttributeOutsideGodotClassAnalyzerTests
{
    [Fact]
    public async Task AttributeHasNoEffectOutsideGodotClass()
    {
        await Verifier.Verify("GODOT0001_AttributeHasNoEffectOutsideGodotClass.cs");
    }
}

using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpCodeFixVerifier<ReplaceSpeciallyRecognizedTypeWithGodotTypeCodeFix, MarshallingRequiresCopyingAnalyzer>;

public class MarshallingRequiresCopyingAnalyzerTests
{
    [Fact]
    public async Task MarshallingTypesThatRequireCopying()
    {
        await Verifier.Verify("GODOT0003_MarshallingTypesThatRequireCopying.cs", "GODOT0003_MarshallingTypesThatRequireCopying.fixed.cs");
    }
}

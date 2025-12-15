using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpCodeFixVerifier<CacheStringNameOrNodePathCodeFix, ImplicitStringConversionAnalyzer>;

public class ImplicitStringConversionAnalyzerTests
{
    [Fact]
    public async Task AvoidImplicitStringConversion()
    {
        await Verifier.Verify("GODOT0005_AvoidImplicitStringConversion.cs", "GODOT0005_AvoidImplicitStringConversion.fixed.cs");
    }
}

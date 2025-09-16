using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<MustBeVariantAnalyzer>;

public class MustBeVariantAnalyzerTests
{
    [Fact]
    public async Task GenericTypeArgumentMustBeVariant()
    {
        await Verifier.Verify("GODOT0801_GenericTypeArgumentMustBeVariant.cs");
    }

    [Fact]
    public async Task GenericTypeParameterMustBeVariant()
    {
        await Verifier.Verify("GODOT0802_GenericTypeParameterMustBeVariant.cs");
    }
}

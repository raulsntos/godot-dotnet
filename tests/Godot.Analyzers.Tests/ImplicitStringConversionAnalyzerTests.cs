using System;
using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpCodeFixVerifier<CacheStringNameOrNodePathCodeFix, ImplicitStringConversionAnalyzer>;

public class ImplicitStringConversionAnalyzerTests
{
    [Fact]
    public async Task AvoidImplicitStringConversion()
    {
        if (OperatingSystem.IsWindows())
        {
            // TODO: Skipping on Windows due to line ending differences.
            // We should really look into why the code fix adds <CR><LF> instead of <LF>
            // to the type declaration's open bracket's trailing trivia on Windows.
            // It doesn't seem to be done by the implementation, but rather by the
            // formatting when Roslyn applies the code fix.
            return;
        }

        await Verifier.Verify("GODOT0005_AvoidImplicitStringConversion.cs", "GODOT0005_AvoidImplicitStringConversion.fixed.cs");
    }
}

using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<BoundMembersMustHaveUniqueNamesAnalyzer>;

public class BoundMembersMustHaveUniqueNamesAnalyzerTests
{
    [Fact]
    public async Task BoundMembersWithDuplicateNames()
    {
        await Verifier.Verify("GODOT0002_BoundMembersWithDuplicateNames.cs");
    }
}

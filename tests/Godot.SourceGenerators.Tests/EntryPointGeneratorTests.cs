using System.Threading.Tasks;

namespace Godot.SourceGenerators.Tests;

using Verifier = CSharpSourceGeneratorVerifier<EntryPointGenerator>;

public class EntryPointGeneratorTests
{
    [Fact]
    public async Task RegisterEveryKindOfClass()
    {
        await Verifier.Verify(
            ["MultipleClassRegistrationKinds.cs"],
            ["Main.generated.cs"]
        );
    }

    [Fact]
    public async Task DisableGodotEntryPointGeneration()
    {
        await Verifier.Verify(
            ["MultipleClassRegistrationKinds.cs", "DisableGodotEntryPointGeneration.cs"],
            [("Main.generated.cs", "MainWithoutEntryPoint.generated.cs")]
        );
    }
}

using System.Threading.Tasks;

namespace Godot.SourceGeneration.Tests;

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

    [Fact]
    public async Task BaseTypesRegisteredBeforeDerivedTypes()
    {
        await Verifier.Verify(
            ["NodesWithInheritance.cs"],
            [("Main.generated.cs", "MainWithInheritance.generated.cs")]
        );
    }
}

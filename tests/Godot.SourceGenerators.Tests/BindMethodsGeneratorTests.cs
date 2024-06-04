using System.Threading.Tasks;

namespace Godot.SourceGenerators.Tests;

using Verifier = CSharpSourceGeneratorVerifier<BindMethodsGenerator>;

public class BindMethodsGeneratorTests
{
    [Fact]
    public async Task BindEveryKindOfMember()
    {
        await Verifier.Verify(
            ["MyNode.cs"],
            ["NS.MyNode.generated.cs"]
        );
    }

    [Fact]
    public async Task BindMethods()
    {
        await Verifier.Verify(
            ["NodeWithMethods.cs"],
            ["NS.NodeWithMethods.generated.cs"]
        );
    }

    [Fact]
    public async Task BindConstants()
    {
        await Verifier.Verify(
            ["NodeWithConstants.cs"],
            ["NS.NodeWithConstants.generated.cs"]
        );
    }

    [Fact]
    public async Task BindProperties()
    {
        await Verifier.Verify(
            ["NodeWithProperties.cs"],
            ["NS.NodeWithProperties.generated.cs"]
        );
    }

    [Fact]
    public async Task BindSignals()
    {
        await Verifier.Verify(
            ["NodeWithSignals.cs"],
            ["NS.NodeWithSignals.generated.cs"]
        );
    }

    [Fact]
    public async Task DefaultParameterValues()
    {
        await Verifier.Verify(
            ["ParameterDefaultValues.cs"],
            ["NS.ParameterDefaultValues.generated.cs"]
        );
    }

    [Fact]
    public async Task DefaultMarshalling()
    {
        await Verifier.Verify(
            ["NodeWithDefaultMarshalling.cs"],
            ["NS.NodeWithDefaultMarshalling.generated.cs"]
        );
    }

    [Fact]
    public async Task NestedNamespaces()
    {
        await Verifier.Verify(
            ["NestedNamespaces.cs"],
            ["NamespaceA.NamespaceB.MyNestedNamespacesNode.generated.cs"]
        );
    }

    [Fact]
    public async Task NestedTypes()
    {
        await Verifier.Verify(
            ["NestedTypes.cs"],
            ["NS.Node1.generated.cs", "NS.Node1.Node2.generated.cs", "NS.Node1.Node2.Node3.generated.cs"]
        );
    }

    [Fact]
    public async Task MultipleClassesInOneFile()
    {
        await Verifier.Verify(
            ["MultipleClassesInOneFile.cs"],
            ["NamespaceA.ClassOne.generated.cs", "NamespaceB.ClassTwo.generated.cs"]
        );
    }
}

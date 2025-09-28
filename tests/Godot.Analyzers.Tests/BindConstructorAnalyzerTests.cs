using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<BindConstructorAnalyzer>;

public class BindConstructorAnalyzerTests
{
    [Fact]
    public async Task BuilderMethodNotFound()
    {
        await Verifier.Verify("GODOT0201_BuilderMethodNotFound.cs", [
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(0).WithArguments("MyNode1.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(1).WithArguments("MyNode2Builder.Create"),
        ]);
    }

    [Fact]
    public async Task BuilderMethodOverloads()
    {
        await Verifier.Verify("GODOT0201_BuilderMethodOverloads.cs", [
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(0).WithArguments("MyNode3.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(1).WithArguments("MyNode4Builder.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(2).WithArguments("MyNode5.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(3).WithArguments("MyNode6Builder.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(4).WithArguments("MyNode7.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodNotFound).WithLocation(5).WithArguments("MyNode8Builder.Create"),
        ]);
    }

    [Fact]
    public async Task BuilderMethodInaccessible()
    {
        await Verifier.Verify("GODOT0201_BuilderMethodInaccessible.cs", [
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodInaccessible).WithLocation(0).WithArguments("MyNode3Builder.Create", "MyNode3"),
        ]);
    }

    [Fact]
    public async Task BuilderMethodMustBeStatic()
    {
        await Verifier.Verify("GODOT0201_BuilderMethodMustBeStatic.cs", [
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustBeStatic).WithLocation(0).WithArguments("MyNode1.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustBeStatic).WithLocation(1).WithArguments("MyNode2Builder.Create"),
        ]);
    }

    [Fact]
    public async Task BuilderMethodMustHaveNoGenericTypeParameters()
    {
        await Verifier.Verify("GODOT0201_BuilderMethodMustHaveNoGenericTypeParameters.cs", [
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustHaveNoGenericTypeParameters).WithLocation(0).WithArguments("MyNode1.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustHaveNoGenericTypeParameters).WithLocation(1).WithArguments("MyNode2Builder.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustHaveNoGenericTypeParameters).WithLocation(2).WithArguments("MyNode3Builder<>.Create"),
        ]);
    }

    [Fact]
    public async Task BuilderMethodMustHaveNoParameters()
    {
        await Verifier.Verify("GODOT0201_BuilderMethodMustHaveNoParameters.cs", [
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustHaveNoParameters).WithLocation(0).WithArguments("MyNode1.Create"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustHaveNoParameters).WithLocation(1).WithArguments("MyNode2Builder.Create"),
        ]);
    }

    [Fact]
    public async Task BuilderMethodMustReturnCompatibleType()
    {
        await Verifier.Verify("GODOT0201_BuilderMethodMustReturnCompatibleType.cs", [
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustReturnCompatibleType).WithLocation(0).WithArguments("MyNode1.Create", "MyNode1"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustReturnCompatibleType).WithLocation(1).WithArguments("MyNode2Builder.Create", "MyNode2"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustReturnCompatibleType).WithLocation(2).WithArguments("MyNode3.Create", "MyNode3"),
            new DiagnosticResult(Descriptors.GODOT0201_BuilderMethodMustReturnCompatibleType).WithLocation(3).WithArguments("MyNode4Builder.Create", "MyNode4"),
        ]);
    }
}

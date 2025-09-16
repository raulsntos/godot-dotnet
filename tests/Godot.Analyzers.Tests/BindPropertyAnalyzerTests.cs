using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<BindPropertyAnalyzer>;

public class BindPropertyAnalyzerTests
{
    [Fact]
    public async Task PropertyTypeIsSupportedInFields()
    {
        await Verifier.Verify("GODOT0501_PropertyTypeIsSupportedInFields.cs");
    }

    [Fact]
    public async Task PropertyTypeIsSupportedInProperties()
    {
        await Verifier.Verify("GODOT0501_PropertyTypeIsSupportedInProperties.cs");
    }

    [Fact]
    public async Task PropertyIsNotStatic()
    {
        await Verifier.Verify("GODOT0502_PropertyIsNotStatic.cs");
    }

    [Fact]
    public async Task PropertyIsNotConst()
    {
        await Verifier.Verify("GODOT0503_PropertyIsNotConst.cs");
    }

    [Fact]
    public async Task PropertyIsNotReadOnly()
    {
        await Verifier.Verify("GODOT0504_PropertyIsNotReadOnly.cs");
    }

    [Fact]
    public async Task PropertyHasGetterAndSetter()
    {
        await Verifier.Verify("GODOT0505_PropertyHasGetterAndSetter.cs", [
            new DiagnosticResult(Descriptors.GODOT0505_PropertyMustHaveSetter).WithLocation(0).WithArguments("MyGetterOnlyProperty"),
            new DiagnosticResult(Descriptors.GODOT0505_PropertyMustHaveGetter).WithLocation(1).WithArguments("MySetterOnlyProperty"),
            new DiagnosticResult(Descriptors.GODOT0505_PropertyMustHaveGetter).WithLocation(2).WithArguments("MyInitOnlyProperty"),
            new DiagnosticResult(Descriptors.GODOT0505_PropertyMustHaveSetter).WithLocation(2).WithArguments("MyInitOnlyProperty"),
            new DiagnosticResult(Descriptors.GODOT0505_PropertyMustHaveSetter).WithLocation(3).WithArguments("MyGetterInitOnlyProperty"),
        ]);
    }

    [Fact]
    public async Task PropertyIsNotIndexer()
    {
        await Verifier.Verify("GODOT0506_PropertyIsNotIndexer.cs");
    }
}

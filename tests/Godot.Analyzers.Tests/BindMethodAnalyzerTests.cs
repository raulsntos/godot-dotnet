using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpAnalyzerVerifier<BindMethodAnalyzer>;

public class BindMethodAnalyzerTests
{
    [Fact]
    public async Task MethodParameterTypesAreSupported()
    {
        await Verifier.Verify("GODOT0601_MethodParameterTypesAreSupported.cs", [
            new DiagnosticResult(Descriptors.GODOT0601_MethodParameterTypeIsNotSupported).WithLocation(0).WithArguments("object"),
        ]);
    }

    [Fact]
    public async Task MethodReturnTypesAreSupported()
    {
        await Verifier.Verify("GODOT0601_MethodReturnTypesAreSupported.cs", [
            new DiagnosticResult(Descriptors.GODOT0601_MethodReturnTypeIsNotSupported).WithLocation(0).WithArguments("object"),
        ]);
    }
}

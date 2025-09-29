using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = CSharpCodeFixVerifier<MergedRpcAttributesCodeFix, MergedRpcAttributesAnalyzer>;

public class MergedRpcAttributesTests
{
    [Fact]
    public static async Task UpgradeFromGodotSharp3ToGodotSharp4()
    {
        await Verifier.Verify(
            "GUA1005_MergedRpcAttributes_FromGodotSharp3.cs",
            "GUA1005_MergedRpcAttributes_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotSharp4);
    }

    [Fact(Skip = "See TODO below. The workaround is not enough because in this test case there are intentional compiler errors, and if we suppress them all the test fails since it expects more errors than it gets.")]
    public static async Task UpgradeFromGodotSharp3ToGodotDotNet()
    {
        // TODO: Godot .NET doesn't implement the [Rpc] attribute yet, so the fixed code will produce compiler errors.
        // We are ignoring compiler diagnostics for now, remove it when the attribute is implemented.
        var verifier = Verifier.MakeVerifier(
            "GUA1005_MergedRpcAttributes_FromGodotSharp3.cs",
            "GUA1005_MergedRpcAttributes_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotDotNet);
        verifier.CompilerDiagnostics = Microsoft.CodeAnalysis.Testing.CompilerDiagnostics.None;
        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeFromGodotSharp4ToGodotDotNet()
    {
        // TODO: Godot .NET doesn't implement the [Rpc] attribute yet, so the fixed code will produce compiler errors.
        // We are ignoring compiler diagnostics for now, remove it when the attribute is implemented.
        var verifier = Verifier.MakeVerifier(
            "GUA1005_MergedRpcAttributes_FromGodotSharp4.cs",
            "GUA1005_MergedRpcAttributes_FromGodotSharp4.fixed.cs",
            GodotReferenceAssemblies.GodotSharp4,
            GodotReferenceAssemblies.GodotDotNet);
        verifier.CompilerDiagnostics = Microsoft.CodeAnalysis.Testing.CompilerDiagnostics.None;
        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeAlreadyLatest()
    {
        // TODO: Godot .NET doesn't implement the [Rpc] attribute yet, so the fixed code will produce compiler errors.
        // We are ignoring compiler diagnostics for now, remove it when the attribute is implemented.
        var verifier = Verifier.MakeVerifier(
            "GUA1005_MergedRpcAttributes_FromGodotSharp4.cs",
            "GUA1005_MergedRpcAttributes_FromGodotSharp4.fixed.cs",
            GodotReferenceAssemblies.GodotDotNet,
            GodotReferenceAssemblies.GodotDotNet);
        verifier.CompilerDiagnostics = Microsoft.CodeAnalysis.Testing.CompilerDiagnostics.None;
        await verifier.RunAsync();
    }
}

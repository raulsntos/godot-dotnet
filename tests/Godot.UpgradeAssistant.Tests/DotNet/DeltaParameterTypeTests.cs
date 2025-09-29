using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = CSharpCodeFixVerifier<DeltaParameterTypeCodeFix, DeltaParameterTypeAnalyzer>;

public class DeltaParameterTypeTests
{
    [Fact]
    public static async Task UpgradeFromGodotSharp3ToGodotSharp4()
    {
        await Verifier.Verify(
            "GUA1010_DeltaParameterType_FromGodotSharp3To4.cs",
            "GUA1010_DeltaParameterType_FromGodotSharp3To4.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotSharp4);
    }

    [Fact]
    public static async Task UpgradeFromGodotSharp3ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1010_DeltaParameterType_FromGodotSharp3.cs",
            "GUA1010_DeltaParameterType_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeFromGodotSharp4ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1010_DeltaParameterType_FromGodotSharp4.cs",
            "GUA1010_DeltaParameterType_FromGodotSharp4.fixed.cs",
            GodotReferenceAssemblies.GodotSharp4,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeAlreadyLatest()
    {
        await Verifier.Verify(
            "GUA1010_DeltaParameterType_FromGodotDotNet.cs",
            "GUA1010_DeltaParameterType_FromGodotDotNet.fixed.cs",
            GodotReferenceAssemblies.GodotDotNet,
            GodotReferenceAssemblies.GodotDotNet);
    }
}

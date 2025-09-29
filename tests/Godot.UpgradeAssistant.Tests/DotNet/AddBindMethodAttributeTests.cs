using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = CSharpCodeFixVerifier<AddBindMethodAttributeCodeFix, AddBindMethodAttributeAnalyzer>;

public class AddBindMethodAttributeTests
{
    [Fact]
    public static async Task UpgradeFromGodotSharp3ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1008_AddBindMethodAttribute_FromGodotSharp3.cs",
            "GUA1008_AddBindMethodAttribute_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeFromGodotSharp4ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1008_AddBindMethodAttribute_FromGodotSharp4.cs",
            "GUA1008_AddBindMethodAttribute_FromGodotSharp4.fixed.cs",
            GodotReferenceAssemblies.GodotSharp4,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeAlreadyLatest()
    {
        await Verifier.Verify(
            "GUA1008_AddBindMethodAttribute_FromGodotDotNet.cs",
            "GUA1008_AddBindMethodAttribute_FromGodotDotNet.fixed.cs",
            GodotReferenceAssemblies.GodotDotNet,
            GodotReferenceAssemblies.GodotDotNet);
    }
}

using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = CSharpCodeFixVerifier<AddGodotClassAttributeCodeFix, AddGodotClassAttributeAnalyzer>;

public class AddGodotClassAttributeTests
{
    [Fact]
    public static async Task UpgradeFromGodotSharp3ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1006_AddGodotClassAttribute_FromGodotSharp3.cs",
            "GUA1006_AddGodotClassAttribute_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeFromGodotSharp4ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1006_AddGodotClassAttribute_FromGodotSharp4.cs",
            "GUA1006_AddGodotClassAttribute_FromGodotSharp4.fixed.cs",
            GodotReferenceAssemblies.GodotSharp4,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeAlreadyLatest()
    {
        await Verifier.Verify(
            "GUA1006_AddGodotClassAttribute_FromGodotDotNet.cs",
            "GUA1006_AddGodotClassAttribute_FromGodotDotNet.fixed.cs",
            GodotReferenceAssemblies.GodotDotNet,
            GodotReferenceAssemblies.GodotDotNet);
    }
}

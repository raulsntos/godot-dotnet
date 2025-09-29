using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = CSharpCodeFixVerifier<VirtualMethodAccessibilityCodeFix, VirtualMethodAccessibilityAnalyzer>;

public class VirtualMethodAccessibilityTests
{
    [Fact]
    public static async Task UpgradeFromGodotSharp3ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1009_VirtualMethodAccessibility_FromGodotSharp3.cs",
            "GUA1009_VirtualMethodAccessibility_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeFromGodotSharp4ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1009_VirtualMethodAccessibility_FromGodotSharp3.cs",
            "GUA1009_VirtualMethodAccessibility_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp4,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeAlreadyLatest()
    {
        await Verifier.Verify(
            "GUA1009_VirtualMethodAccessibility_FromGodotDotNet.cs",
            "GUA1009_VirtualMethodAccessibility_FromGodotDotNet.fixed.cs",
            GodotReferenceAssemblies.GodotDotNet,
            GodotReferenceAssemblies.GodotDotNet);
    }
}

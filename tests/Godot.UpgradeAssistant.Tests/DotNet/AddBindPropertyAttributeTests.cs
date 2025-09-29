using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = CSharpCodeFixVerifier<AddBindPropertyAttributeCodeFix, AddBindPropertyAttributeAnalyzer>;

public class AddBindPropertyAttributeTests
{
    [Fact]
    public static async Task UpgradeFromGodotSharp3ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1007_AddBindPropertyAttribute_FromGodotSharp3.cs",
            "GUA1007_AddBindPropertyAttribute_FromGodotSharp3.fixed.cs",
            GodotReferenceAssemblies.GodotSharp3,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeFromGodotSharp4ToGodotDotNet()
    {
        await Verifier.Verify(
            "GUA1007_AddBindPropertyAttribute_FromGodotSharp4.cs",
            "GUA1007_AddBindPropertyAttribute_FromGodotSharp4.fixed.cs",
            GodotReferenceAssemblies.GodotSharp4,
            GodotReferenceAssemblies.GodotDotNet);
    }

    [Fact]
    public static async Task UpgradeAlreadyLatest()
    {
        await Verifier.Verify(
            "GUA1007_AddBindPropertyAttribute_FromGodotDotNet.cs",
            "GUA1007_AddBindPropertyAttribute_FromGodotDotNet.fixed.cs",
            GodotReferenceAssemblies.GodotDotNet,
            GodotReferenceAssemblies.GodotDotNet);
    }
}

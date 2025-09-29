using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = UpgradeProviderVerifier<DotNetProjectSdkUpgradeProvider, DotNetProjectSdkAnalysisProvider>;

public class DotNetProjectSdkTests
{
    [Fact]
    public static async Task UpgradeWithMicrosoftSdk()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestProject = """
            <Project {|GUA0002:Sdk="Microsoft.NET.Sdk"|}>

              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeWithGodotSdk()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestProject = """
            <Project {|GUA0002:Sdk="Godot.NET.Sdk/4.0.0"|}>

              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeWithGodotSdkInGlobalJson()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestProject = """
            <Project {|GUA0002:Sdk="Godot.NET.Sdk"|}>

              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.TestGlobalJson = """
            {
              "msbuild-sdks": {
                "Godot.NET.Sdk": "4.0.0"
              }
            }
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk">

              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedGlobalJson = """
            {
              "msbuild-sdks": {
                "Godot.NET.Sdk": "4.5.0"
              }
            }
            """;

        await verifier.RunAsync();
    }
}

using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = UpgradeProviderVerifier<DotNetProjectEnablePreviewUpgradeProvider, DotNetProjectEnablePreviewAnalysisProvider>;

public class DotNetProjectEnablePreviewTests
{
    [Fact]
    public static async Task AddsEnablePreviewPropertyWhenMissing()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestProject = """
            {|GUA0006:|}<Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

              <PropertyGroup>
                <EnableGodotDotNetPreview>true</EnableGodotDotNetPreview>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpdatesEnablePreviewPropertyWhenFalse()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestProject = """
            {|GUA0006:|}<Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <EnableGodotDotNetPreview>false</EnableGodotDotNetPreview>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <EnableGodotDotNetPreview>true</EnableGodotDotNetPreview>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task NoChangeWhenEnablePreviewAlreadyTrue()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <EnableGodotDotNetPreview>true</EnableGodotDotNetPreview>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <EnableGodotDotNetPreview>true</EnableGodotDotNetPreview>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task NoChangeWhenGodotDotNetIsNotEnabled()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task IgnoresConditionedEnablePreviewProperty()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestProject = """
            {|GUA0006:|}<Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
                <EnableGodotDotNetPreview>false</EnableGodotDotNetPreview>
              </PropertyGroup>

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.5.0">

              <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
                <EnableGodotDotNetPreview>false</EnableGodotDotNetPreview>
              </PropertyGroup>

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

              <PropertyGroup>
                <EnableGodotDotNetPreview>true</EnableGodotDotNetPreview>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }
}

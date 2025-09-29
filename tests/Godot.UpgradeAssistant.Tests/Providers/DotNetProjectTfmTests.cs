using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = UpgradeProviderVerifier<DotNetProjectTfmUpgradeProvider, DotNetProjectTfmAnalysisProvider>;

public class DotNetProjectTfmTests
{
    [Theory]
    [InlineData("net6.0", "net8.0")]
    [InlineData("net7.0", "net8.0")]
    public static async Task UpgradeSingleTfm(string initialTfm, string expectedTfm)
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                {|GUA0001:<TargetFramework>{{initialTfm}}</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>{{expectedTfm}}</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Theory]
    [InlineData("net8.0")]
    [InlineData("net9.0")]
    public static async Task UpgradeSingleTfmNoChange(string tfm)
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>{{tfm}}</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>{{tfm}}</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Theory]
    [InlineData("net6.0;net7.0", "net8.0")]
    [InlineData("net6.0;net7.0;net9.0", "net9.0")]
    public static async Task UpgradeMultiTargetingTfms(string initialTfm, string expectedTfm)
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                {|GUA0001:<TargetFrameworks>{{initialTfm}}</TargetFrameworks>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFrameworks>{{expectedTfm}}</TargetFrameworks>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroid1()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                {|GUA0001:<TargetFramework>net6.0</TargetFramework>|}
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroid2()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroid3()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net9.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroidInGroup1()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                {|GUA0001:<TargetFramework>net6.0</TargetFramework>|}
              </PropertyGroup>

              <PropertyGroup Condition="'$(GodotTargetPlatform)' == 'android'">
                {|GUA0001:<TargetFramework>net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroidInGroup2()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                {|GUA0001:<TargetFramework>net6.0</TargetFramework>|}
              </PropertyGroup>

              <PropertyGroup Condition="'$(GodotTargetPlatform)' == 'android'">
                {|GUA0001:<TargetFramework>net7.0</TargetFramework>|}
                <SomeProperty>SomeValue</SomeProperty>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

              <PropertyGroup Condition="'$(GodotTargetPlatform)' == 'android'">
                <SomeProperty>SomeValue</SomeProperty>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmIOS()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
                <TargetFramework Condition="'$(GodotTargetPlatform)' == 'ios'">net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
                <TargetFramework Condition="'$(GodotTargetPlatform)' == 'ios'">net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroidAndIOS1()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                {|GUA0001:<TargetFramework>net6.0</TargetFramework>|}
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net7.0</TargetFramework>|}
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'ios'">net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroidAndIOS2()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net7.0</TargetFramework>|}
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'ios'">net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
                <TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net8.0</TargetFramework>
                <TargetFramework Condition="'$(GodotTargetPlatform)' == 'ios'">net8.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradePlatformSpecificTfmAndroidWithMultiTargetingTfms()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
                {|GUA0001:<TargetFramework Condition="'$(GodotTargetPlatform)' == 'android'">net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = """
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }

    [Theory]
    [InlineData("net6.0", "net8.0")]
    [InlineData("net7.0", "net8.0")]
    public static async Task UpgradeTfmWithUnknownCondition(string initialTfm, string expectedTfm)
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 4, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                {|GUA0001:<TargetFramework>{{initialTfm}}</TargetFramework>|}
                {|GUA0001:<TargetFramework Condition="'$(SomeOtherCondition)' == 'value'">net7.0</TargetFramework>|}
              </PropertyGroup>

            </Project>
            """;

        verifier.FixedProject = $$"""
            <Project Sdk="Godot.NET.Sdk/4.0.0">

              <PropertyGroup>
                <TargetFramework>{{expectedTfm}}</TargetFramework>
                <TargetFramework Condition="'$(SomeOtherCondition)' == 'value'">net7.0</TargetFramework>
              </PropertyGroup>

            </Project>
            """;

        await verifier.RunAsync();
    }
}

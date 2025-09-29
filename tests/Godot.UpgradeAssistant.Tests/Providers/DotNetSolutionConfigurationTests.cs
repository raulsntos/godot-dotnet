using System.Threading.Tasks;
using Godot.UpgradeAssistant.Providers;

namespace Godot.UpgradeAssistant.Tests;

using Verifier = UpgradeProviderVerifier<DotNetSolutionConfigurationUpgradeProvider, DotNetSolutionConfigurationAnalysisProvider>;

public class DotNetSolutionConfigurationTests
{
    [Fact]
    public static async Task UpgradeSlnToGodotSharpFromDefault()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestSolutionExtension = "sln";
        verifier.TestSolution = """
            {|GUA0005:|}Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            		EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.ActiveCfg = Release|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.Build.0 = Release|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        verifier.FixedSolution = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		ExportDebug|Any CPU = ExportDebug|Any CPU
            		ExportRelease|Any CPU = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.ActiveCfg = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.Build.0 = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.ActiveCfg = ExportRelease|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.Build.0 = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeSlnToGodotSharpFromOldGodotSharp3()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestSolutionExtension = "sln";
        verifier.TestSolution = """
            {|GUA0005:|}Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            		Tools|Any CPU = Tools|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.ActiveCfg = Release|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.Build.0 = Release|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Tools|Any CPU.ActiveCfg = Tools|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Tools|Any CPU.Build.0 = Tools|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        verifier.FixedSolution = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		ExportDebug|Any CPU = ExportDebug|Any CPU
            		ExportRelease|Any CPU = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.ActiveCfg = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.Build.0 = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.ActiveCfg = ExportRelease|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.Build.0 = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeSlnxToGodotSharpFromDefault()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestSolutionExtension = "slnx";
        verifier.TestSolution = """
            {|GUA0005:|}<Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="Release" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        verifier.FixedSolution = """
            <Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="ExportDebug" />
                <BuildType Name="ExportRelease" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeSlnxToGodotSharpFromOldGodotSharp3()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = false;

        verifier.TestSolutionExtension = "slnx";
        verifier.TestSolution = """
            {|GUA0005:|}<Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="Release" />
                <BuildType Name="Tools" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        verifier.FixedSolution = """
            <Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="ExportDebug" />
                <BuildType Name="ExportRelease" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeSlnToGodotDotNetFromDefault()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestSolutionExtension = "sln";
        verifier.TestSolution = """
            {|GUA0005:|}Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            		EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.ActiveCfg = Release|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.Build.0 = Release|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        verifier.FixedSolution = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		ExportDebug|Any CPU = ExportDebug|Any CPU
            		ExportRelease|Any CPU = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.ActiveCfg = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.Build.0 = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.ActiveCfg = ExportRelease|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.Build.0 = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeSlnToGodotDotNetFromOldGodotSharp3()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestSolutionExtension = "sln";
        verifier.TestSolution = """
            {|GUA0005:|}Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            		Tools|Any CPU = Tools|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.ActiveCfg = Release|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Release|Any CPU.Build.0 = Release|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Tools|Any CPU.ActiveCfg = Tools|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Tools|Any CPU.Build.0 = Tools|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        verifier.FixedSolution = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio 2012
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject", "TestProject.csproj", "{84E8D18C-3871-437E-8290-F01063253C59}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		ExportDebug|Any CPU = ExportDebug|Any CPU
            		ExportRelease|Any CPU = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.ActiveCfg = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportDebug|Any CPU.Build.0 = ExportDebug|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.ActiveCfg = ExportRelease|Any CPU
            		{84E8D18C-3871-437E-8290-F01063253C59}.ExportRelease|Any CPU.Build.0 = ExportRelease|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            EndGlobal

            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeSlnxToGodotDotNetFromDefault()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestSolutionExtension = "slnx";
        verifier.TestSolution = """
            {|GUA0005:|}<Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="Release" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        verifier.FixedSolution = """
            <Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="ExportDebug" />
                <BuildType Name="ExportRelease" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        await verifier.RunAsync();
    }

    [Fact]
    public static async Task UpgradeSlnxToGodotDotNetFromOldGodotSharp3()
    {
        var verifier = Verifier.MakeVerifier();
        verifier.TargetGodotVersion = new SemVer(4, 5, 0);
        verifier.IsGodotDotNetEnabled = true;

        verifier.TestSolutionExtension = "slnx";
        verifier.TestSolution = """
            {|GUA0005:|}<Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="Release" />
                <BuildType Name="Tools" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        verifier.FixedSolution = """
            <Solution>
              <Configurations>
                <BuildType Name="Debug" />
                <BuildType Name="ExportDebug" />
                <BuildType Name="ExportRelease" />
              </Configurations>
              <Project Path="TestProject.csproj" />
            </Solution>

            """;

        await verifier.RunAsync();
    }
}

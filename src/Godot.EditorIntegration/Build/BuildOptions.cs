using System.Collections.Generic;
using Godot.EditorIntegration.Build.Cli;

namespace Godot.EditorIntegration.Build;

// For more information about the options listed here, see:
// https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference
// https://github.com/dotnet/sdk/blob/31cdb8caee788678dc5ab809287053a682e123cd/src/Cli/dotnet/commands/dotnet-build/BuildCommandParser.cs
// https://github.com/dotnet/sdk/blob/31cdb8caee788678dc5ab809287053a682e123cd/src/Cli/dotnet/commands/dotnet-publish/PublishCommandParser.cs
// https://github.com/dotnet/sdk/blob/31cdb8caee788678dc5ab809287053a682e123cd/src/Cli/dotnet/commands/dotnet-clean/CleanCommandParser.cs

/// <summary>
/// Options common to all <c>dotnet</c> commands.
/// </summary>
internal abstract class CommonOptions
{
    public required string SlnOrProject { get; set; }
    public string? OutputPath { get; set; }
    public string? TargetFramework { get; set; }
    public string? RuntimeIdentifier { get; set; }
    public string? Configuration { get; set; }
    public List<string> CustomProperties { get; } = [];
    public string? LogsPath { get; set; }
    public VerbosityOption? Verbosity { get; set; }
    public bool NoConsoleLog { get; set; }
    public bool EnableLogger { get; set; }
    public string? LoggerTypeFullName { get; set; }
    public string? LoggerAssemblyPath { get; set; }
    public bool EnableBinaryLog { get; set; }
}

/// <summary>
/// Options common to all building <c>dotnet</c> commands.
/// </summary>
internal abstract class CommonBuildOptions : CommonOptions
{
    public bool NoRestore { get; set; }
    public bool SelfContained { get; set; }
    public string? GetProperty { get; set; }
}

/// <summary>
/// Options for the <c>dotnet build</c> command.
/// </summary>
internal sealed class BuildOptions : CommonBuildOptions
{
    public bool NoIncremental { get; set; }
}

/// <summary>
/// Options for the <c>dotnet publish</c> command.
/// </summary>
internal sealed class PublishOptions : CommonBuildOptions
{
    public bool NoBuild { get; set; }
}

/// <summary>
/// Options for the <c>dotnet clean</c> command.
/// </summary>
internal sealed class CleanOptions : CommonOptions { }

using System.Collections.Generic;
using System.IO;

namespace Godot.EditorIntegration.Build.Cli;

// For more information about the options listed here, see:
// https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference
// https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build
// https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
// https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-clean

/// <summary>
/// Defines the options available in the dotnet CLI that can be used by <see cref="DotNetCli"/>.
/// </summary>
internal static class DotNetCliOptions
{
    public static CliOptionDescriptor<string> SlnOrProject =>
        new((value, args) => args.Add(value));

    public static CliOptionDescriptor<string?> OutputPath =>
        CliOptionDescriptor.FromString("--output");

    public static CliOptionDescriptor<string?> TargetFramework =>
        CliOptionDescriptor.FromString("--framework");

    public static CliOptionDescriptor<string?> RuntimeIdentifier =>
        CliOptionDescriptor.FromString("--runtime");

    public static CliOptionDescriptor<string?> Configuration =>
        CliOptionDescriptor.FromString("--configuration");

    public static CliOptionDescriptor<bool> SelfContained =>
        CliOptionDescriptor.FromBoolean("--self-contained");

    public static CliOptionDescriptor<IEnumerable<string>> CustomProperties =>
        new((value, args) =>
        {
            foreach (string property in value)
            {
                args.Add($"-p:{property}");
            }
        });

    public static CliOptionDescriptor<bool> NoRestore =>
        CliOptionDescriptor.FromBoolean("--no-restore");

    public static CliOptionDescriptor<bool> NoBuild =>
        CliOptionDescriptor.FromBoolean("--no-build");

    public static CliOptionDescriptor<bool> NoIncremental =>
        CliOptionDescriptor.FromBoolean("--no-incremental");

    public static CliOptionDescriptor<VerbosityOption?> Verbosity =>
        new((value, args) =>
        {
            string? verbosity = value switch
            {
                VerbosityOption.Quiet => "quiet",
                VerbosityOption.Minimal => "minimal",
                VerbosityOption.Normal => "normal",
                VerbosityOption.Detailed => "detailed",
                VerbosityOption.Diagnostic => "diagnostic",
                _ => null,
            };

            if (!string.IsNullOrEmpty(verbosity))
            {
                args.Add($"--verbosity:{verbosity}");
            }
        });

    public static CliOptionDescriptor<bool> NoConsoleLog =>
        CliOptionDescriptor.FromBoolean("-noconlog");

    public static CliOptionDescriptor<BuildLoggerOption> Logger =>
        new((value, args) =>
        {
            if (value is not null)
            {
                args.Add($"-l:{value.TypeFullName},{value.AssemblyPath};{value.LogsPath}");
            }
        });

    public static CliOptionDescriptor<string?> BinaryLog =>
        new((value, args) =>
        {
            if (!string.IsNullOrEmpty(value))
            {
                args.Add($"-bl:{Path.Join(value, BuildManager.MSBuildBinaryLogFileName)}");
                args.Add("-ds:False");
            }
        });

    public static CliOptionDescriptor<string?> GetProperty =>
        new((value, args) =>
        {
            if (!string.IsNullOrEmpty(value))
            {
                args.Add($"--getProperty:{value}");
            }
        });
}

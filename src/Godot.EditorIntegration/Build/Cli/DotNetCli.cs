using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Godot.EditorIntegration.Build.Cli;

internal static class DotNetCli
{
    public static ProcessStartInfo CreateBuildStartInfo(BuildOptions options)
    {
        return CreateStartInfoCore([
            "build",
            DotNetCliOptions.SlnOrProject.WithValue(options.SlnOrProject),
            DotNetCliOptions.OutputPath.WithValue(options.OutputPath),
            DotNetCliOptions.TargetFramework.WithValue(options.TargetFramework),
            DotNetCliOptions.RuntimeIdentifier.WithValue(options.RuntimeIdentifier),
            DotNetCliOptions.Configuration.WithValue(options.Configuration),
            DotNetCliOptions.SelfContained.WithValue(options.SelfContained),
            DotNetCliOptions.CustomProperties.WithValue(options.CustomProperties),
            DotNetCliOptions.NoRestore.WithValue(options.NoRestore),
            DotNetCliOptions.NoIncremental.WithValue(options.NoIncremental),
            DotNetCliOptions.Verbosity.WithValue(options.Verbosity),
            DotNetCliOptions.NoConsoleLog.WithValue(options.NoConsoleLog),
            DotNetCliOptions.Logger.WithValueIfEnabled(options.EnableLogger, () =>
            {
                ThrowIfNullWhenOptionEnabled(options.LoggerTypeFullName);
                ThrowIfNullWhenOptionEnabled(options.LoggerAssemblyPath);
                ThrowIfNullWhenOptionEnabled(options.LogsPath);
                return new BuildLoggerOption()
                {
                    TypeFullName = options.LoggerTypeFullName,
                    AssemblyPath = options.LoggerAssemblyPath,
                    LogsPath = options.LogsPath,
                };
            }),
            DotNetCliOptions.BinaryLog.WithValueIfEnabled(options.EnableBinaryLog, () =>
            {
                ThrowIfNullWhenOptionEnabled(options.LogsPath);
                return options.LogsPath;
            }),
        ]);
    }

    public static ProcessStartInfo CreatePublishStartInfo(PublishOptions options)
    {
        return CreateStartInfoCore([
            "publish",
            DotNetCliOptions.SlnOrProject.WithValue(options.SlnOrProject),
            DotNetCliOptions.OutputPath.WithValue(options.OutputPath),
            DotNetCliOptions.TargetFramework.WithValue(options.TargetFramework),
            DotNetCliOptions.RuntimeIdentifier.WithValue(options.RuntimeIdentifier),
            DotNetCliOptions.Configuration.WithValue(options.Configuration),
            DotNetCliOptions.SelfContained.WithValue(options.SelfContained),
            DotNetCliOptions.CustomProperties.WithValue(options.CustomProperties),
            DotNetCliOptions.NoRestore.WithValue(options.NoRestore),
            DotNetCliOptions.NoBuild.WithValue(options.NoBuild),
            DotNetCliOptions.Verbosity.WithValue(options.Verbosity),
            DotNetCliOptions.NoConsoleLog.WithValue(options.NoConsoleLog),
            DotNetCliOptions.Logger.WithValueIfEnabled(options.EnableLogger, () =>
            {
                ThrowIfNullWhenOptionEnabled(options.LoggerTypeFullName);
                ThrowIfNullWhenOptionEnabled(options.LoggerAssemblyPath);
                ThrowIfNullWhenOptionEnabled(options.LogsPath);
                return new BuildLoggerOption()
                {
                    TypeFullName = options.LoggerTypeFullName,
                    AssemblyPath = options.LoggerAssemblyPath,
                    LogsPath = options.LogsPath,
                };
            }),
            DotNetCliOptions.BinaryLog.WithValueIfEnabled(options.EnableBinaryLog, () =>
            {
                ThrowIfNullWhenOptionEnabled(options.LogsPath);
                return options.LogsPath;
            }),
        ]);
    }

    public static ProcessStartInfo CreateCleanStartInfo(CleanOptions options)
    {
        return CreateStartInfoCore([
            "clean",
            DotNetCliOptions.SlnOrProject.WithValue(options.SlnOrProject),
            DotNetCliOptions.OutputPath.WithValue(options.OutputPath),
            DotNetCliOptions.TargetFramework.WithValue(options.TargetFramework),
            DotNetCliOptions.RuntimeIdentifier.WithValue(options.RuntimeIdentifier),
            DotNetCliOptions.Configuration.WithValue(options.Configuration),
            DotNetCliOptions.CustomProperties.WithValue(options.CustomProperties),
            DotNetCliOptions.Verbosity.WithValue(options.Verbosity),
            DotNetCliOptions.NoConsoleLog.WithValue(options.NoConsoleLog),
            DotNetCliOptions.Logger.WithValueIfEnabled(options.EnableLogger, () =>
            {
                ThrowIfNullWhenOptionEnabled(options.LoggerTypeFullName);
                ThrowIfNullWhenOptionEnabled(options.LoggerAssemblyPath);
                ThrowIfNullWhenOptionEnabled(options.LogsPath);
                return new BuildLoggerOption()
                {
                    TypeFullName = options.LoggerTypeFullName,
                    AssemblyPath = options.LoggerAssemblyPath,
                    LogsPath = options.LogsPath,
                };
            }),
            DotNetCliOptions.BinaryLog.WithValueIfEnabled(options.EnableBinaryLog, () =>
            {
                ThrowIfNullWhenOptionEnabled(options.LogsPath);
                return options.LogsPath;
            }),
        ]);
    }

    private static ProcessStartInfo CreateStartInfoCore(IEnumerable<CliOption> options)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            CreateNoWindow = true,
        };

        startInfo.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] =
            ((string)EditorInterface.Singleton.GetEditorSettings().GetSetting("interface/editor/editor_language")).Replace('_', '-');

        // Some computers set the PLATFORM environment variable which conflicts with .NET, so remove it.
        // For more context see: https://github.com/dotnet/arcade/blob/4e73daf131ef02e8264b0d3e850c4bdafe6e02b6/eng/common/build.ps1#L36-L40
        RemovePlatformVariable(startInfo.EnvironmentVariables);

        foreach (var option in options)
        {
            option.AppendArguments(startInfo.ArgumentList);
        }

        return startInfo;
    }

    /// <summary>
    /// Find all the environment variables in <paramref name="environmentVariables"/>
    /// with the key <c>PLATFORM</c> ignoring the case and removes them.
    /// </summary>
    /// <param name="environmentVariables">Dictionary of environment variables.</param>
    private static void RemovePlatformVariable(StringDictionary environmentVariables)
    {
        List<string> platformEnvironmentVariables = [];

        foreach (string env in environmentVariables.Keys)
        {
            if (env.Equals("PLATFORM", StringComparison.OrdinalIgnoreCase))
            {
                platformEnvironmentVariables.Add(env);
            }
        }

        foreach (string env in platformEnvironmentVariables)
        {
            environmentVariables.Remove(env);
        }
    }

    private static void ThrowIfNullWhenOptionEnabled([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null)
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_ParameterCannotBeNullWhenOptionIsEnabled(paramName));
        }
    }
}

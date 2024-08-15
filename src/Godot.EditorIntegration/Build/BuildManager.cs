using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Godot.EditorIntegration.Build.Cli;
using Godot.EditorIntegration.Internals;
using Godot.EditorIntegration.Utils;

namespace Godot.EditorIntegration.Build;

internal static class BuildManager
{
    private static bool _buildInProgress;

    public const string DefaultBuildConfiguration = "Debug";

    // Keep in sync with the constants in `Godot.EditorIntegration.MSBuildLogger.BuildLogger'.
    public const string MSBuildIssuesFileName = "msbuild_issues.csv";
    public const string MSBuildLogFileName = "msbuild_log.txt";
    public const string MSBuildBinaryLogFileName = "msbuild.binlog";

    public delegate void BuildLaunchFailedEventHandler(string reason);

    public static event BuildLaunchFailedEventHandler? BuildLaunchFailed;
    public static event Action<CommonOptions>? BuildStarted;
    public static event Action<BuildResult>? BuildFinished;
    public static event Action<string?>? StdOutputReceived;
    public static event Action<string?>? StdErrorReceived;

    private static void ShowBuildErrorDialog(string message)
    {
        EditorInternal.ShowWarning(message, SR.BuildProjectFailed);
        DotNetEditorPlugin.Singleton.MakeMSBuildPanelVisible();
    }

    private static string GetLogFilePath(string logsDirPath)
    {
        return Path.Join(logsDirPath, MSBuildLogFileName);
    }

    private static string GetIssuesFilePath(string logsDirPath)
    {
        return Path.Join(logsDirPath, MSBuildIssuesFileName);
    }

    private static bool BuildCore<TOptions>(TOptions options, Func<TOptions, ProcessStartInfo> getStartInfo) where TOptions : CommonOptions
    {
        if (_buildInProgress)
        {
            throw new InvalidOperationException(SR.InvalidOperation_BuildAlreadyInProgress);
        }

        _buildInProgress = true;

        Debug.Assert(!string.IsNullOrEmpty(options.LogsPath));

        try
        {
            BuildStarted?.Invoke(options);

            try
            {
                string issuesFile = GetIssuesFilePath(options.LogsPath);
                if (File.Exists(issuesFile))
                {
                    File.Delete(issuesFile);
                }
            }
            catch (IOException e)
            {
                BuildLaunchFailed?.Invoke(SR.FormatBuildManager_CannotRemoveIssuesFile(GetIssuesFilePath(options.LogsPath)));
                Console.Error.WriteLine(e);
            }

            try
            {
                using var process = StartProcess(getStartInfo(options));
                process.WaitForExit();
                int exitCode = process.ExitCode;

                if (exitCode != 0 && OS.Singleton.IsStdOutVerbose())
                {
                    GD.Print(SR.FormatMSBuildExitedWithCode(exitCode, GetLogFilePath(options.LogsPath)));
                }

                BuildFinished?.Invoke(exitCode == 0 ? BuildResult.Success : BuildResult.Error);

                return exitCode == 0;
            }
            catch (Exception e)
            {
                BuildLaunchFailed?.Invoke(SR.FormatMSBuildThrewAnException(e.GetType().FullName, e.Message));
                Console.Error.WriteLine(e);
                return false;
            }
        }
        finally
        {
            _buildInProgress = false;
        }
    }

    public static bool BuildProjectBlocking(BuildOptions options)
    {
        if (!EditorProgress.Invoke("dotnet_build", SR.BuildProjectProgressLabel, 1, progress =>
        {
            string assemblyName = Path.GetFileNameWithoutExtension(options.SlnOrProject);
            string runtimeIdentifier = !string.IsNullOrEmpty(options.RuntimeIdentifier)
                ? options.RuntimeIdentifier
                : RuntimeInformation.RuntimeIdentifier;

            progress.Step(SR.FormatBuildProjectProgressStep(assemblyName, runtimeIdentifier));
            return BuildCore(options, DotNetCli.CreateBuildStartInfo);
        }))
        {
            ShowBuildErrorDialog(SR.BuildProjectFailed);
            return false;
        }

        return true;
    }

    public static bool CleanProjectBlocking(CleanOptions options)
    {
        if (!EditorProgress.Invoke("dotnet_clean", SR.CleanProjectProgressLabel, 1, progress =>
        {
            string assemblyName = Path.GetFileNameWithoutExtension(options.SlnOrProject);

            progress.Step(SR.FormatCleanProjectProgressStep(assemblyName));
            return BuildCore(options, DotNetCli.CreateCleanStartInfo);
        }))
        {
            ShowBuildErrorDialog(SR.CleanProjectFailed);
            return false;
        }

        return true;
    }

    public static bool PublishProject(PublishOptions options)
    {
        return BuildCore(options, DotNetCli.CreatePublishStartInfo);
    }

    internal static Process StartProcess(ProcessStartInfo startInfo)
    {
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.StandardOutputEncoding = Encoding.UTF8;
        startInfo.StandardErrorEncoding = Encoding.UTF8;

        string cmdDisplay = startInfo.GetCommandLineDisplay();
        StdOutputReceived?.Invoke(SR.FormatBuildManager_RunningCommand(cmdDisplay));
        if (OS.Singleton.IsStdOutVerbose())
        {
            Console.WriteLine(cmdDisplay);
        }

        var process = new Process() { StartInfo = startInfo };

        if (StdOutputReceived is not null)
        {
            process.OutputDataReceived += (_, e) => StdOutputReceived.Invoke(e.Data);
        }
        if (StdErrorReceived is not null)
        {
            process.ErrorDataReceived += (_, e) => StdErrorReceived.Invoke(e.Data);
        }

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }
}

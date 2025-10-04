using Microsoft.Build.Framework;
using Serilog;

namespace Godot.UpgradeAssistant.Cli.Logging;

internal sealed class BuildLogger : Microsoft.Build.Framework.ILogger
{
    public LoggerVerbosity Verbosity { get; set; }

    public string? Parameters { get; set; }

    public void Initialize(IEventSource eventSource)
    {
        if (Verbosity > LoggerVerbosity.Normal)
        {
            eventSource.MessageRaised += LogVerbose;
        }
        else
        {
            eventSource.BuildStarted += LogVerbose;
            eventSource.BuildFinished += LogVerbose;
        }

        eventSource.WarningRaised += LogWarning;
        eventSource.ErrorRaised += LogError;
        eventSource.CustomEventRaised += LogVerbose;
    }

    private void LogInformation(object? sender, BuildEventArgs e)
    {
        Log.Information(e.Message ?? "");
    }

    private void LogWarning(object? sender, BuildEventArgs e)
    {
        Log.Warning(e.Message ?? "");
    }

    private void LogError(object? sender, BuildEventArgs e)
    {
        Log.Error(e.Message ?? "");
    }

    private void LogVerbose(object? sender, BuildEventArgs e)
    {
        Log.Verbose(e.Message ?? "");
    }

    public void Shutdown() { }
}

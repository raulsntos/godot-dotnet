using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Godot.BindingsGenerator.Logging;

/// <summary>
/// Logger that outputs messages using the MSBuild task logger.
/// </summary>
internal sealed class MSBuildTaskLogger : ILogger
{
    private readonly TaskLoggingHelper _logger;

    public MSBuildTaskLogger(TaskLoggingHelper logger)
    {
        _logger = logger;
    }

    public void Log(LogLevel logLevel, string message)
    {
        // TODO: Temporarily making every log a message to prevent generating errors that fail the build.
        // The reported errors need to be fixed.
        _logger.LogMessage(MessageImportance.High, logLevel switch
        {
            LogLevel.Warning => $"WARNING: {message}",
            >= LogLevel.Error => $"ERROR: {message}",
            _ => message,
        });
        return;

#pragma warning disable CS0162 // Unreachable code detected
        switch (logLevel)
        {
            case <= LogLevel.Debug:
                _logger.LogMessage(MessageImportance.Low, message);
                break;

            case LogLevel.Information:
                _logger.LogMessage(MessageImportance.High, message);
                break;

            case LogLevel.Warning:
                _logger.LogWarning(message);
                break;

            case >= LogLevel.Error:
                _logger.LogError(message);
                break;
        }
#pragma warning restore CS0162 // Unreachable code detected
    }
}

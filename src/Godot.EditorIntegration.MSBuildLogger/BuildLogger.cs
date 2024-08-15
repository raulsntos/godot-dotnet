using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.Build.Framework;

namespace Godot.EditorIntegration.MSBuildLogger;

// IMPORTANT: This class must be public so MSBuild can find it.
/// <summary>
/// Implementation of <see cref="ILogger"/> used by the editor integration when building .NET projects
/// to collect diagnostics and display them in Godot.
/// </summary>
public sealed class BuildLogger : ILogger, IDisposable
{
    // Keep in sync with the constants in 'Godot.EditorIntegration.Build.BuildManager'.
    private const string MSBuildIssuesFileName = "msbuild_issues.csv";
    private const string MSBuildLogFileName = "msbuild_log.txt";
    private const string MSBuildBinaryLogFileName = "msbuild.binlog";

    /// <inheritdoc/>
    public string? Parameters { get; set; }

    /// <inheritdoc/>
    public LoggerVerbosity Verbosity { get; set; }

    private IndentedTextWriter? _logWriter;
    private TextWriter? _issuesWriter;

    /// <inheritdoc/>
    public void Initialize(IEventSource eventSource)
    {
        if (string.IsNullOrEmpty(Parameters))
        {
            throw new LoggerException(SR.Logger_LogDirectoryParameterNotSpecified);
        }

        string[] parameters = Parameters.Split(';');

        string logDir = parameters[0];

        if (string.IsNullOrEmpty(logDir))
        {
            throw new LoggerException(SR.Logger_LogDirectoryParameterIsEmpty);
        }

        if (parameters.Length > 1)
        {
            throw new LoggerException(SR.Logger_TooManyParametersPassed);
        }

        string logFile = Path.Join(logDir, MSBuildLogFileName);
        string issuesFile = Path.Join(logDir, MSBuildIssuesFileName);

        try
        {
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            _logWriter = new IndentedTextWriter(new StreamWriter(logFile), "\t");
            _issuesWriter = new StreamWriter(issuesFile);
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException
             || ex is ArgumentNullException
             || ex is PathTooLongException
             || ex is DirectoryNotFoundException
             || ex is NotSupportedException
             || ex is ArgumentException
             || ex is SecurityException
             || ex is IOException)
            {
                throw new LoggerException(SR.FormatLogger_CreateLogFileFailed(ex.Message));
            }

            // Unexpected failure.
            throw;
        }

        eventSource.ProjectStarted += EventSource_ProjectStarted;
        eventSource.ProjectFinished += EventSource_ProjectFinished;
        eventSource.MessageRaised += EventSource_MessageRaised;
        eventSource.WarningRaised += EventSource_WarningRaised;
        eventSource.ErrorRaised += EventSource_ErrorRaised;
    }

    private void EventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
    {
        WriteLine(e.Message);
        if (_logWriter is not null)
        {
            _logWriter.Indent++;
        }
    }

    private void EventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
    {
        if (_logWriter is not null)
        {
            _logWriter.Indent--;
        }
        WriteLine(e.Message);
    }

    private void LogBuildEvent(string file, int lineNumber, int columnNumber, string severity, string diagnosticId, string? message, string? projectFile)
    {
        var sb = new StringBuilder();

        // Log event to the log file.
        sb.Append(file);
        sb.Append(CultureInfo.InvariantCulture, $"({lineNumber},{columnNumber})");
        sb.Append(CultureInfo.InvariantCulture, $": {severity} {diagnosticId}: {message}");
        if (string.IsNullOrEmpty(projectFile))
        {
            sb.Append(CultureInfo.InvariantCulture, $" [{projectFile}]");
        }
        WriteLine(sb.ToString());

        sb.Clear();

        // Log event to the CSV file.
        sb.Append(severity);
        sb.Append(',');
        sb.Append(CsvEscape(file));
        sb.Append(',');
        sb.Append(lineNumber);
        sb.Append(',');
        sb.Append(columnNumber);
        sb.Append(',');
        sb.Append(CsvEscape(diagnosticId));
        sb.Append(',');
        sb.Append(CsvEscape(message));
        sb.Append(',');
        sb.Append(CsvEscape(projectFile));
        _issuesWriter?.WriteLine(sb.ToString());
    }

    private void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
    {
        LogBuildEvent(e.File, e.LineNumber, e.ColumnNumber, "error", e.Code, e.Message, e.ProjectFile);
    }

    private void EventSource_WarningRaised(object sender, BuildWarningEventArgs e)
    {
        LogBuildEvent(e.File, e.LineNumber, e.ColumnNumber, "warning", e.Code, e.Message, e.ProjectFile);
    }

    private void EventSource_MessageRaised(object sender, BuildMessageEventArgs e)
    {
        // BuildMessageEventArgs adds Importance to BuildEventArgs.
        // Let's take account of the verbosity setting we've been passed in deciding whether to log the message.
        if (e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal)
         || e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal)
         || e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            WriteLineWithSenderAndMessage(string.Empty, e);
        }

        bool IsVerbosityAtLeast(LoggerVerbosity checkVerbosity)
        {
            return Verbosity >= checkVerbosity;
        }
    }

    /// <summary>
    /// Write a line to the log, adding the SenderName and Message
    /// (these parameters are on all MSBuild event argument objects)
    /// </summary>
    private void WriteLineWithSenderAndMessage(string line, BuildEventArgs e)
    {
        if (string.Equals(e.SenderName, "MSBuild", StringComparison.OrdinalIgnoreCase))
        {
            // Well, if the sender name is MSBuild, let's leave it out for prettiness.
            WriteLine($"{line}{e.Message}");
        }
        else
        {
            WriteLine($"{e.SenderName}: {line}{e.Message}");
        }
    }

    private void WriteLine(string line)
    {
        _logWriter?.WriteLine(line);
    }

    /// <inheritdoc/>
    public void Shutdown()
    {
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _logWriter?.Dispose();
        _issuesWriter?.Dispose();
    }

    [return: NotNullIfNotNull(nameof(value))]
    private static string? CsvEscape(string? value, char delimiter = ',')
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        bool hasSpecialChar = value.IndexOfAny(['\"', '\n', '\r', delimiter]) != -1;

        if (hasSpecialChar)
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

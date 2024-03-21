using System;

namespace Godot.BindingsGenerator.Logging;

/// <summary>
/// Logger that outputs messages to the console (stdout and stderr).
/// </summary>
internal sealed class ConsoleLogger : ILogger
{
    public static ConsoleLogger Instance { get; } = new();

    private ConsoleLogger() { }

    public void Log(LogLevel logLevel, string message)
    {
        switch (logLevel)
        {
            case <= LogLevel.Information:
                LogInformation(message);
                break;

            case LogLevel.Warning:
                LogWarning(message);
                break;

            case >= LogLevel.Error:
                LogError(message);
                break;
        }
    }

    private static void LogInformation(string message)
    {
        Console.WriteLine(message);
    }

    private static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Error.WriteLine(message);
        Console.ResetColor();
    }

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(message);
        Console.ResetColor();
    }
}

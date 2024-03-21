namespace Godot.BindingsGenerator.Logging;

internal static class Logger
{
    public static void LogTrace(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Trace, message);
    }

    public static void LogDebug(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Debug, message);
    }

    public static void LogInformation(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Information, message);
    }

    public static void LogWarning(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Warning, message);
    }

    public static void LogError(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Error, message);
    }

    public static void LogCritical(this ILogger logger, string message)
    {
        logger.Log(LogLevel.Critical, message);
    }
}

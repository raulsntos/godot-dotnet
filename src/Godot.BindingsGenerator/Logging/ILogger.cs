namespace Godot.BindingsGenerator.Logging;

internal interface ILogger
{
    public void Log(LogLevel logLevel, string message);
}

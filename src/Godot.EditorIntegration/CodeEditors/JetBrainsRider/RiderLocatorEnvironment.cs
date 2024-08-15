using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Rider.PathLocator;

namespace Godot.EditorIntegration.CodeEditors;

internal sealed class RiderLocatorEnvironment : IRiderLocatorEnvironment
{
    public JetBrains.Rider.PathLocator.OS CurrentOS
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return JetBrains.Rider.PathLocator.OS.Windows;
            }
            if (OperatingSystem.IsMacOS())
            {
                return JetBrains.Rider.PathLocator.OS.MacOSX;
            }
            if (OperatingSystem.IsLinux())
            {
                return JetBrains.Rider.PathLocator.OS.Linux;
            }
            return JetBrains.Rider.PathLocator.OS.Other;
        }
    }

    public T? FromJson<T>([StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }

    public void Info(string message, Exception? e = null)
    {
        GD.Print($"{message} {e}");
    }

    public void Warn(string message, Exception? e = null)
    {
        GD.PushWarning($"{message} {e}");
    }

    public void Error(string message, Exception? e = null)
    {
        GD.PushError($"{message} {e}");
    }

    public void Verbose(string message, Exception? e = null)
    {
        if (OS.Singleton.IsStdOutVerbose())
        {
            GD.Print($"{message} {e}");
        }
    }
}

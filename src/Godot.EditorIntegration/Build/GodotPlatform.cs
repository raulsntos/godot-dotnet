using System;

namespace Godot.EditorIntegration.Build;

/// <summary>
/// Helper to determine the Godot platform from the OS name used by the export platform preset.
/// </summary>
internal static class GodotPlatform
{
    public static bool IsWindows(string platform)
    {
        // Must match the OS name in EditorExportPlatformWindows.
        return platform.Equals("Windows", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsLinux(string platform)
    {
        // Must match the OS name in EditorExportPlatformLinuxBSD.
        return platform.Equals("Linux", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsAndroid(string platform)
    {
        // Must match the OS name in EditorExportPlatformAndroid.
        return platform.Equals("Android", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsMacOS(string platform)
    {
        // Must match the OS name in EditorExportPlatformMacOS.
        return platform.Equals("macOS", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsIOS(string platform)
    {
        // Must match the OS name in EditorExportPlatformIOS.
        return platform.Equals("iOS", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsWeb(string platform)
    {
        // Must match the OS name in EditorExportPlatformWeb.
        return platform.Equals("Web", StringComparison.OrdinalIgnoreCase);
    }
}

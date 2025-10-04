using System;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.UpgradeAssistant.Providers;

internal static class AnalyzerUtils
{
    public static bool IsGodotDotNetEnabled(this AnalyzerOptions options)
    {
        var globalOptions = options.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (globalOptions.TryGetValue($"build_property.{PropertyNames.IsGodotDotNetEnabled}", out string? value))
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public static SemVer GetTargetGodotVersion(this AnalyzerOptions options)
    {
        var globalOptions = options.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (globalOptions.TryGetValue($"build_property.{PropertyNames.TargetGodotVersion}", out string? value))
        {
            return SemVer.Parse(value);
        }

        return Constants.LatestGodotVersion;
    }
}

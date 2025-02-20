using System;
using System.IO;
using Godot.EditorIntegration.Build.Cli;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Build;

internal static class BuildOptionsExtensions
{
    public static TOptions WithGodotCommonDefaults<TOptions>(this TOptions options) where TOptions : CommonOptions
    {
        var editorSettings = EditorInterface.Singleton.GetEditorSettings();

        options.Verbosity = editorSettings.GetSetting(EditorSettingNames.VerbosityLevel).As<VerbosityOption>();
        options.NoConsoleLog = (bool)editorSettings.GetSetting(EditorSettingNames.NoConsoleLogging);

        // We want to force NoConsoleLog to true when Verbosity is Detailed or above.
        // At that point, there's so much info logged that it doesn't make sense to display it in
        // the tiny editor window, and it'd make the editor hang or crash anyway.
        if (options.Verbosity >= VerbosityOption.Detailed)
        {
            options.NoConsoleLog = true;
        }

        options.EnableLogger = true;
        options.LoggerTypeFullName = "Godot.EditorIntegration.MSBuildLogger.BuildLogger";
        options.LoggerAssemblyPath = Path.Join(EditorPath.EditorAssembliesPath, "Godot.EditorIntegration.MSBuildLogger", "Godot.EditorIntegration.MSBuildLogger.dll");
        options.EnableBinaryLog = (bool)editorSettings.GetSetting(EditorSettingNames.CreateBinaryLog);

        // TODO: This gets the feature from the running editor which is fine for project builds
        // but not for exports. For exports we should take this from the template, so you can use a
        // single-precision build of the editor to export with a double-precision build of the templates.
        // See: https://github.com/godotengine/godot/pull/84711
        options.WithGodotFloat64Property(OS.Singleton.HasFeature("double"));
        return options;
    }

    public static TOptions WithGodotDebugDefaults<TOptions>(this TOptions options) where TOptions : CommonOptions
    {
        options.Configuration = BuildManager.DefaultBuildConfiguration;
        options.LogsPath = EditorPath.GetLogsDirPathFor(options.Configuration);
        options.WithGodotCommonDefaults();
        return options;
    }

    public static TOptions WithDebugSymbolsProperties<TOptions>(this TOptions options, bool includeDebugSymbols) where TOptions : CommonOptions
    {
        if (!includeDebugSymbols)
        {
            options.CustomProperties.Add("DebugType=None");
            options.CustomProperties.Add("DebugSymbols=false");
        }

        return options;
    }

    public static TOptions WithGodotTargetPlatformProperty<TOptions>(this TOptions options, string platform) where TOptions : CommonOptions
    {
        ArgumentException.ThrowIfNullOrEmpty(platform);
        options.CustomProperties.Add($"GodotTargetPlatform={platform}");
        return options;
    }

    private static TOptions WithGodotFloat64Property<TOptions>(this TOptions options, bool value) where TOptions : CommonOptions
    {
        if (value)
        {
            options.CustomProperties.Add("GodotFloat64=true");
        }

        return options;
    }
}

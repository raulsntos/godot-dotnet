using System.Reflection;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Common values used by the upgrade assistant.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The latest Godot version supported as a target version for an upgrade.
    /// </summary>
    public static SemVer LatestGodotVersion { get; } = GetAssemblyVersion();

    /// <summary>
    /// The minimum Godot version supported as a target version for an upgrade.
    /// </summary>
    public static SemVer MinSupportedGodotVersion { get; } = new(4, 0, 0);

    // TODO: Update this with the actual first version once we release the first version of Godot .NET.
    /// <summary>
    /// The first Godot version to support the Godot .NET bindings.
    /// </summary>
    public static SemVer FirstSupportedGodotDotNetVersion { get; } = new(4, 5, 0);

    // TODO: Update this with the actual last version once we drop support for GodotSharp.
    /// <summary>
    /// The last Godot version to support the GodotSharp bindings.
    /// </summary>
    public static SemVer LastSupportedGodotSharpVersion { get; } = new(42, 42, 42);

    /// <summary>
    /// Assembly name for the Godot MSBuild SDK.
    /// </summary>
    public const string GodotSdkAssemblyName = "Godot.NET.Sdk";

    private static SemVer GetAssemblyVersion()
    {
        var assembly = typeof(Constants).Assembly;
        var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttribute is not null)
        {
            string version = versionAttribute.InformationalVersion;
            return SemVer.Parse(version);
        }

        return default;
    }
}

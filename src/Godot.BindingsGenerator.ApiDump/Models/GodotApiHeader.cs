namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Contains the version of Godot that the GDExtension API information was dumped from.
/// </summary>
public class GodotApiHeader
{
    /// <summary>
    /// Major section of the version (e.g.: the '4' in '4.1.0').
    /// </summary>
    [JsonPropertyName("version_major")]
    public required int VersionMajor { get; set; }

    /// <summary>
    /// Minor section of the version (e.g.: the '1' in '4.1.0').
    /// </summary>
    [JsonPropertyName("version_minor")]
    public required int VersionMinor { get; set; }

    /// <summary>
    /// Patch section of the version (e.g.: the '0' in '4.1.0').
    /// </summary>
    [JsonPropertyName("version_patch")]
    public int VersionPatch { get; set; }

    /// <summary>
    /// Status section of the version (e.g.: 'dev', 'beta', 'stable').
    /// </summary>
    [JsonPropertyName("version_status")]
    public string VersionStatus { get; set; } = string.Empty;

    /// <summary>
    /// Build section of the version (e.g.: 'official', 'custom_build').
    /// </summary>
    [JsonPropertyName("version_build")]
    public string VersionBuild { get; set; } = string.Empty;

    /// <summary>
    /// Full version in a formatted string (e.g.: 'Godot v4.1.0.stable.official.mono').
    /// </summary>
    [JsonPropertyName("version_full_name")]
    public string VersionFullName { get; set; } = string.Empty;
}

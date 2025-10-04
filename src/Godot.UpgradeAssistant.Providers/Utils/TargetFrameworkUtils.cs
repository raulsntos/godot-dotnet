using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using NuGet.Frameworks;

namespace Godot.UpgradeAssistant.Providers;

internal static partial class TargetFrameworkUtils
{
    private sealed class GodotTfmsData
    {
        /// <summary>
        /// TFM entries that contain information about the required TFM
        /// for each Godot version.
        /// </summary>
        public GodotTfmEntry[] Tfms { get; set; } = [];
    }

    private sealed class GodotTfmEntry
    {
        /// <summary>
        /// TFM required by Godot.
        /// </summary>
        public string Tfm { get; set; } = "";

        /// <summary>
        /// The Godot version that introduced the TFM requirement.
        /// </summary>
        public string Version { get; set; } = "";

        /// <summary>
        /// The Godot platform that the TFM requirement applies to.
        /// Or <see langword="null"/> if it applies to all platforms.
        /// </summary>
        public string? Platform { get; set; }
    }

    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        UseStringEnumConverter = true
    )]
    [JsonSerializable(typeof(GodotTfmsData))]
    [JsonSerializable(typeof(GodotTfmEntry))]
    private sealed partial class GodotTfmsDataJsonContext : JsonSerializerContext { }

    private const string TargetFrameworkNodeName = "TargetFramework";
    private const string TargetFrameworksNodeName = "TargetFrameworks";

    [GeneratedRegex(@"\s*'\$\(GodotTargetPlatform\)'\s*==\s*'(?<platform>[A-Za-z]+)'\s*", RegexOptions.IgnoreCase)]
    private static partial Regex GodotTargetPlatformConditionRegex { get; }

    private static readonly string[] _godotPlatformNames =
    [
        "windows",
        "linuxbsd",
        "macos",
        "android",
        "ios",
        "web",
    ];

    private static bool IsTargetFrameworkProperty(this ProjectPropertyElement property)
    {
        return property.ElementName.Equals(TargetFrameworkNodeName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTargetFrameworksProperty(this ProjectPropertyElement property)
    {
        return property.ElementName.Equals(TargetFrameworksNodeName, StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<ProjectPropertyElement> GetTargetFrameworkProperties(this ProjectRootElement projectRoot)
    {
        return projectRoot.Properties
            .Where(p =>
            {
                return p.IsTargetFrameworkProperty()
                    || p.IsTargetFrameworksProperty();
            });
    }

    public static bool IsMultiTargeting(this ProjectPropertyElement tfmProperty)
    {
        return tfmProperty switch
        {
            _ when tfmProperty.IsTargetFrameworksProperty() => true,
            _ when tfmProperty.IsTargetFrameworkProperty() => false,
            _ => throw new ArgumentException(SR.Argument_InvalidPropertyElementNotTfm, nameof(tfmProperty)),
        };
    }

    public static NuGetFramework GetTargetFramework(ProjectPropertyElement tfmProperty)
    {
        if (IsMultiTargeting(tfmProperty))
        {
            throw new ArgumentException(SR.Argument_InvalidPropertyElementNotTfm, nameof(tfmProperty));
        }

        return NuGetFramework.Parse(tfmProperty.Value);
    }

    public static NuGetFramework[] GetTargetFrameworks(ProjectPropertyElement tfmProperty)
    {
        if (IsMultiTargeting(tfmProperty))
        {
            // We are in a multi-targeting scenario.
            string[] tfmValues = tfmProperty.Value.Split(';');
            var tfms = new NuGetFramework[tfmValues.Length];
            for (int i = 0; i < tfmValues.Length; i++)
            {
                tfms[i] = NuGetFramework.Parse(tfmValues[i]);
            }
            return tfms;
        }

        return [NuGetFramework.Parse(tfmProperty.Value)];
    }

    private static GodotTfmsData? _cachedTfmsData;
    private static GodotTfmsData? _cachedGodotDotNetTfmsData;

    private static ref GodotTfmsData? GetTfmData(bool isGodotDotNetEnabled)
    {
        if (isGodotDotNetEnabled)
        {
            return ref _cachedGodotDotNetTfmsData;
        }

        return ref _cachedTfmsData;
    }

    private static async Task Initialize(bool isGodotDotNetEnabled = false, CancellationToken cancellationToken = default)
    {
        Debug.Assert(GetTfmData(isGodotDotNetEnabled) is null, "TFM data is already initialized.");

        var assembly = typeof(TargetFrameworkUtils).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith("Godot.UpgradeAssistant.Providers.Assets.GodotTfms.", StringComparison.Ordinal));

        List<GodotTfmsData> data = [];

        foreach (string name in resourceNames)
        {
            if (name.Contains(".GodotDotNet.", StringComparison.Ordinal) != isGodotDotNetEnabled)
            {
                // When not targeting the Godot .NET packages, skip the group of entries with special naming.
                // When targeting the Godot .NET packages, skip everything but the entries with special naming.
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(name);
            Debug.Assert(stream is not null);

            var entries = await JsonSerializer.DeserializeAsync(stream, typeof(GodotTfmsData), GodotTfmsDataJsonContext.Default, cancellationToken: cancellationToken).ConfigureAwait(false) as GodotTfmsData;
            Debug.Assert(entries is not null);

            data.Add(entries);
        }

        Debug.Assert(data.Count == 1, "TFM data defined in multiple files.");

        GetTfmData(isGodotDotNetEnabled) = data[0];
    }

    /// <summary>
    /// Get the required target framework version for the specified version of Godot
    /// and target platform. The Godot .NET packages may require a different version
    /// than the GodotSharp packages.
    /// </summary>
    /// <param name="targetGodotVersion">
    /// The target version of Godot that determines the minimum required target framework
    /// version.
    /// </param>
    /// <param name="godotPlatform">
    /// The target platform that determines the minimum required target framework version,
    /// or <see langword="null"/> to obtain the version common to all platforms.
    /// </param>
    /// <param name="isGodotDotNetEnabled">
    /// Whether the Godot .NET packages will be used. A different target framework version
    /// may be required when the Godot .NET packages are used instead of the GodotSharp packages.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The minimum required target framework version that matches the requirements specified
    /// by the given parameters.
    /// </returns>
    public static async ValueTask<NuGetFramework?> GetRequiredTargetFrameworkAsync(SemVer targetGodotVersion, string? godotPlatform = null, bool isGodotDotNetEnabled = false, CancellationToken cancellationToken = default)
    {
        if (GetTfmData(isGodotDotNetEnabled) is null)
        {
            await Initialize(isGodotDotNetEnabled, cancellationToken).ConfigureAwait(false);
        }
        if (GetTfmData(isGodotDotNetEnabled) is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_GodotTfmInformationNotAvailable);
        }

        var data = GetTfmData(isGodotDotNetEnabled)!;

        string? requiredTfm = null;
        SemVer firstVersion = default;
        foreach (var currentEntry in data.Tfms)
        {
            // If this TFM entry only applies to a specific platform.
            if (!string.IsNullOrEmpty(currentEntry.Platform))
            {
                if (string.IsNullOrEmpty(godotPlatform))
                {
                    // There is no specified platform so the TFM applies to all platforms,
                    // ignore TFM entries that are platform-specific.
                    continue;
                }

                if (!godotPlatform.Equals(currentEntry.Platform, StringComparison.OrdinalIgnoreCase))
                {
                    // This TFM entry doesn't apply to the specified platform.
                    continue;
                }
            }

            var currentFirstVersion = SemVer.Parse(currentEntry.Version);
            if (currentFirstVersion > targetGodotVersion)
            {
                // TFM does not apply to our target version.
                continue;
            }

            if (currentFirstVersion > firstVersion)
            {
                firstVersion = currentFirstVersion;
                requiredTfm = currentEntry.Tfm;
            }
        }

        if (string.IsNullOrEmpty(requiredTfm))
        {
            // This can only happen when attempting to upgrade to a target Godot version below the minimum supported,
            // so the GodotTfms resource won't have an entry for that version. But this is not possible because
            // the CLI should have checked much earlier that the target version is not supported.
            throw new UnreachableException($"No TFM data for target Godot version '{targetGodotVersion}'.");
        }

        return NuGetFramework.Parse(requiredTfm);
    }

    /// <summary>
    /// Check whether the property or its parent <c>PropertyGroup</c>
    /// has a non-empty condition.
    /// </summary>
    /// <param name="property">The property element.</param>
    /// <returns>
    /// <see langword="true"/> if the property or its parent has a condition.
    /// </returns>
    public static bool IsConditioned(this ProjectPropertyElement property)
    {
        return !string.IsNullOrEmpty(property.Condition)
            || !string.IsNullOrEmpty(property.Parent.Condition);
    }

    private static bool ConditionMatchesGodotPlatform(string condition, [NotNullWhen(true)] out string? platform)
    {
        // Check if the condition is checking the 'GodotTargetPlatform' for one of the
        // Godot platforms with built-in support in the Godot.NET.Sdk.
        var match = GodotTargetPlatformConditionRegex.Match(condition);
        if (match.Success)
        {
            platform = match.Groups["platform"].Value;
            return _godotPlatformNames.Contains(platform, StringComparer.OrdinalIgnoreCase);
        }

        platform = null;
        return false;
    }

    public static bool ConditionMatchesGodotPlatform(this ProjectPropertyElement tfmProperty, [NotNullWhen(true)] out string? platform)
    {
        if (ConditionMatchesGodotPlatform(tfmProperty.Condition, out platform))
        {
            return true;
        }

        if (ConditionMatchesGodotPlatform(tfmProperty.Parent.Condition, out platform))
        {
            return true;
        }

        platform = null;
        return false;
    }
}

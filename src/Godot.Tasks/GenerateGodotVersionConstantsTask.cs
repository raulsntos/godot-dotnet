using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Versioning;

namespace Godot.Tasks;

/// <summary>
/// MSBuild task to generate Godot version constants.
/// </summary>
public class GenerateGodotVersionConstantsTask : Task
{
    /// <summary>
    /// The generated version constants.
    /// </summary>
    [Output]
    public string GodotVersionConstants { get; set; } = "";

    /// <summary>
    /// Execute the MSBuild task.
    /// </summary>
    /// <returns><see langword="true"/> if the task was successful.</returns>
    public override bool Execute()
    {
        try
        {
            if (!TryGetGodotVersion(out var version))
            {
                Log.LogError("Failed to retrieve Godot version.");
                return false;
            }

            var constants = GenerateVersionConstants(version.Major, version.Minor, version.Patch);
            GodotVersionConstants = string.Join(";", constants);
            Log.LogMessage(MessageImportance.Low, $"Generated Godot version constants: {GodotVersionConstants}");
            return true;
        }
        catch (Exception e)
        {
            Log.LogError($"Error generating Godot version constants: {e}");
            return false;
        }
    }

    private static bool TryGetGodotVersion([NotNullWhen(true)] out NuGetVersion? version)
    {
        // Get the Godot version from this assembly's informational version.
        // As long as the version of this package being used matches the Godot version, this should be sufficient.
        var assembly = typeof(GenerateGodotVersionConstantsTask).Assembly;
        var assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string? informationVersion = assemblyInformationalVersionAttribute?.InformationalVersion;
        if (string.IsNullOrEmpty(informationVersion))
        {
            version = null;
            return false;
        }

        version = NuGetVersion.Parse(informationVersion);
        return version is not null;
    }

    private static List<string> GenerateVersionConstants(int major, int minor, int patch)
    {
        List<string> constants = [];

        constants.Add($"GODOT{major}");
        constants.Add($"GODOT{major}_{minor}");
        constants.Add($"GODOT{major}_{minor}_{patch}");

        // Major version "or greater" constants (e.g., GODOT4_OR_GREATER, GODOT5_OR_GREATER, etc.)
        for (int v = 4; v <= major; v++)
        {
            constants.Add($"GODOT{v}_OR_GREATER");
        }

        // Minor version "or greater" constants (e.g., GODOT4_0_OR_GREATER, GODOT4_1_OR_GREATER, etc.)
        for (int v = 0; v <= minor; v++)
        {
            constants.Add($"GODOT{major}_{v}_OR_GREATER");
        }

        // Patch version "or greater" constants (e.g., GODOT4_5_0_OR_GREATER, GODOT4_5_1_OR_GREATER, etc.)
        for (int v = 0; v <= patch; v++)
        {
            constants.Add($"GODOT{major}_{minor}_{v}_OR_GREATER");
        }

        return constants;
    }
}

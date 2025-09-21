using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.ProjectModel;

namespace Godot.Tasks;

/// <summary>
/// MSBuild task to check for incompatible Godot package references.
/// </summary>
public class CheckGodotPackageCompatibilityTask : Task
{
    /// <summary>
    /// The target framework moniker to use when checking for package compatibility.
    /// </summary>
    [Required]
    public string TargetFramework { get; set; } = "";

    /// <summary>
    /// The runtime identifier to use when checking for package compatibility.
    /// </summary>
    public string? RuntimeIdentifier { get; set; }

    /// <summary>
    /// Path to the project.assets.json file.
    /// </summary>
    [Required]
    public string ProjectAssetsFile { get; set; } = "";

    /// <summary>
    /// Indicates whether the project uses Godot .NET instead of GodotSharp.
    /// </summary>
    public bool UsesGodotDotNet { get; set; }

    /// <summary>
    /// Execute the MSBuild task.
    /// </summary>
    /// <returns><see langword="true"/> if the task was successful.</returns>
    public override bool Execute()
    {
        try
        {
            string activeBindingsName = "Godot .NET";
            string inactiveBindingsName = "GodotSharp";
            if (!UsesGodotDotNet)
            {
                (activeBindingsName, inactiveBindingsName) = (inactiveBindingsName, activeBindingsName);
            }

            var lockFileFormat = new LockFileFormat();
            var assetsFile = lockFileFormat.Read(ProjectAssetsFile);

            var projectReferenceNamesByPath = GetProjectReferenceNamesByPath(assetsFile);

            List<string> topLevelReferences =
            [
                .. GetTopLevelPackageReferences(TargetFramework, assetsFile),
                .. GetTopLevelProjectReferences(TargetFramework, assetsFile, projectReferenceNamesByPath),
            ];

            var packageLibraries = GetAllLibrariesForFramework(TargetFramework, RuntimeIdentifier, assetsFile).ToArray();

            foreach (string topLevelReference in topLevelReferences)
            {
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (HasIncompatibleDependencies(topLevelReference, packageLibraries, visited))
                {
                    Log.LogError($"The project references '{topLevelReference}' which contains {inactiveBindingsName} packages that are incompatible with the active Godot C# bindings. The active C# bindings is {activeBindingsName}.");
                }
            }

            return !Log.HasLoggedErrors;
        }
        catch (Exception e)
        {
            Log.LogError($"Error checking Godot package compatibility: {e}");
            return false;
        }
    }

    private static Dictionary<string, string> GetProjectReferenceNamesByPath(LockFile assetsFile)
    {
        string? projectDirectoryPath = Path.GetDirectoryName(assetsFile.PackageSpec.FilePath);
        if (string.IsNullOrEmpty(projectDirectoryPath))
        {
            return [];
        }

        Dictionary<string, string> map = [];

        foreach (var library in assetsFile.Libraries)
        {
            if (library.Type != "project")
            {
                continue;
            }

            map.Add(Path.GetFullPath(library.Path, projectDirectoryPath), library.Name);
        }

        return map;
    }

    private static IEnumerable<string> GetTopLevelPackageReferences(string targetFrameworkMoniker, LockFile assetsFile)
    {
        var targetFrameworkInformation = assetsFile.PackageSpec.TargetFrameworks.FirstOrDefault(tfi => tfi.TargetAlias.Equals(targetFrameworkMoniker, StringComparison.OrdinalIgnoreCase));
        if (targetFrameworkInformation is null)
        {
            return [];
        }

        return targetFrameworkInformation.Dependencies.Select(d => d.Name);
    }

    private static IEnumerable<string> GetTopLevelProjectReferences(string targetFrameworkMoniker, LockFile assetsFile, Dictionary<string, string> projectReferenceNameByPath)
    {
        var restoreMetadataFrameworkInfo = assetsFile.PackageSpec.RestoreMetadata.TargetFrameworks.FirstOrDefault(tfi => tfi.TargetAlias.Equals(targetFrameworkMoniker, StringComparison.OrdinalIgnoreCase));
        if (restoreMetadataFrameworkInfo is null)
        {
            yield break;
        }

        foreach (var projectReference in restoreMetadataFrameworkInfo.ProjectReferences)
        {
            string projectPath = projectReference.ProjectPath;
            if (projectReferenceNameByPath.TryGetValue(projectPath, out string? projectName))
            {
                yield return projectName;
            }
        }
    }

    private static IEnumerable<LockFileTargetLibrary> GetAllLibrariesForFramework(string targetFrameworkMoniker, string? runtimeIdentifier, LockFile assetsFile)
    {
        var target = assetsFile.GetTarget(targetFrameworkMoniker, runtimeIdentifier);

        if (string.IsNullOrEmpty(runtimeIdentifier))
        {
            // If the runtime identifier was not specified, then we already have
            // all the libraries for the requested target framework.
            return target?.Libraries ?? [];
        }

        // Otherwise, we need to concatenate the libraries from the specific runtime identifier
        // with the libraries for a null runtime identifier to account for projects that don't
        // have a runtime identifier.
        var targetNoRid = assetsFile.GetTarget(targetFrameworkMoniker, runtimeIdentifier: null);
        return [
            .. target?.Libraries ?? [],
            .. targetNoRid?.Libraries ?? [],
        ];
    }

    private bool IsIncompatibleReference(string referenceIdentity)
    {
        // Check for incompatible assemblies based on the active C# bindings.
        if (UsesGodotDotNet)
        {
            // When using Godot .NET, check for GodotSharp assemblies.
            return referenceIdentity.Equals("GodotSharp", StringComparison.OrdinalIgnoreCase)
                || referenceIdentity.Equals("GodotSharpEditor", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // When using GodotSharp, check for Godot.Bindings assemblies.
            return referenceIdentity.Equals("Godot.Bindings", StringComparison.OrdinalIgnoreCase);
        }
    }

    private bool HasIncompatibleDependencies(string topLevelPackage, IEnumerable<LockFileTargetLibrary> packageLibraries, HashSet<string> visited)
    {
        Stack<string> stack = [];
        stack.Push(topLevelPackage);

        while (stack.Count > 0)
        {
            string currentPackageId = stack.Pop();

            if (IsIncompatibleReference(currentPackageId))
            {
                return true;
            }

            if (!visited.Add(currentPackageId))
            {
                continue;
            }

            var library = FindLibrary(currentPackageId, packageLibraries);
            if (library is not null)
            {
                if (library.Dependencies?.Count > 0)
                {
                    foreach (var dependency in library.Dependencies)
                    {
                        stack.Push(dependency.Id);
                    }
                }
            }
        }

        return false;

        static LockFileTargetLibrary? FindLibrary(string packageId, IEnumerable<LockFileTargetLibrary> packageLibraries)
        {
            foreach (var library in packageLibraries)
            {
                if (library.Name?.Equals(packageId, StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return library;
                }
            }

            return null;
        }
    }
}

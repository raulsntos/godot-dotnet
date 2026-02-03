using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Definition;
using Microsoft.CodeAnalysis;
using NuGet.Frameworks;
using NuGet.ProjectModel;
using Serilog;

namespace Godot.UpgradeAssistant.Providers;

using EvaluatedProject = Microsoft.Build.Evaluation.Project;

internal sealed class DotNetProjectReferencesAnalysisProvider : IAnalysisProvider
{
    private static DiagnosticDescriptor TfmRule =>
        Descriptors.GUA0003_DotNetProjectReferencesTfm;

    private static DiagnosticDescriptor OldBindingsRule =>
        Descriptors.GUA0004_DotNetProjectReferencesOldBindings;

    public async Task AnalyzeAsync(AnalysisContext context, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Analyzer only applies to .NET workspaces and there isn't one.
            return;
        }

        // Find the path to the 'project.assets.json' file that contains the resolved references after a restore.
        // The upgrade assistant always runs a restore step before running analyzers, so the file should exist.
        var projectRoot = context.Workspace.DotNetWorkspace.OpenProjectRootElement();
        var evaluatedProject = EvaluatedProject.FromProjectRootElement(projectRoot, new ProjectOptions());
        string projectAssetsFilePath = evaluatedProject.GetPropertyValue("ProjectAssetsFile");

        LockFile assetsFile;
        try
        {
            var lockFileFormat = new LockFileFormat();
            assetsFile = lockFileFormat.Read(projectAssetsFilePath);
        }
        catch (Exception e)
        {
            // We couldn't read the assets file, so we can't analyze it.
            // We'll just assume every reference is fine.
            // Godot .NET projects have a task that checks for incompatible packages
            // during the build, so the user will be informed if there are issues.
            Log.Debug(e, $"Failed to read 'project.assets.json' at '{projectAssetsFilePath}'.");
            return;
        }

        var projectReferenceNamesByPath = GetProjectReferenceNamesByPath(assetsFile);

        // Get the required TFM for the Godot version we're upgrading to.
        // Although different platforms may have different required TFMs,
        // we only check the default one for simplicity.
        var requiredTfm = await TargetFrameworkUtils.GetRequiredTargetFrameworkAsync(context.TargetGodotVersion, null, context.IsGodotDotNetEnabled, cancellationToken).ConfigureAwait(false);
        if (requiredTfm is null)
        {
            // We could not find a required TFM, we may not need to upgrade
            // so assume referenced projects will also still work.
            return;
        }

        foreach (var target in assetsFile.Targets)
        {
            string targetFramework = target.TargetFramework.GetShortFolderName();
            string? runtimeIdentifier = target.RuntimeIdentifier;

            List<string> topLevelReferences =
            [
                .. GetTopLevelPackageReferences(targetFramework, assetsFile),
                .. GetTopLevelProjectReferences(targetFramework, assetsFile, projectReferenceNamesByPath),
            ];

            var packageLibraries = target.Libraries;

            foreach (string topLevelReference in topLevelReferences)
            {
                if (ShouldIgnorePackage(context, topLevelReference))
                {
                    continue;
                }

                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (HasIncompatibleDependencies(context, topLevelReference, packageLibraries, visited))
                {
                    context.ReportAnalysis(AnalysisResult.Create(
                        id: OldBindingsRule.Id,
                        title: OldBindingsRule.Title,
                        location: default,
                        description: OldBindingsRule.Description,
                        helpUri: null,
                        messageFormat: OldBindingsRule.MessageFormat,
                        // Message Format parameters.
                        topLevelReference));
                }

                if (HasIncompatibleTfm(context, target, topLevelReference, packageLibraries, requiredTfm))
                {
                    context.ReportAnalysis(AnalysisResult.Create(
                        id: TfmRule.Id,
                        title: TfmRule.Title,
                        location: default,
                        description: TfmRule.Description,
                        helpUri: null,
                        messageFormat: TfmRule.MessageFormat,
                        // Message Format parameters.
                        topLevelReference,
                        context.TargetGodotVersion,
                        requiredTfm));
                }
            }
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

    private static bool ShouldIgnorePackage(AnalysisContext context, string referenceIdentity)
    {
        if (context.IsGodotDotNetEnabled)
        {
            // If we're upgrading the project to Godot .NET, these top-level packages will be replaced
            // so we don't need to report them.
            return referenceIdentity.Equals("GodotSharp", StringComparison.OrdinalIgnoreCase)
                || referenceIdentity.Equals("GodotSharpEditor", StringComparison.OrdinalIgnoreCase)
                || referenceIdentity.Equals("Godot.SourceGenerators", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // If we're upgrading between GodotSharp versions, the only relevant case is upgrading from 3 to 4.
            // But the GodotSharp 3 packages were never published to NuGet, so we won't find them as top-level packages.
            return false;
        }
    }

    private static bool IsIncompatibleReference(AnalysisContext context, string referenceIdentity)
    {
        // Check for incompatible assemblies based on the active C# bindings.
        if (context.IsGodotDotNetEnabled)
        {
            // When using Godot .NET, check for GodotSharp assemblies.
            return referenceIdentity.Equals("GodotSharp", StringComparison.OrdinalIgnoreCase)
                || referenceIdentity.Equals("GodotSharpEditor", StringComparison.OrdinalIgnoreCase)
                || referenceIdentity.Equals("Godot.SourceGenerators", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // When using GodotSharp, check for Godot.Bindings assemblies.
            return referenceIdentity.Equals("Godot.Bindings", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static bool HasIncompatibleDependencies(AnalysisContext context, string topLevelPackage, IEnumerable<LockFileTargetLibrary> packageLibraries, HashSet<string> visited)
    {
        Stack<string> stack = [];
        stack.Push(topLevelPackage);

        while (stack.Count > 0)
        {
            string currentPackageId = stack.Pop();

            if (IsIncompatibleReference(context, currentPackageId))
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
    }

    private static bool HasIncompatibleTfm(AnalysisContext context, LockFileTarget target, string topLevelPackage, IEnumerable<LockFileTargetLibrary> packageLibraries, NuGetFramework requiredTfm)
    {
        var library = FindLibrary(topLevelPackage, packageLibraries);
        if (library is null)
        {
            // We couldn't find the library, assume it's compatible.
            return false;
        }

        // Project references specify the TFM in the 'Framework' property, so we should use that if available.
        // Otherwise, this is a package reference and we use assume it supports the same TFM as the target.
        // Technically, package references can multi-target, so it may be compatible but we won't know
        // because this information won't be available in the 'project.assets.json' file.
        // We could look into the package stored in the package cache using NuGet APIs, but it's probably
        // not worth it since that only matters for projects upgrading from Godot 3.
        // From Godot 4 every project must target at least .NET 6 which is always forward compatible.
        NuGetFramework? libraryTfm = target.TargetFramework;
        if (library.Framework is not null)
        {
            libraryTfm = NuGetFramework.Parse(library.Framework);
        }

        // If the library has a list of supported frameworks, check compatibility.
        if (libraryTfm is not null)
        {
            if (!DefaultCompatibilityProvider.Instance.IsCompatible(libraryTfm, requiredTfm))
            {
                // Library is not compatible with the required TFM.
                return true;
            }
        }

        return false;
    }

    private static LockFileTargetLibrary? FindLibrary(string packageId, IEnumerable<LockFileTargetLibrary> packageLibraries)
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

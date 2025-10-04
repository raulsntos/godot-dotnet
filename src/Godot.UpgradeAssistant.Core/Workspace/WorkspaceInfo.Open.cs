using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Godot.UpgradeAssistant;

partial class WorkspaceInfo
{
    /// <summary>
    /// An event raised whenever a .NET workspace logs errors or other diagnostics,
    /// like an error when trying to open a workspace.
    /// </summary>
    public static event EventHandler<WorkspaceDiagnosticEventArgs>? DotNetWorkspaceDiagnostic;

    /// <summary>
    /// Create a <see cref="WorkspaceInfo"/> from the given path to the
    /// <c>project.godot</c> file of the Godot project to open.
    /// </summary>
    /// <param name="godotProjectFilePath">Path to the <c>project.godot</c> file.</param>
    /// <param name="dotnetSolutionFilePath">Path to the .NET solution file.</param>
    /// <param name="dotnetProjectFilePath">Path to the .NET project file.</param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the step.
    /// </param>
    /// <returns>Task that completes when the workspace is ready.</returns>
    /// <exception cef="FileNotFoundException">
    /// File specified by <paramref name="godotProjectFilePath"/>, <paramref name="dotnetSolutionFilePath"/>,
    /// or <paramref name="dotnetProjectFilePath"/> does not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The .NET project uses an unsupported MSBuild SDK.
    /// </exception>
    public static async Task<WorkspaceInfo> OpenAsync(string godotProjectFilePath, string dotnetSolutionFilePath, string dotnetProjectFilePath, CancellationToken cancellationToken = default)
    {
        var project = new GodotProject()
        {
            ProjectFilePath = godotProjectFilePath,
            GodotVersion = DetermineGodotVersionFromProjectFile(godotProjectFilePath, dotnetProjectFilePath),
        };

        var dotnetWorkspace = await OpenDotNetWorkspaceAsync(godotProjectFilePath, dotnetSolutionFilePath, dotnetProjectFilePath, cancellationToken).ConfigureAwait(false);
        if (dotnetWorkspace is not null)
        {
            return new WorkspaceInfo(project, dotnetWorkspace);
        }

        return new WorkspaceInfo(project);
    }

    private static SemVer DetermineGodotVersionFromProjectFile(string godotProjectFilePath, string dotnetProjectFilePath)
    {
        // GodotSharp 4.0+ projects always use a version of Godot's MSBuild SDK
        // that matches the version of the Godot editor that they were last edited with.
        var projectRoot = ProjectRootElement.Open(dotnetProjectFilePath);
        string sdk = projectRoot.Sdk;
        if (!sdk.StartsWith(Constants.GodotSdkAssemblyName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_MSBuildSdkNotSupported(sdk));
        }

        JsonDocument? globalJson = null;
        DetermineGlobalJsonLocation(godotProjectFilePath, out string? globalJsonPath);
        if (File.Exists(globalJsonPath))
        {
            using var globalJsonStream = File.OpenRead(globalJsonPath);
            globalJson = GlobalJson.ParseAsDocument(globalJsonStream);
        }

        // Parse the version from the MSBuild SDK: 'Godot.NET.Sdk/{Version}'.
        // Or from the 'global.json' file.
        if (!DotNetWorkspaceInfo.TryGetGodotSdkVersion(projectRoot, globalJson, out var sdkVersion))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_MSBuildSdkNotSupported(sdk));
        }

        return sdkVersion;
    }

    private static async ValueTask<MSBuildDotNetWorkspaceInfo?> OpenDotNetWorkspaceAsync(string godotProjectFilePath, string dotnetSolutionFilePath, string dotnetProjectFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(dotnetSolutionFilePath) && !File.Exists(dotnetProjectFilePath))
        {
            // A .NET workspace does not seem available.
            return null;
        }

        var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += LogWorkspaceDiagnostics;

        var solution = await workspace.OpenSolutionAsync(dotnetSolutionFilePath, cancellationToken: cancellationToken).ConfigureAwait(false);

        // Try to find the C# project in the solution. Attempting to open it again will throw an exception.
        // Since we only have the path we have to check all the projects in the solution and compare their paths.
        // There may be more than one ProjectInfo in the solution for the same project if it's multi-targeting.
        var project = solution.Projects.FirstOrDefault(project => project.FilePath == dotnetProjectFilePath);
        // If we couldn't find the C# project in the solution, let's open it separately.
        project ??= await workspace.OpenProjectAsync(dotnetProjectFilePath, cancellationToken: cancellationToken).ConfigureAwait(false);

        var projectRoot = ProjectRootElement.Open(dotnetProjectFilePath, ProjectCollection.GlobalProjectCollection, preserveFormatting: true);

        DetermineGlobalJsonLocation(godotProjectFilePath, out string? globalJsonPath);

        return new MSBuildDotNetWorkspaceInfo(workspace, project, projectRoot, globalJsonPath);

        static void LogWorkspaceDiagnostics(object? sender, WorkspaceDiagnosticEventArgs e)
        {
            DotNetWorkspaceDiagnostic?.Invoke(sender, e);
        }
    }

    private static bool DetermineGlobalJsonLocation(string godotProjectFilePath, [NotNullWhen(true)] out string? globalJsonPath)
    {
        // The .NET SDK looks for a 'global.json' file in the current working directory
        // (which isn't necessarily the same as the project directory) or one of its
        // parents directories.
        // We'll assume the current working directory is the Godot project root directory
        // because that's where the dotnet commands will be executed from when building
        // from Godot.

        string godotProjectPath = Path.GetDirectoryName(godotProjectFilePath)!;
        return TryGetGlobalJsonPath(godotProjectPath, out globalJsonPath);

        static bool TryGetGlobalJsonPath(string startPath, [NotNullWhen(true)] out string? globalJsonPath)
        {
            string? cwd = startPath;

            do
            {
                globalJsonPath = Path.Join(cwd, "global.json");
                if (File.Exists(globalJsonPath))
                {
                    return true;
                }

                cwd = Path.GetDirectoryName(cwd);

                if (string.IsNullOrWhiteSpace(cwd) || !Directory.Exists(cwd))
                {
                    globalJsonPath = null;
                    return false;
                }
            } while (true);
        }
    }
}

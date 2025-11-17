using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

namespace Godot.EditorIntegration.Internals;

/// <summary>
/// Contains cached directory/file names and paths used for the .NET projects.
/// </summary>
internal static class EditorPath
{
    private static string? _projectAssemblyName;
    private static string? _slnPath;
    private static string? _csprojPath;
    private static string? _editorAssembliesPath;
    private static string? _baseBuildLogsPath;

    public static string ProjectAssemblyName => _projectAssemblyName ??= EditorInternal.GetProjectAssemblyName();

    public static string ProjectSlnPath => _slnPath ??= GetProjectSlnOrSlnxPath();

    public static string ProjectCSProjPath => _csprojPath ??= EditorInternal.GetProjectCSProjPath();

    public static string EditorAssembliesPath => _editorAssembliesPath ??= EditorInternal.GetEditorAssembliesPath();

    public static string GetLogsDirPathFor(string project, string configuration)
    {
        _baseBuildLogsPath ??= ProjectSettings.Singleton.GlobalizePath("user://msbuild_logs/");
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(project)));
        return Path.Join(_baseBuildLogsPath, hash, configuration);
    }

    public static string GetLogsDirPathFor(string configuration)
    {
        return GetLogsDirPathFor(ProjectCSProjPath, configuration);
    }

    public static void InvalidateCachedDirectories()
    {
        // Clear all the cached values that may change based on the project settings.
        _projectAssemblyName = null;
        _slnPath = null;
        _csprojPath = null;
    }

    private static string GetProjectSlnOrSlnxPath()
    {
        string slnPath = EditorInternal.GetProjectSlnPath();
        string slnDir = Path.GetDirectoryName(slnPath)!;

        List<string> slnPaths =
        [
            .. Directory.GetFiles(slnDir, "*.sln"),
            .. Directory.GetFiles(slnDir, "*.slnx"),
        ];

        for (int i = slnPaths.Count - 1; i > 0; --i)
        {
            ISolutionSerializer? serializer = SolutionSerializers.GetSerializerByMoniker(slnPaths[i]);

            if (serializer is null)
            {
                goto SolutionIsInvalid;
            }

            SolutionModel solution = serializer.OpenAsync(slnPaths[i], CancellationToken.None).Result;

            foreach (SolutionProjectModel project in solution.SolutionProjects)
            {
                string csProjPath = Path.GetFullPath(project.FilePath, Path.GetDirectoryName(slnPaths[i])!)
                                        .Replace('\\', '/');

                if (string.Equals(csProjPath, ProjectCSProjPath, StringComparison.Ordinal))
                {
                    goto SolutionIsValid;
                }
            }

        SolutionIsInvalid:
            slnPaths.RemoveAt(i);
        SolutionIsValid:;
        }

        // TODO: Better error handling.
        switch (slnPaths.Count)
        {
            case 0:
                return slnPath;
            case 1:
                return slnPaths[0];
            default:
                GD.PushError($"""
                    Multiple solutions containing a project with assembly name '{ProjectAssemblyName}' were found:
                    {string.Join('\n', slnPaths).Replace('\\', '/')}
                    Please ensure only one solution contains the project assembly.
                    If you have recently migrated to .slnx please ensure that you have removed the unused .sln.
                    """);
                return string.Empty;
        }
    }
}

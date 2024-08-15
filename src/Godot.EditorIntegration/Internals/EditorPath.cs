using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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

    public static string ProjectSlnPath => _slnPath ??= EditorInternal.GetProjectSlnPath();

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
}

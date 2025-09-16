using System.IO;
using System.Reflection;

namespace Godot.SourceGenerators.Tests;

internal static class Constants
{
    public static Assembly GodotBindingsAssembly => typeof(GodotObject).Assembly;

    public static string ExecutingAssemblyPath { get; }
    public static string SourceFolderPath { get; }
    public static string GeneratedSourceFolderPath { get; }

    static Constants()
    {
        ExecutingAssemblyPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!);

        string testDataPath = Path.Combine(ExecutingAssemblyPath, "TestData");

        SourceFolderPath = Path.Combine(testDataPath, "Sources");
        GeneratedSourceFolderPath = Path.Combine(testDataPath, "GeneratedSources");
    }
}

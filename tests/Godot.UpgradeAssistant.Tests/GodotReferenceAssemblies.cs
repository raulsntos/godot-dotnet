using System.IO;
using Microsoft.CodeAnalysis;
using NuGet.Configuration;

namespace Godot.UpgradeAssistant.Tests;

internal static class GodotReferenceAssemblies
{
    public static MetadataReference[] GodotSharp3 { get; }

    public static MetadataReference[] GodotSharp4 { get; }

    public static MetadataReference[] GodotDotNet { get; }

    private static readonly string _nugetCachePath = SettingsUtility.GetGlobalPackagesFolder(Settings.LoadDefaultSettings(null));

    static GodotReferenceAssemblies()
    {
        GodotSharp3 = [
            MetadataReference.CreateFromFile(GetArchivedAssemblyPath("GodotSharp", "3.6.0")),
            MetadataReference.CreateFromFile(GetArchivedAssemblyPath("GodotSharpEditor", "3.6.0")),
        ];

        GodotSharp4 = [
            MetadataReference.CreateFromFile(GetNuGetPackageAssemblyPath("GodotSharp", "4.5.0", "net8.0")),
            MetadataReference.CreateFromFile(GetNuGetPackageAssemblyPath("GodotSharpEditor", "4.5.0", "net8.0")),
        ];

        GodotDotNet = [
            MetadataReference.CreateFromFile(typeof(GodotObject).Assembly.Location),
        ];
    }

    private static string GetNuGetPackageAssemblyPath(string packageName, string packageVersion, string tfm)
    {
        return Path.Combine(_nugetCachePath, packageName.ToLowerInvariant(), packageVersion, "lib", tfm, $"{packageName}.dll");
    }

    private static string GetArchivedAssemblyPath(string assemblyName, string version)
    {
        return Path.Combine(Constants.ExecutingAssemblyPath, "ArchivedAssemblies", version, $"{assemblyName}.dll");
    }
}

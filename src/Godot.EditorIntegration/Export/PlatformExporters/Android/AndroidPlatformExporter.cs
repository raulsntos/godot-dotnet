using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot.Collections;
using Godot.EditorIntegration.Build;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Export;

internal sealed class AndroidPlatformExporter : PlatformExporter
{
    public override bool SupportsPlatform(string godotPlatform)
    {
        return GodotPlatform.IsAndroid(godotPlatform);
    }

    private static HashSet<RuntimeIdentifierArchitecture> DetermineArchitectures(HashSet<string> features)
    {
        HashSet<RuntimeIdentifierArchitecture> architectures = [];

        TryAddArchitecture(RuntimeIdentifierArchitecture.X86);
        TryAddArchitecture(RuntimeIdentifierArchitecture.X64);
        TryAddArchitecture(RuntimeIdentifierArchitecture.Arm);
        TryAddArchitecture(RuntimeIdentifierArchitecture.Arm64);

        return architectures;

        void TryAddArchitecture(RuntimeIdentifierArchitecture architecture)
        {
            if (features.Contains(RuntimeIdentifierArchitecture.ToGodot(architecture)))
            {
                architectures.Add(architecture);
            }
        }
    }

    public override void DetermineBuildOptions(PlatformExporterContext context)
    {
        var architectures = DetermineArchitectures(context.PresetFeatures);
        foreach (RuntimeIdentifierArchitecture architecture in architectures)
        {
            var publishOptions = new PublishOptions()
            {
                SlnOrProject = EditorPath.ProjectCSProjPath,
                LogsPath = EditorPath.GetLogsDirPathFor(context.BuildConfiguration),
                OutputPath = context.CreateTemporaryDirectory(),
                Configuration = context.BuildConfiguration,
                RuntimeIdentifier = $"{RuntimeIdentifierOS.Android}-{architecture}",
                SelfContained = true,
            }
                .WithGodotCommonDefaults()
                .WithGodotTargetPlatformProperty(context.TargetPlatform)
                .WithDebugSymbolsProperties(context.IncludeDebugSymbols);

            context.AddBuild(new ExportBuildOptions()
            {
                PublishOptions = publishOptions,
                AssemblyName = EditorPath.ProjectAssemblyName,
                GodotPlatform = context.TargetPlatform,
                GodotArchitecture = RuntimeIdentifierArchitecture.ToGodot(architecture),
            });
        }
    }

    public override void ExportAfterBuild(PlatformExporterContext context, ExportBuildOptions options)
    {
        Debug.Assert(!string.IsNullOrEmpty(options.PublishOptions.OutputPath));
        string outputPath = options.PublishOptions.OutputPath;

        string assemblyPath = Path.Join(outputPath, $"{EditorPath.ProjectAssemblyName}.dll");

        if (!File.Exists(assemblyPath))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_PublishSuccessfulButAssemblyNotFound(assemblyPath));
        }

        string projectDataDirName = options.GetExportedDataDirectoryName();

        PackedStringArray tags = [options.GodotArchitecture];

        foreach (string path in Directory.EnumerateFiles(outputPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(outputPath, path);
            if (IsSharedObject(path))
            {
                string target = Path.Join(projectDataDirName, Path.GetDirectoryName(relativePath));
                context.ExportPlugin.AddSharedObject(path, tags, target);
            }
            else
            {
                byte[] bytes = File.ReadAllBytes(path);

                PackedByteArray fileData = [.. bytes];

                context.ExportPlugin.AddFile($"res://.godot/dotnet/{options.GodotArchitecture}/{relativePath}", fileData, remap: false);
            }
        }

        static bool IsSharedObject(string path)
        {
            if (path.EndsWith(".so", StringComparison.OrdinalIgnoreCase)
             || path.EndsWith(".a", StringComparison.OrdinalIgnoreCase)
             || path.EndsWith(".jar", StringComparison.OrdinalIgnoreCase)
             || path.EndsWith(".dex", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}

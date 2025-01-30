using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Godot.Collections;
using Godot.EditorIntegration.Build;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Export;

internal sealed class DesktopPlatformExporter : PlatformExporter
{
    public override bool SupportsPlatform(string godotPlatform)
    {
        return GodotPlatform.IsWindows(godotPlatform)
            || GodotPlatform.IsMacOS(godotPlatform)
            || GodotPlatform.IsLinux(godotPlatform);
    }

    private static RuntimeIdentifierOS DetermineRuntimeIdentifierOS(string platform)
    {
        return platform switch
        {
            _ when GodotPlatform.IsWindows(platform) => RuntimeIdentifierOS.Win,
            _ when GodotPlatform.IsMacOS(platform) => RuntimeIdentifierOS.OSX,
            _ when GodotPlatform.IsLinux(platform) => RuntimeIdentifierOS.Linux,
            _ => throw new NotSupportedException(SR.FormatNotSupported_TargetPlatform(platform)),
        };
    }

    private static HashSet<RuntimeIdentifierArchitecture> DetermineArchitectures(string platform, ReadOnlySet<string> features)
    {
        HashSet<RuntimeIdentifierArchitecture> architectures = [];

        TryAddArchitecture(RuntimeIdentifierArchitecture.X86);
        TryAddArchitecture(RuntimeIdentifierArchitecture.X64);
        TryAddArchitecture(RuntimeIdentifierArchitecture.Arm);
        TryAddArchitecture(RuntimeIdentifierArchitecture.Arm64);

        if (GodotPlatform.IsLinux(platform))
        {
            TryAddArchitecture(RuntimeIdentifierArchitecture.RiscV64);
            TryAddArchitecture(RuntimeIdentifierArchitecture.Ppc64le);
        }

        if (GodotPlatform.IsMacOS(platform))
        {
            if (features.Contains("universal"))
            {
                architectures.Add(RuntimeIdentifierArchitecture.X64);
                architectures.Add(RuntimeIdentifierArchitecture.Arm64);
            }
        }

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
        RuntimeIdentifierOS ridOS = DetermineRuntimeIdentifierOS(context.TargetPlatform);
        var architectures = DetermineArchitectures(context.TargetPlatform, context.PresetFeatures);
        foreach (RuntimeIdentifierArchitecture architecture in architectures)
        {
            var publishOptions = new PublishOptions()
            {
                SlnOrProject = EditorPath.ProjectCSProjPath,
                LogsPath = EditorPath.GetLogsDirPathFor(context.BuildConfiguration),
                OutputPath = context.CreateTemporaryDirectory(),
                Configuration = context.BuildConfiguration,
                RuntimeIdentifier = $"{ridOS}-{architecture}",
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

        string nativeAotExtension = context.TargetPlatform switch
        {
            string platform when GodotPlatform.IsWindows(platform) => "dll",
            string platform when GodotPlatform.IsMacOS(platform) => "dylib",
            _ => "so",
        };

        string assemblyPath = Path.Join(outputPath, $"{EditorPath.ProjectAssemblyName}.dll");
        string nativeAotPath = Path.Join(outputPath, $"{EditorPath.ProjectAssemblyName}.{nativeAotExtension}");

        if (!File.Exists(assemblyPath) && !File.Exists(nativeAotPath))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_PublishSuccessfulButAssemblyNotFound2(assemblyPath, nativeAotPath));
        }

        string projectDataDirName = options.GetExportedDataDirectoryName();

        if (GodotPlatform.IsMacOS(context.TargetPlatform))
        {
            projectDataDirName = Path.Join("Contents", "Resources", projectDataDirName);
        }

        PackedStringArray tags = [options.GodotArchitecture];

        foreach (string path in Directory.EnumerateFiles(outputPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(outputPath, path);
            string target = Path.Join(projectDataDirName, Path.GetDirectoryName(relativePath));
            context.ExportPlugin.AddSharedObject(path, tags, target);
        }
    }
}

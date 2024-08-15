using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot.Collections;
using Godot.EditorIntegration.Build;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Export;

internal sealed class IOSPlatformExporter : PlatformExporter
{
    public override bool SupportsPlatform(string godotPlatform)
    {
        return GodotPlatform.IsIOS(godotPlatform);
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
            context.AddBuild(CreateOptions(context, RuntimeIdentifierOS.IOS, architecture));
        }

        foreach (RuntimeIdentifierArchitecture architecture in (ReadOnlySpan<RuntimeIdentifierArchitecture>)
        [
            RuntimeIdentifierArchitecture.Arm64,
            RuntimeIdentifierArchitecture.X64,
        ])
        {
            context.AddBuild(CreateOptions(context, RuntimeIdentifierOS.IOSSimulator, architecture));
        }

        static ExportBuildOptions CreateOptions(PlatformExporterContext context, RuntimeIdentifierOS os, RuntimeIdentifierArchitecture architecture)
        {
            string runtimeIdentifier = $"{os}-{architecture}";

            string baseOutputPath = EditorInternal.GetProjectOutputPath(EditorPath.ProjectCSProjPath);
            var publishOptions = new PublishOptions()
            {
                SlnOrProject = EditorPath.ProjectCSProjPath,
                LogsPath = EditorPath.GetLogsDirPathFor(context.BuildConfiguration),
                // Xcode project links directly to files in the publish dir, so don't use a temporary directory.
                OutputPath = Path.Join(baseOutputPath, "godot-publish", runtimeIdentifier),
                Configuration = context.BuildConfiguration,
                RuntimeIdentifier = runtimeIdentifier,
                SelfContained = true,
            }
                .WithGodotCommonDefaults()
                .WithGodotTargetPlatformProperty(context.TargetPlatform)
                .WithDebugSymbolsProperties(context.IncludeDebugSymbols);

            return new ExportBuildOptions()
            {
                PublishOptions = publishOptions,
                AssemblyName = EditorPath.ProjectAssemblyName,
                GodotPlatform = context.TargetPlatform,
                GodotArchitecture = RuntimeIdentifierArchitecture.ToGodot(architecture),
            };
        }
    }

    public override void ExportAfterBuild(PlatformExporterContext context, ExportBuildOptions options)
    {
        Debug.Assert(!string.IsNullOrEmpty(options.PublishOptions.OutputPath));
        Debug.Assert(!string.IsNullOrEmpty(options.PublishOptions.RuntimeIdentifier));
        string outputPath = options.PublishOptions.OutputPath;
        string runtimeIdentifier = options.PublishOptions.RuntimeIdentifier;

        string nativeAotPath = Path.Join(outputPath, $"{EditorPath.ProjectAssemblyName}.dylib");

        if (!File.Exists(nativeAotPath))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_PublishSuccessfulButAssemblyNotFound(nativeAotPath));
        }

        // For iOS simulator builds, skip packaging the build outputs.
        if (runtimeIdentifier.StartsWith(RuntimeIdentifierOS.IOSSimulator, StringComparison.Ordinal))
        {
            return;
        }

        string projectDataDirName = options.GetExportedDataDirectoryName();

        PackedStringArray tags = [options.GodotArchitecture];

        AddExportFiles(outputPath);

        void AddExportFiles(string path)
        {
            foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetFileName(file) == $"{EditorPath.ProjectAssemblyName}.dylib")
                {
                    // Exclude the dylib artifact, since it's included separately as an xcframework.
                    continue;
                }

                if (path.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
                {
                    context.ExportPlugin.AddIOSBundleFile(path);
                }
                else
                {
                    string relativePath = Path.GetRelativePath(outputPath, path);
                    string target = Path.Join(projectDataDirName, Path.GetDirectoryName(relativePath));
                    context.ExportPlugin.AddSharedObject(path, tags, target);
                }
            }

            foreach (string dir in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                if (dir.EndsWith(".dsym", StringComparison.OrdinalIgnoreCase))
                {
                    // Exclude dsym folders.
                    continue;
                }

                AddExportFiles(dir);
            }
        }
    }

    public override void ExportAfterAllBuilds(PlatformExporterContext context)
    {
        List<string> outputPaths = context.BuildOptions
            .Select(options => options.PublishOptions.OutputPath!)
            .ToList();

        if (outputPaths.Count > 2)
        {
            // Lipo the simulator binaries together.

            string outputPath = Path.Join(outputPaths[1], $"{EditorPath.ProjectAssemblyName}.dylib");
            var files = outputPaths
                .Skip(1)
                .Select(path => Path.Join(path, $"{EditorPath.ProjectAssemblyName}.dylib"));

            if (!EditorInternal.LipOCreateFile(outputPath, new PackedStringArray(files)))
            {
                throw new InvalidOperationException(SR.InvalidOperation_CreateFatBinaryFailed);
            }

            outputPaths.RemoveRange(2, outputPaths.Count - 2);
        }

        string baseOutputPath = EditorInternal.GetProjectOutputPath(EditorPath.ProjectSlnPath);
        string xcFrameworkPath = Path.Join(baseOutputPath, $"{EditorPath.ProjectAssemblyName}_aot.xcframework");
        if (!XCFramework.GenerateBlocking(outputPaths, xcFrameworkPath))
        {
            throw new InvalidOperationException(SR.InvalidOperation_GenerateXCFrameworkFailed);
        }

        context.ExportPlugin.AddIOSEmbeddedFramework(xcFrameworkPath);
    }
}

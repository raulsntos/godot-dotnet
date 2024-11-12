using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Godot.Collections;
using Godot.EditorIntegration.Build;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Export;

[GodotClass]
internal sealed partial class DotNetExportPlugin : EditorExportPlugin
{
    /// <summary>
    /// The current exporter for an ongoing export, or <see langword="null"/>
    /// if there isn't an active export process.
    /// </summary>
    private PlatformExporter? _exporter;

    /// <summary>
    /// The current exporter context for an ongoing export, or <see langword="null"/>
    /// if there isn't an active export process.
    /// </summary>
    private PlatformExporterContext? _exporterContext;

    private readonly List<PlatformExporter> _exporters =
    [
        new DesktopPlatformExporter(),
        new AndroidPlatformExporter(),
        new IOSPlatformExporter(),
        // TODO: Browser platform is not implemented yet, uncomment this when it is.
        // new BrowserPlatformExporter(),
    ];

    protected override string _GetName() => ".NET";

    private static bool ProjectContainsDotNet()
    {
        return File.Exists(EditorPath.ProjectCSProjPath);
    }

    protected override PackedStringArray _GetExportFeatures(EditorExportPlatform platform, bool debug)
    {
        if (!ProjectContainsDotNet())
        {
            return [];
        }

        return ["dotnet"];
    }

    protected override GodotArray<GodotDictionary> _GetExportOptions(EditorExportPlatform platform)
    {
        return
        [
            new GodotDictionary()
            {
                ["option"] = new GodotDictionary()
                {
                    ["name"] = ExportOptionNames.IncludeDebugSymbols,
                    ["type"] = (int)VariantType.Bool,
                },
                ["default_value"] = true,
            },
        ];
    }

    private bool TryGetPlatformExporter(string platform, [NotNullWhen(true)] out PlatformExporter? platformExporter)
    {
        foreach (var exporter in _exporters)
        {
            if (exporter.SupportsPlatform(platform))
            {
                platformExporter = exporter;
                return true;
            }
        }

        platformExporter = null;
        return false;
    }

    protected override void _ExportBegin(PackedStringArray features, bool isDebug, string path, uint flags)
    {
        try
        {
            ExportBeginCore([.. features], isDebug);
        }
        catch (Exception e)
        {
            AddExceptionMessage(GetExportPlatform(), e);
        }
    }

    private void ExportBeginCore(HashSet<string> features, bool isDebug)
    {
        if (!ProjectContainsDotNet())
        {
            return;
        }

        string platform = GetExportPlatform().GetOSName();

        if (!TryGetPlatformExporter(platform, out _exporter))
        {
            throw new NotSupportedException(SR.FormatNotSupported_TargetPlatform(platform));
        }

        _exporterContext = new PlatformExporterContext()
        {
            ExportPlugin = this,
            TargetPlatform = platform,
            PresetFeatures = new ReadOnlySet<string>(features),
            BuildConfiguration = isDebug ? "ExportDebug" : "ExportRelease",
            IncludeDebugSymbols = (bool)GetOption(ExportOptionNames.IncludeDebugSymbols),
        };

        _exporter.DetermineBuildOptions(_exporterContext);

        _exporter.ExportBeforeAllBuilds(_exporterContext);

        if (!EditorProgress.Invoke("dotnet_publish", SR.ExportProjectProgressLabel, _exporterContext.BuildOptions.Count, progress =>
        {
            for (int i = 0; i < _exporterContext.BuildOptions.Count; i++)
            {
                ExportBuildOptions exportOptions = _exporterContext.BuildOptions[i];
                PublishOptions options = exportOptions.PublishOptions;

                string assemblyName = Path.GetFileNameWithoutExtension(options.SlnOrProject);
                string runtimeIdentifier = !string.IsNullOrEmpty(options.RuntimeIdentifier)
                    ? options.RuntimeIdentifier
                    : RuntimeInformation.RuntimeIdentifier;
                progress.Step(SR.FormatBuildProjectProgressStep(assemblyName, runtimeIdentifier), i);

                if (!BuildManager.PublishProject(options))
                {
                    return false;
                }

                _exporter.ExportAfterBuild(_exporterContext, exportOptions);
            }

            return true;
        }))
        {
            throw new InvalidOperationException(SR.InvalidOperation_BuildFailed);
        }

        _exporter.ExportAfterAllBuilds(_exporterContext);
    }

    protected override void _ExportEnd()
    {
        try
        {
            ExportEndCore();
        }
        catch (Exception e)
        {
            AddExceptionMessage(GetExportPlatform(), e);
        }
        finally
        {
            _exporter = null;
            _exporterContext = null;
        }
    }

    private void ExportEndCore()
    {
        if (_exporter is null || _exporterContext is null)
        {
            // There was no export in progress or starting the export failed.
            return;
        }

        _exporter.Cleanup(_exporterContext);
    }

    internal static void AddExceptionMessage(EditorExportPlatform platform, Exception exception)
    {
        string? exceptionMessage = exception.Message;
        if (string.IsNullOrEmpty(exceptionMessage))
        {
            exceptionMessage = SR.FormatExportProjectMessageExceptionThrown(exception.GetType().Name);
        }

        platform.AddMessage(EditorExportPlatform.ExportMessageType.Error, SR.ExportProjectMessageCategory, exceptionMessage);

        // We also print exceptions as we receive them to stderr.
        Console.Error.WriteLine(exception);
    }
}

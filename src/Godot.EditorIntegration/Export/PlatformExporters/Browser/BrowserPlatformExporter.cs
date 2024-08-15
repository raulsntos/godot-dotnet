using System;
using Godot.EditorIntegration.Build;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Export;

internal sealed class BrowserPlatformExporter : PlatformExporter
{
    public override bool SupportsPlatform(string godotPlatform)
    {
        return GodotPlatform.IsWeb(godotPlatform);
    }

    public override void DetermineBuildOptions(PlatformExporterContext context)
    {
        var publishOptions = new PublishOptions()
        {
            SlnOrProject = EditorPath.ProjectCSProjPath,
            LogsPath = EditorPath.GetLogsDirPathFor(context.BuildConfiguration),
            OutputPath = context.CreateTemporaryDirectory(),
            Configuration = context.BuildConfiguration,
            RuntimeIdentifier = $"{RuntimeIdentifierOS.Browser}-{RuntimeIdentifierArchitecture.Wasm}",
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
            GodotArchitecture = RuntimeIdentifierArchitecture.ToGodot(RuntimeIdentifierArchitecture.Wasm),
        });
    }

    public override void ExportAfterBuild(PlatformExporterContext context, ExportBuildOptions options)
    {
        throw new NotImplementedException(SR.FormatNotSupported_TargetPlatform(context.TargetPlatform));
    }
}

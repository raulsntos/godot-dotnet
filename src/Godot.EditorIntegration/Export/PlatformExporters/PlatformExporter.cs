using System.IO;

namespace Godot.EditorIntegration.Export;

/// <summary>
/// Implements the export process of .NET projects for a specific platform.
/// </summary>
internal abstract class PlatformExporter
{
    /// <summary>
    /// Indicates if the exporter supports the given Godot platform.
    /// </summary>
    /// <param name="godotPlatform">Target Godot platform for this export process.</param>
    /// <returns>Whether the exporter supports the platform.</returns>
    public abstract bool SupportsPlatform(string godotPlatform);

    /// <summary>
    /// Determine the <see cref="ExportBuildOptions"/> that will be used to build the .NET project
    /// for exporting and add it to <paramref name="context"/>.
    /// </summary>
    /// <param name="context">
    /// <see cref="PlatformExporterContext"/> instance that contains data for an ongoing export
    /// and can be used to add the <see cref="ExportBuildOptions"/> that will be used to launch
    /// the necessary builds of the .NET project.
    /// </param>
    public abstract void DetermineBuildOptions(PlatformExporterContext context);

    /// <summary>
    /// Prepare export before all builds.
    /// </summary>
    /// <param name="context">
    /// <see cref="PlatformExporterContext"/> instance that contains data for an ongoing export.
    /// </param>
    public virtual void ExportBeforeAllBuilds(PlatformExporterContext context) { }

    /// <summary>
    /// Add files to the export after a build.
    /// </summary>
    /// <param name="context">
    /// <see cref="PlatformExporterContext"/> instance that contains data for an ongoing export.
    /// </param>
    /// <param name="options">
    /// The options that were used to launch the build that just finished.
    /// </param>
    public abstract void ExportAfterBuild(PlatformExporterContext context, ExportBuildOptions options);

    /// <summary>
    /// Add files to the export after all builds.
    /// </summary>
    /// <param name="context">
    /// <see cref="PlatformExporterContext"/> instance that contains data for an ongoing export.
    /// </param>
    public virtual void ExportAfterAllBuilds(PlatformExporterContext context) { }

    /// <summary>
    /// Cleanup after the export process has finished.
    /// </summary>
    /// <param name="context">
    /// <see cref="PlatformExporterContext"/> instance that contains data for the finished export.
    /// </param>
    public virtual void Cleanup(PlatformExporterContext context)
    {
        foreach (string folder in context.TemporaryDirectories)
        {
            Directory.Delete(folder, recursive: true);
        }

        context.TemporaryDirectories.Clear();
    }
}

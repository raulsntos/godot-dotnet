using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Step that restores packages in the target project, to prepare it for the upgrade.
/// </summary>
public sealed class RestorePackagesStep : AssistantStepBase<RestorePackagesStep.Configuration>
{
    /// <summary>
    /// Configuration for the restore step.
    /// </summary>
    public sealed class Configuration : AssistantStepConfiguration
    {
        /// <summary>
        /// The MSBuild logger to use with the restore operation.
        /// </summary>
        public ILogger? Logger { get; init; }
    }

    private BuildResult? _result;

    /// <summary>
    /// Constructs a <see cref="RestorePackagesStep"/>.
    /// </summary>
    /// <param name="configuration">Provides the configuration for the step.</param>
    public RestorePackagesStep(Configuration configuration) : base(configuration) { }

    /// <summary>
    /// Gets the build result of the restore step.
    /// </summary>
    /// <returns>Build result of the restore step.</returns>
    /// <exception cref="InvalidOperationException">
    /// The step has not been executed yet so there is no build result.
    /// </exception>
    public BuildResult GetResult()
    {
        if (_result is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_RestorePackagesStepHasntExecuted);
        }

        return _result;
    }

    /// <inheritdoc/>
    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (Config.Workspace.DotNetWorkspace is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_RestorePackagesStepCannotRunWithoutWorkspace);
        }

        var projectRoot = Config.Workspace.DotNetWorkspace.OpenProjectRootElement();

        var projectInstance = new ProjectInstance(projectRoot.FullPath);

        var buildParameters = new BuildParameters();
        if (Config.Logger is not null)
        {
            buildParameters.Loggers = [Config.Logger];
        }

        var restoreRequest = new BuildRequestData(projectInstance, ["Restore"]);

        _result = BuildManager.DefaultBuildManager.Build(buildParameters, restoreRequest);

        // Reload the project because, by design, NuGet properties (like NuGetPackageRoot)
        // aren't available in a project until after restore is run the first time.
        // https://github.com/NuGet/Home/issues/9150
        await Config.Workspace.ReloadAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the step and returns the build result.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the step.
    /// </param>
    /// <returns>Task that completes when the step finishes its execution.</returns>
    public async Task<BuildResult> RunAndGetResultAsync(CancellationToken cancellationToken = default)
    {
        await RunAsync(cancellationToken).ConfigureAwait(false);
        return GetResult();
    }
}

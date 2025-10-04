using System;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Base step for all the assistant's steps.
/// </summary>
public abstract class AssistantStepBase<TConfiguration> where TConfiguration : AssistantStepConfiguration
{
    /// <summary>
    /// Configuration for this step.
    /// </summary>
    protected TConfiguration Config { get; }

    /// <summary>
    /// Constructs a <see cref="AssistantStepBase{TConfiguration}"/>.
    /// </summary>
    /// <param name="configuration">Provides the configuration for the step.</param>
    public AssistantStepBase(TConfiguration configuration)
    {
        ValidateConfiguration(configuration);
        Config = configuration;
    }

    /// <summary>
    /// Execute the step.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the step.
    /// </param>
    /// <returns>Task that completes when the step finishes its execution.</returns>
    public abstract Task RunAsync(CancellationToken cancellationToken = default);

    private static void ValidateConfiguration(AssistantStepConfiguration configuration)
    {
        if (configuration.TargetGodotVersion < Constants.MinSupportedGodotVersion)
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_InvalidTargetGodotVersion(configuration.TargetGodotVersion, Constants.MinSupportedGodotVersion));
        }
    }
}

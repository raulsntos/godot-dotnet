using System;

namespace Godot.Bridge;

/// <summary>
/// Provides configuration to initialize the bridge between Godot and the .NET bindings.
/// </summary>
public sealed class GodotBridgeConfiguration
{
    internal Action<InitializationLevel>? InitCallback { get; private set; }
    internal Action<InitializationLevel>? TerminateCallback { get; private set; }
    internal InitializationLevel MinimumInitLevel { get; private set; } = InitializationLevel.Core;

    /// <summary>
    /// Registers the function that will be invoked when initializing the extension.
    /// The initialization function will be called for all the <see cref="InitializationLevel"/>
    /// values up to the current initialization level in the engine, skipping the
    /// levels below the minimum set using <see cref="SetMinimumLibraryInitializationLevel(InitializationLevel)"/>.
    /// </summary>
    /// <param name="init">Initialization function.</param>
    public void RegisterInitializer(Action<InitializationLevel> init)
    {
        InitCallback = init;
    }

    /// <summary>
    /// Registers the function that will be invoked when terminating the extension.
    /// The termination function will be called for all the <see cref="InitializationLevel"/>
    /// values starting from the current initialization level in the engine until reaching
    /// the lowest level.
    /// </summary>
    /// <param name="terminate">Termination function.</param>
    public void RegisterTerminator(Action<InitializationLevel> terminate)
    {
        TerminateCallback = terminate;
    }

    /// <summary>
    /// Sets the minimum required <see cref="InitializationLevel"/> for the extension.
    /// The extension won't be initialized until the engine reaches the requested
    /// initialization level.
    /// </summary>
    /// <param name="level">Minimum initialization level.</param>
    public void SetMinimumLibraryInitializationLevel(InitializationLevel level)
    {
        MinimumInitLevel = level;
    }
}

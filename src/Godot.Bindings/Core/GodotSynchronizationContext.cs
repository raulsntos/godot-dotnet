using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Godot;

public sealed class GodotSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = [];

    private GodotSynchronizationContext()
    {
    }

    /// <inheritdoc />
    public override void Send(SendOrPostCallback d, object? state)
    {
        // If we're already processing on the main thread, we can invoke directly.
        if (SynchronizationContext.Current == this)
        {
            d(state);
            return;
        }

        var completion = new TaskCompletionSource();

        _queue.Add((
            Callback: state =>
            {
                try
                {
                    d(state);
                }
                finally
                {
                    completion.SetResult();
                }
            },
            State: state
        ));

        completion.Task.Wait();
    }

    /// <inheritdoc />
    public override void Post(SendOrPostCallback d, object? state)
    {
        _queue.Add((Callback: d, State: state));
    }

    /// <inheritdoc cref="ExecutePendingContinuations"/>
    private void ExecutePendingContinuationsCore()
    {
        while (_queue.TryTake(out var workItem))
        {
            workItem.Callback.Invoke(workItem.State);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _queue.Dispose();
    }

    /// <summary>
    /// A singleton instance of the synchronization context.
    /// </summary>
    public static readonly GodotSynchronizationContext Singleton = new();

    /// <summary>
    /// Initializes the synchronization context. This should be called before any user code is executed.
    /// </summary>
    internal static void InitializeSynchronizationContext()
    {
        SynchronizationContext.SetSynchronizationContext(Singleton);
    }

    /// <summary>
    /// Executes all queued continuations.
    /// </summary>
    internal static void ExecutePendingContinuations()
    {
        if (SynchronizationContext.Current is not GodotSynchronizationContext context)
        {
            return;
        }

        context.ExecutePendingContinuationsCore();
    }
}

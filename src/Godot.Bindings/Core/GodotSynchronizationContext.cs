using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Godot;

internal sealed class GodotSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly struct WorkItem
    {
        public required SendOrPostCallback Callback { private get; init; }
        public object? State { private get; init; }
        public ManualResetEvent? Handle { private get; init; }

        public void Invoke()
        {
            try
            {
                Callback(State);
            }
            finally
            {
                Handle?.Set();
            }
        }
    }

    private readonly int _mainThreadId;
    private readonly BlockingCollection<WorkItem> _queue = new();

    private GodotSynchronizationContext(int mainThreadId)
    {
        _mainThreadId = mainThreadId;
    }

    /// <inheritdoc />
    public override void Send(SendOrPostCallback d, object? state)
    {
        // If we're already processing on the main thread, we can invoke directly.
        if (Environment.CurrentManagedThreadId == _mainThreadId)
        {
            d(state);
            return;
        }

        using var handle = new ManualResetEvent(false);

        _queue.Add(new WorkItem()
        {
            Callback = d,
            State = state,
            Handle = handle,
        });

        handle.WaitOne();
    }

    /// <inheritdoc />
    public override void Post(SendOrPostCallback d, object? state)
    {
        _queue.Add(new WorkItem()
        {
            Callback = d,
            State = state,
        });
    }

    /// <inheritdoc cref="ExecutePendingContinuations"/>
    private void ExecutePendingContinuationsCore()
    {
        while (_queue.TryTake(out var workItem))
        {
            workItem.Invoke();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _queue.Dispose();
    }

    /// <summary>
    /// Initializes the synchronization context. This should be called before any user code is executed.
    /// </summary>
    internal static void InitializeSynchronizationContext()
    {
        SynchronizationContext.SetSynchronizationContext(
            new GodotSynchronizationContext(Environment.CurrentManagedThreadId));
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

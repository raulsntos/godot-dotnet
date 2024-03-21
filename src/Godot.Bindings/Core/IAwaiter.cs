using System.Runtime.CompilerServices;

namespace Godot;

/// <summary>
/// A type that implements the required shape so it can be awaited and has no return value.
/// </summary>
internal interface IAwaiter : INotifyCompletion
{
    /// <summary>
    /// Indicates whether the await is completed.
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the await.
    /// </summary>
    public void GetResult();
}

/// <summary>
/// A type that implements the required shape so it can be awaited and has a return value.
/// </summary>
/// <typeparam name="TResult">The type of the result received after awaiting.</typeparam>
internal interface IAwaiter<out TResult> : INotifyCompletion
{
    /// <summary>
    /// Indicates whether the await is completed.
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the await.
    /// </summary>
    public TResult GetResult();
}

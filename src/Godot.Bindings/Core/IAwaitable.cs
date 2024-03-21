namespace Godot;

/// <summary>
/// A type that implements the required shape so it can be awaited and has no return value.
/// </summary>
internal interface IAwaitable
{
    /// <summary>
    /// Gets an awaiter for this awaitable.
    /// </summary>
    public IAwaiter GetAwaiter();
}

/// <summary>
/// A type that implements the required shape so it can be awaited and has a return value.
/// </summary>
/// <typeparam name="TAwaiter">The type of the object that implements the awaiter shape.</typeparam>
/// <typeparam name="TResult">The type of the result received after awaiting.</typeparam>
internal interface IAwaitable<TAwaiter, out TResult> where TAwaiter : IAwaiter<TResult>
{
    /// <summary>
    /// Gets an awaiter for this awaitable.
    /// </summary>
    public TAwaiter GetAwaiter();
}

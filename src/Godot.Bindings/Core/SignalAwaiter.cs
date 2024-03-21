using System;
using System.Diagnostics;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot;

/// <summary>
/// Provides an awaiter that can wait for the next emission of a signal.
/// </summary>
public sealed class SignalAwaiter : IAwaiter<Variant[]>, IAwaitable<SignalAwaiter, Variant[]>
{
    private readonly StringName _signal;
    private readonly GodotObject? _target;

    private bool _completed;
    private Variant[]? _result;
    private Action? _continuation;

    /// <summary>
    /// Constructs a new <see cref="SignalAwaiter"/> that connects to the given
    /// signal and can wait for the next emission.
    /// </summary>
    /// <param name="source">Owner of the signal that will be awaited.</param>
    /// <param name="signal">Name of the signal that will be awaited.</param>
    /// <param name="target">Object that constructs the awaiter.</param>
    /// <exception cref="InvalidOperationException">
    /// Error connecting to the signal.
    /// </exception>
    public SignalAwaiter(GodotObject source, StringName signal, GodotObject? target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(signal);

        _signal = signal;
        _target = target;

        var callable = new SignalAwaiterCallable(this);
        var callback = Callable.CreateTakingOwnership(callable);

        Error err = source.Connect(signal, callback, (uint)GodotObject.ConnectFlags.OneShot);
        if (err != Error.Ok)
        {
            throw new InvalidOperationException($"Error connecting to signal '{signal}' during await.");
        }
    }

    /// <summary>
    /// Whether the signal being awaited has been emitted.
    /// </summary>
    public bool IsCompleted => _completed;

    /// <summary>
    /// Sets the action to perform when the <see cref="SignalAwaiter"/> object
    /// stops waiting for the signal to be emitted.
    /// </summary>
    /// <param name="continuation">The action to perform when the wait operation completes.</param>
    public void OnCompleted(Action continuation)
    {
        _continuation = continuation;
    }

    /// <summary>
    /// Gets the <see cref="Variant"/> arguments sent by the signal, or an empty
    /// array if the signal hasn't been emitted yet.
    /// </summary>
    /// <returns>Signal arguments.</returns>
    public Variant[] GetResult() => _result ?? [];

    /// <summary>
    /// Gets an awaiter used to await the signal associated with this <see cref="SignalAwaiter"/>.
    /// </summary>
    /// <returns>An awaiter instance.</returns>
    public SignalAwaiter GetAwaiter() => this;

    private void SignalCallback(NativeGodotVariantPtrSpan args)
    {
        _completed = true;

        Variant[]? signalArgs = null;

        if (args.Length > 0)
        {
            signalArgs = new Variant[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                signalArgs[i] = Variant.CreateCopying(args[i]);
            }
        }

        _result = signalArgs;
        _continuation?.Invoke();
    }

    /// <summary>
    /// Converts this <see cref="SignalAwaiter"/> to a string.
    /// </summary>
    /// <returns>A string representation of this signal awaiter.</returns>
    public override string ToString()
    {
        if (_target is not null)
        {
            return $"{_target}::SignalAwaiter::{_signal}";
        }

        return $"null::SignalAwaiter::{_signal}";
    }

    private sealed class SignalAwaiterCallable : CustomCallable
    {
        private readonly SignalAwaiter _awaiter;

        public SignalAwaiterCallable(SignalAwaiter awaiter)
        {
            _awaiter = awaiter;
        }

        protected override ulong GetObjectId()
        {
            return _awaiter._target?.GetInstanceId() ?? 0;
        }

        protected override CallError _Call(ReadOnlySpan<Variant> args, out Variant result)
        {
            // We implement the internal Call method so this will never be called.
            throw new UnreachableException();
        }

        internal unsafe override void Call(NativeGodotVariantPtrSpan args, NativeGodotVariant* outRet, GDExtensionCallError* outError)
        {
            _awaiter.SignalCallback(args);
            outError->error = GDExtensionCallErrorType.GDEXTENSION_CALL_OK;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_awaiter._signal, _awaiter._target);
        }

        public override string ToString()
        {
            return _awaiter.ToString();
        }
    }
}

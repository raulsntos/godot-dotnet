using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot.Bridge;

namespace Godot.NativeInterop;

internal sealed class DelegateCallable : CustomCallable, IEquatable<DelegateCallable>
{
    private readonly unsafe delegate* managed<object, NativeGodotVariantPtrSpan, out NativeGodotVariant, void> _trampoline;

    public object? Target => Delegate.Target;

    public Delegate Delegate { get; }

    public unsafe DelegateCallable(Delegate @delegate, delegate* managed<object, NativeGodotVariantPtrSpan, out NativeGodotVariant, void> trampoline)
    {
        Delegate = @delegate;
        _trampoline = trampoline;
    }

    protected override ulong GetObjectId()
    {
        if (Target is GodotObject target)
        {
            return target.GetInstanceId();
        }

        return 0;
    }

    protected override bool TryGetArgumentCount(out long argCount)
    {
        argCount = Delegate.Method.GetParameters().Length;
        return true;
    }

    protected override CallError _Call(ReadOnlySpan<Variant> args, out Variant result)
    {
        // We implement the internal Call method so this will never be called.
        throw new UnreachableException();
    }

    internal override unsafe void Call(NativeGodotVariantPtrSpan args, NativeGodotVariant* outRet, GDExtensionCallError* outError)
    {
        Debug.Assert(Delegate is not null);
        Debug.Assert(_trampoline is not null);

        _trampoline(Delegate, args, out NativeGodotVariant ret);

        *outRet = ret;
        outError->error = GDExtensionCallErrorType.GDEXTENSION_CALL_OK;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Delegate);
    }

    public override bool Equals(object? obj)
    {
        return obj is DelegateCallable other && Equals(other);
    }

    public bool Equals(DelegateCallable? delegateCallable)
    {
        return EqualityComparer<Delegate>.Default.Equals(Delegate, delegateCallable?.Delegate);
    }

    public override string? ToString()
    {
        return Delegate.ToString();
    }
}

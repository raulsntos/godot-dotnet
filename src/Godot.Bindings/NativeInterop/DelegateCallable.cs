using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot.Bridge;

namespace Godot.NativeInterop;

internal sealed class DelegateCallable : CustomCallable, IEquatable<DelegateCallable>
{
    private readonly Delegate _delegate;
    private readonly unsafe delegate* managed<object, NativeGodotVariantPtrSpan, out NativeGodotVariant, void> _trampoline;

    public object? Target
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _delegate.Target;
    }

    public Delegate Delegate
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _delegate;
    }

    public unsafe DelegateCallable(Delegate @delegate, delegate* managed<object, NativeGodotVariantPtrSpan, out NativeGodotVariant, void> trampoline)
    {
        _delegate = @delegate;
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
        // TODO: Replace with DiagnosticMethodInfo in .NET 9.0
        // https://github.com/dotnet/runtime/issues/96528
        argCount = _delegate.Method.GetParameters().Length;
        return true;
    }

    protected override CallError _Call(ReadOnlySpan<Variant> args, out Variant result)
    {
        // We implement the internal Call method so this will never be called.
        throw new UnreachableException();
    }

    internal override unsafe void Call(NativeGodotVariantPtrSpan args, NativeGodotVariant* outRet, GDExtensionCallError* outError)
    {
        Debug.Assert(_delegate is not null);
        Debug.Assert(_trampoline is not null);

        _trampoline(_delegate, args, out NativeGodotVariant ret);

        *outRet = ret;
        outError->error = GDExtensionCallErrorType.GDEXTENSION_CALL_OK;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_delegate);
    }

    public override bool Equals(object? obj)
    {
        return obj is DelegateCallable other && Equals(other);
    }

    public bool Equals(DelegateCallable? delegateCallable)
    {
        return EqualityComparer<Delegate>.Default.Equals(_delegate, delegateCallable?._delegate);
    }

    public override string? ToString()
    {
        return _delegate.ToString();
    }
}

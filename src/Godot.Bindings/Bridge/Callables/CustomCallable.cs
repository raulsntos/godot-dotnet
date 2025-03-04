using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.NativeInterop;

namespace Godot.Bridge;

/// <summary>
/// Implements the custom behavior for an user-implemented <see cref="Callable"/>.
/// Most users should use <see cref="Callable"/> directly, this is a low-level
/// API to allow implementing custom behavior for special cases and it's used
/// internally to implement delegate-based Callables.
/// </summary>
public abstract class CustomCallable
{
    /// <summary>
    /// Convert a <see cref="CustomCallable"/> into a <see cref="Callable"/>.
    /// </summary>
    public static implicit operator Callable(CustomCallable customCallable)
    {
        return Callable.CreateTakingOwnership(customCallable);
    }

    internal unsafe NativeGodotCallable ConstructCallable()
    {
        var gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);

        var info = new GDExtensionCallableCustomInfo2()
        {
            callable_userdata = (void*)GCHandle.ToIntPtr(gcHandle),
            token = GodotBridge.LibraryPtr,
            object_id = GetObjectId(),
            call_func = &Call_Native,
            is_valid_func = &IsValid_Native,
            free_func = &Free_Native,
            hash_func = &Hash_Native,
            equal_func = &Equals_Native,
            less_than_func = &LessThan_Native,
            to_string_func = &ToString_Native,
            get_argument_count_func = &GetArgumentCount_Native,
        };

        NativeGodotCallable callable = default;
        GodotBridge.GDExtensionInterface.callable_custom_create2(&callable, &info);
        return callable;
    }

    /// <summary>
    /// The instance ID of the object that is the owner of this callable.
    /// </summary>
    /// <returns>Instance ID of the callable's owner.</returns>
    protected abstract ulong GetObjectId();

    /// <summary>
    /// Determines whether this callable is still valid.
    /// </summary>
    /// <returns><see langword="true"/> if the callable is valid.</returns>
    protected virtual bool IsValid() => true;

    /// <summary>
    /// Try to retrieve the argument count required by this callable.
    /// </summary>
    /// <param name="argCount">The number of parameters of the function.</param>
    /// <returns><see langword="true"/> if the argument count was retrieved successfully.</returns>
    protected virtual bool TryGetArgumentCount(out long argCount)
    {
        argCount = 0;
        return false;
    }

#pragma warning disable CA1707 // Identifiers should not contain underscores.
#pragma warning disable IDE1006 // Naming Styles.
    /// <summary>
    /// Implements the callback that will be invoked when this callable is called.
    /// </summary>
    /// <param name="args">Arguments that this callable is invoked with.</param>
    /// <param name="result">The value returned by the callable's invocation.</param>
    /// <returns>
    /// A status that indicates whether the call was successful, or the error that occurred otherwise.
    /// </returns>
    protected abstract CallError _Call(ReadOnlySpan<Variant> args, out Variant result);
#pragma warning restore IDE1006 // Naming Styles.
#pragma warning restore CA1707 // Identifiers should not contain underscores.

    internal virtual unsafe void Call(NativeGodotVariantPtrSpan args, NativeGodotVariant* outRet, GDExtensionCallError* outError)
    {
        Variant[] variantArgs = new Variant[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            variantArgs[i] = Variant.CreateCopying(args[i]);
        }

        CallError status = _Call(variantArgs, out Variant result);
        if (status != CallError.Ok)
        {
            outError->error = (GDExtensionCallErrorType)status;
            return;
        }

        *outRet = result.NativeValue.DangerousSelfRef;
        outError->error = GDExtensionCallErrorType.GDEXTENSION_CALL_OK;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void Call_Native(void* userData, NativeGodotVariant** args, long argsCount, NativeGodotVariant* outRet, GDExtensionCallError* outError)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        var callable = (CustomCallable?)gcHandle.Target;

        Debug.Assert(callable is not null);

        if (callable is null)
        {
            outError->error = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INSTANCE_IS_NULL;
            return;
        }

        callable.Call(new NativeGodotVariantPtrSpan(args, (int)argsCount), outRet, outError);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool IsValid_Native(void* userData)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        var callable = (CustomCallable?)gcHandle.Target;

        if (callable is null)
        {
            return false;
        }

        return callable.IsValid();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void Free_Native(void* userData)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        if (gcHandle.Target is IDisposable disposable)
        {
            disposable.Dispose();
        }
        gcHandle.Free();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe uint Hash_Native(void* userData)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        var callable = (CustomCallable?)gcHandle.Target;

        Debug.Assert(callable is not null);

        return (uint)callable.GetHashCode();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool Equals_Native(void* userDataLeft, void* userDataRight)
    {
        var gcHandleLeft = GCHandle.FromIntPtr((nint)userDataLeft);
        var callableLeft = (CustomCallable?)gcHandleLeft.Target;

        var gcHandleRight = GCHandle.FromIntPtr((nint)userDataRight);
        var callableRight = (CustomCallable?)gcHandleRight.Target;

        return EqualityComparer<CustomCallable>.Default.Equals(callableLeft, callableRight);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool LessThan_Native(void* userDataLeft, void* userDataRight)
    {
        var gcHandleLeft = GCHandle.FromIntPtr((nint)userDataLeft);
        var callableLeft = (CustomCallable?)gcHandleLeft.Target;

        var gcHandleRight = GCHandle.FromIntPtr((nint)userDataRight);
        var callableRight = (CustomCallable?)gcHandleRight.Target;

        return Comparer<CustomCallable>.Default.Compare(callableLeft, callableRight) switch
        {
            // callableLeft is less than callableRight.
            < 0 => true,
            // callableLeft equals callableRight.
            0 => false,
            // callableLeft is greater than callableRight.
            > 0 => false,
        };
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void ToString_Native(void* userData, bool* outIsValid, NativeGodotString* outStr)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        var callable = (CustomCallable?)gcHandle.Target;

        if (callable is null)
        {
            *outIsValid = false;
            *outStr = default;
        }
        else
        {
            *outIsValid = true;
            *outStr = NativeGodotString.Create(callable.ToString());
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe long GetArgumentCount_Native(void* userData, bool* outIsValid)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        var callable = (CustomCallable?)gcHandle.Target;

        Debug.Assert(callable is not null);

        *outIsValid = callable.TryGetArgumentCount(out long argCount);
        return argCount;
    }
}

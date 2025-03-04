using System;
using Godot.NativeInterop;

namespace Godot;

/// <summary>
/// Represents a signal defined in an object.
/// </summary>
public readonly struct Signal : IAwaitable<SignalAwaiter, Variant[]>
{
    internal readonly NativeGodotSignal.Movable NativeValue;

    /// <summary>
    /// Object that contains the signal.
    /// </summary>
    public GodotObject? Owner
    {
        get
        {
            ulong objectId = NativeValue.DangerousSelfRef.ObjectId;
            if (objectId == 0)
            {
                return null;
            }

            return GodotObject.InstanceFromId(objectId);
        }
    }

    /// <summary>
    /// Name of the signal.
    /// </summary>
    public StringName? Name
    {
        get
        {
            NativeGodotStringName name = NativeValue.DangerousSelfRef.Name;
            if (!name.IsAllocated)
            {
                return null;
            }

            return StringName.CreateCopying(name);
        }
    }

    private Signal(NativeGodotSignal nativeValueToOwn)
    {
        NativeValue = nativeValueToOwn.AsMovable();
    }

    /// <summary>
    /// Constructs a new <see cref="Signal"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static Signal CreateTakingOwnership(NativeGodotSignal nativeValueToOwn)
    {
        return new Signal(nativeValueToOwn);
    }

    /// <summary>
    /// Constructs a new <see cref="Signal"/> from the value borrowed from
    /// <paramref name="nativeValueToCopy"/>, copying the value.
    /// Since the new instance is a copy of the value, the caller is responsible
    /// of disposing the new instance to avoid memory leaks.
    /// </summary>
    internal static Signal CreateCopying(NativeGodotSignal nativeValueToCopy)
    {
        return new Signal(NativeGodotSignal.Create(nativeValueToCopy));
    }

    /// <summary>
    /// Creates a new <see cref="Signal"/> with the name <paramref name="name"/>
    /// in the specified <paramref name="owner"/>.
    /// </summary>
    /// <param name="owner">Object that contains the signal.</param>
    /// <param name="name">Name of the signal.</param>
    public Signal(GodotObject owner, StringName name)
    {
        NativeValue = NativeGodotSignal.Create(GodotObject.GetNativePtr(owner), name.NativeValue.DangerousSelfRef).AsMovable();
    }

    /// <summary>
    /// Gets a <see cref="SignalAwaiter"/> that can be awaited for the next
    /// emission of the signal.
    /// </summary>
    /// <returns>The awaiter for the signal.</returns>
    /// <exception cref="InvalidOperationException">
    /// Signal is invalid and has a null owner or name.
    /// </exception>
    public SignalAwaiter GetAwaiter()
    {
        GodotObject? owner = Owner;
        if (owner is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_SignalOwnerIsNull);
        }

        StringName? name = Name;
        if (name is null)
        {
            throw new InvalidOperationException(SR.InvalidOperation_SignalNameIsNull);
        }

        return new SignalAwaiter(owner, name, owner);
    }
}

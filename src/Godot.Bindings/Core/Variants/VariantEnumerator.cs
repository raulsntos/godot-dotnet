using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot;

/// <summary>
/// Enumerate the elements of a <see cref="Variant"/>, if it can be enumerated
/// (i.e.: GodotArray).
/// </summary>
public ref struct VariantEnumerator
{
    /// <summary>
    /// The variant that will be enumerated.
    /// </summary>
    private NativeGodotVariant _source;

    /// <summary>
    /// The state of the enumeration (i.e.: an index).
    /// </summary>
    private NativeGodotVariant _iterator;

    /// <summary>
    /// Indicates whether the enumerator has been initialized.
    /// </summary>
    private bool _isEnumeratorActive;

    /// <summary>
    /// Initialize the enumerator.
    /// </summary>
    /// <param name="source">The variant to enumerate.</param>
    internal VariantEnumerator(Variant source)
    {
        _source = source.NativeValue.DangerousSelfRef;
    }

    /// <summary>
    /// Gets the element at the current position of the enumerator.
    /// </summary>
    public Variant Current { get; private set; }

    /// <summary>
    /// Returns this instance as an enumerator.
    /// </summary>
    public readonly VariantEnumerator GetEnumerator() => this;

    /// <summary>
    /// Advances the enumerator to the next element of the variant.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the enumerator successfully advanced to the next element;
    /// <see langword="false"/> if the enumerator reached the last element.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The variant can't be enumerated.
    /// </exception>
    public bool MoveNext()
    {
        bool hasNext = !_isEnumeratorActive
            ? IterInit(_source, ref _iterator, out bool valid)
            : IterNext(_source, ref _iterator, out valid);
        ThrowIfInvalid(valid, _source);

        _isEnumeratorActive = true;

        Current = Variant.CreateTakingOwnership(IterGet(_source, ref _iterator, out valid));
        ThrowIfInvalid(valid, _source);

        return hasNext;
    }

    private static unsafe bool IterInit(NativeGodotVariant source, ref NativeGodotVariant iterator, out bool isValid)
    {
        Unsafe.SkipInit(out bool valid);

        bool result = GodotBridge.GDExtensionInterface.variant_iter_init(source.GetUnsafeAddress(), iterator.GetUnsafeAddress(), &valid);

        isValid = valid;
        return result;
    }

    private static unsafe bool IterNext(NativeGodotVariant source, ref NativeGodotVariant iterator, out bool isValid)
    {
        Unsafe.SkipInit(out bool valid);

        bool result = GodotBridge.GDExtensionInterface.variant_iter_next(source.GetUnsafeAddress(), iterator.GetUnsafeAddress(), &valid);

        isValid = valid;
        return result;
    }

    private static unsafe NativeGodotVariant IterGet(NativeGodotVariant source, ref NativeGodotVariant iterator, out bool isValid)
    {
        Unsafe.SkipInit(out bool valid);
        NativeGodotVariant result = default;

        GodotBridge.GDExtensionInterface.variant_iter_get(source.GetUnsafeAddress(), iterator.GetUnsafeAddress(), result.GetUnsafeAddress(), &valid);

        isValid = valid;
        return result;
    }

    private static void ThrowIfInvalid([DoesNotReturnIf(false)] bool valid, NativeGodotVariant source)
    {
        if (!valid)
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_VariantCantBeEnumerated(Variant.CreateTakingOwnership(source)));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot.Collections;

/// <summary>
/// Wrapper around Godot's Dictionary class, a dictionary of Variant
/// typed elements allocated in the engine in C++. Useful when
/// interfacing with the engine.
/// </summary>
[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerDisplay("Count = {Count}")]
public sealed class GodotDictionary :
    IDictionary<Variant, Variant>,
    IReadOnlyDictionary<Variant, Variant>,
    IDisposable
{
    internal NativeGodotDictionary.Movable NativeValue;

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    /// <summary>
    /// Constructs a new empty <see cref="GodotDictionary"/>.
    /// </summary>
    /// <returns>A new Godot Dictionary.</returns>
    public GodotDictionary()
    {
        NativeValue = NativeGodotDictionary.Create().AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    private GodotDictionary(NativeGodotDictionary nativeValueToOwn)
    {
        NativeValue = (nativeValueToOwn.IsAllocated
            ? nativeValueToOwn
            : NativeGodotDictionary.Create()).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotDictionary"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static GodotDictionary CreateTakingOwnership(NativeGodotDictionary nativeValueToOwn)
    {
        return new GodotDictionary(nativeValueToOwn);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotDictionary"/> from the value borrowed from
    /// <paramref name="nativeValueToCopy"/>, copying the value.
    /// Since the new instance is a copy of the value, the caller is responsible
    /// of disposing the new instance to avoid memory leaks.
    /// </summary>
    internal static GodotDictionary CreateCopying(NativeGodotDictionary nativeValueToCopy)
    {
        return new GodotDictionary(NativeGodotDictionary.Create(nativeValueToCopy));
    }

    /// <summary>
    /// Releases the unmanaged <see cref="GodotDictionary"/> instance.
    /// </summary>
    ~GodotDictionary()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="GodotDictionary"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        // Always dispose `NativeValue` even if disposing is true.
        NativeValue.DangerousSelfRef.Dispose();

        if (_weakReferenceToSelf is not null)
        {
            DisposablesTracker.UnregisterDisposable(_weakReferenceToSelf);
        }
    }

    /// <summary>
    /// Returns the value at the given <paramref name="key"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The property is assigned and the dictionary is read-only.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// The property is retrieved and an entry for <paramref name="key"/>
    /// does not exist in the dictionary.
    /// </exception>
    /// <value>The value at the given <paramref name="key"/>.</value>
    public unsafe Variant this[Variant key]
    {
        get
        {
            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

            NativeGodotVariant* value = self.GetPtrw(key.NativeValue.DangerousSelfRef);
            if (value is null)
            {
                throw new KeyNotFoundException(SR.FormatKeyNotFound_DictionaryKeyNotFound(key));
            }

            return Variant.CreateCopying(*value);
        }
        set
        {
            ThrowIfReadOnly();

            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
            NativeGodotVariant variantKey = key.NativeValue.DangerousSelfRef;
            NativeGodotVariant* itemPtr = self.GetPtrw(variantKey);
            itemPtr->Dispose();
            *itemPtr = NativeGodotVariant.Create(value.NativeValue.DangerousSelfRef);
        }
    }

    /// <summary>
    /// Gets the collection of keys in this <see cref="GodotDictionary"/>.
    /// </summary>
    public ICollection<Variant> Keys
    {
        get
        {
            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
            NativeGodotArray keysArray = NativeGodotDictionary.Keys(in self);
            return GodotArray.CreateTakingOwnership(keysArray);
        }
    }

    /// <summary>
    /// Gets the collection of elements in this <see cref="GodotDictionary"/>.
    /// </summary>
    public ICollection<Variant> Values
    {
        get
        {
            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
            NativeGodotArray valuesArray = NativeGodotDictionary.Values(in self);
            return GodotArray.CreateTakingOwnership(valuesArray);
        }
    }

    IEnumerable<Variant> IReadOnlyDictionary<Variant, Variant>.Keys => Keys;

    IEnumerable<Variant> IReadOnlyDictionary<Variant, Variant>.Values => Values;

    /// <summary>
    /// Returns the number of elements in this <see cref="GodotDictionary"/>.
    /// This is also known as the size or length of the dictionary.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count
    {
        get
        {
            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
            return (int)NativeGodotDictionary.GetSize(in self);
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the dictionary is read-only.
    /// See <see cref="MakeReadOnly"/>.
    /// </summary>
    public bool IsReadOnly => NativeValue.DangerousSelfRef.IsReadOnly;

    /// <summary>
    /// Makes the <see cref="GodotDictionary"/> read-only, i.e. disabled modying of the
    /// dictionary's elements. Does not apply to nested content, e.g. content of
    /// nested dictionaries.
    /// </summary>
    public void MakeReadOnly()
    {
        if (IsReadOnly)
        {
            // Avoid interop call when the dictionary is already read-only.
            return;
        }

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        NativeGodotDictionary.MakeReadOnly(ref self);
    }

    /// <summary>
    /// Adds an value <paramref name="value"/> at key <paramref name="key"/>
    /// to this <see cref="GodotDictionary"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The dictionary is read-only.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An entry for <paramref name="key"/> already exists in the dictionary.
    /// </exception>
    /// <param name="key">The key at which to add the value.</param>
    /// <param name="value">The value to add.</param>
    public unsafe void Add(Variant key, Variant value)
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantKey = key.NativeValue.DangerousSelfRef;

        if (NativeGodotDictionary.Has(in self, variantKey))
        {
            throw new ArgumentException(SR.Argument_DictionaryKeyAlreadyExists, nameof(key));
        }

        NativeGodotVariant* itemPtr = self.GetPtrw(variantKey);
        itemPtr->Dispose();
        *itemPtr = NativeGodotVariant.Create(value.NativeValue.DangerousSelfRef);
    }

    void ICollection<KeyValuePair<Variant, Variant>>.Add(KeyValuePair<Variant, Variant> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// Clears the dictionary, removing all entries from it.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The dictionary is read-only.
    /// </exception>
    public void Clear()
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        NativeGodotDictionary.Clear(ref self);
    }

    /// <summary>
    /// Checks if this <see cref="GodotDictionary"/> contains the given key.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <returns>Whether or not this dictionary contains the given key.</returns>
    public bool ContainsKey(Variant key)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        return NativeGodotDictionary.Has(in self, key.NativeValue.DangerousSelfRef);
    }

    unsafe bool ICollection<KeyValuePair<Variant, Variant>>.Contains(KeyValuePair<Variant, Variant> item)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        NativeGodotVariant itemKey = item.Key.NativeValue.DangerousSelfRef;

        NativeGodotVariant* value = self.GetPtrw(itemKey);
        if (value is null)
        {
            return false;
        }

        NativeGodotVariant itemValue = item.Value.NativeValue.DangerousSelfRef;
        return NativeGodotVariant.Equals(itemValue, *value);
    }

    /// <summary>
    /// Copies the elements of this <see cref="GodotDictionary"/> to the given untyped
    /// <see cref="KeyValuePair{TKey, TValue}"/> array, starting at the given index.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0 or greater than the array's size.
    /// -or-
    /// The destination array was not big enough.
    /// </exception>
    /// <param name="array">The array to copy to.</param>
    /// <param name="arrayIndex">The index to start at.</param>
    void ICollection<KeyValuePair<Variant, Variant>>.CopyTo(KeyValuePair<Variant, Variant>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        var (keys, values, count) = GetKeyValuePairs();

        for (int i = 0; i < count; i++)
        {
            array[arrayIndex] = new(keys[i], values[i]);
            arrayIndex++;
        }
    }

    /// <summary>
    /// Returns a copy of the <see cref="GodotDictionary"/>.
    /// If <paramref name="deep"/> is <see langword="true"/>, a deep copy is performed:
    /// all nested arrays and dictionaries are duplicated and will not be shared with
    /// the original dictionary. If <see langword="false"/>, a shallow copy is made and
    /// references to the original nested arrays and dictionaries are kept, so that
    /// modifying a sub-array or dictionary in the copy will also impact those
    /// referenced in the source dictionary. Note that any <see cref="GodotObject"/> derived
    /// elements will be shallow copied regardless of the <paramref name="deep"/>
    /// setting.
    /// </summary>
    /// <param name="deep">If <see langword="true"/>, performs a deep copy.</param>
    /// <returns>A new Godot Dictionary.</returns>
    public GodotDictionary Duplicate(bool deep = false)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        NativeGodotDictionary newDictionary = NativeGodotDictionary.Duplicate(in self, deep);
        return CreateTakingOwnership(newDictionary);
    }

    /// <summary>
    /// Adds entries from <paramref name="dictionary"/> to this dictionary.
    /// By default, duplicate keys are not copied over, unless <paramref name="overwrite"/>
    /// is <see langword="true"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The dictionary is read-only.
    /// </exception>
    /// <param name="dictionary">Dictionary to copy entries from.</param>
    /// <param name="overwrite">If duplicate keys should be copied over as well.</param>
    public void Merge(GodotDictionary dictionary, bool overwrite = false)
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        NativeGodotDictionary other = dictionary.NativeValue.DangerousSelfRef;
        NativeGodotDictionary.Merge(ref self, other, overwrite);
    }

    /// <summary>
    /// Compares this <see cref="GodotDictionary"/> against the <paramref name="other"/>
    /// <see cref="GodotDictionary"/> recursively. Returns <see langword="true"/> if the
    /// two dictionaries contain the same keys and values. The order of the entries
    /// does not matter.
    /// otherwise.
    /// </summary>
    /// <param name="other">The other dictionary to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the dictionaries contain the same keys and values,
    /// <see langword="false"/> otherwise.
    /// </returns>
    public bool RecursiveEqual(GodotDictionary other)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        NativeGodotDictionary otherVariant = other.NativeValue.DangerousSelfRef;
        return NativeGodotDictionary.OperatorEqual(self, otherVariant);
    }

    /// <summary>
    /// Removes an element from this <see cref="GodotDictionary"/> by key.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The dictionary is read-only.
    /// </exception>
    /// <param name="key">The key of the element to remove.</param>
    public bool Remove(Variant key)
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        return NativeGodotDictionary.Erase(ref self, key.NativeValue.DangerousSelfRef);
    }

    unsafe bool ICollection<KeyValuePair<Variant, Variant>>.Remove(KeyValuePair<Variant, Variant> item)
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        NativeGodotVariant itemKey = item.Key.NativeValue.DangerousSelfRef;

        NativeGodotVariant* value = self.GetPtrw(itemKey);
        if (value is null)
        {
            return false;
        }

        NativeGodotVariant itemValue = item.Value.NativeValue.DangerousSelfRef;
        if (!NativeGodotVariant.Equals(itemValue, *value))
        {
            return false;
        }

        return NativeGodotDictionary.Erase(ref self, itemKey);
    }

    /// <summary>
    /// Gets the value for the given <paramref name="key"/> in the dictionary.
    /// Returns <see langword="true"/> if an entry for the given key exists in
    /// the dictionary; otherwise, returns <see langword="false"/>.
    /// </summary>
    /// <param name="key">The key of the element to get.</param>
    /// <param name="value">The value at the given <paramref name="key"/>.</param>
    /// <returns>If an entry was found for the given <paramref name="key"/>.</returns>
    public unsafe bool TryGetValue(Variant key, out Variant value)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        NativeGodotVariant keyNative = key.NativeValue.DangerousSelfRef;

        NativeGodotVariant* valueNative = self.GetPtrw(keyNative);
        if (valueNative is null)
        {
            value = default;
            return false;
        }

        value = Variant.CreateCopying(*valueNative);
        return true;
    }

    private unsafe KeyValuePair<Variant, Variant> GetKeyValuePair(int index)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        NativeGodotVariant key = NativeGodotDictionary.Keys(in self).GetPtrw()[index];
        NativeGodotVariant value = NativeGodotDictionary.Values(in self).GetPtrw()[index];

        return new KeyValuePair<Variant, Variant>(
            Variant.CreateTakingOwnership(key),
            Variant.CreateTakingOwnership(value));
    }

    private (GodotArray Keys, GodotArray Values, int Count) GetKeyValuePairs()
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        NativeGodotArray keysArray = NativeGodotDictionary.Keys(in self);
        var keys = GodotArray.CreateTakingOwnership(keysArray);

        NativeGodotArray valuesArray = NativeGodotDictionary.Values(in self);
        var values = GodotArray.CreateTakingOwnership(valuesArray);

        int count = (int)NativeGodotDictionary.GetSize(in self);

        return (keys, values, count);
    }

    // IEnumerable

    /// <summary>
    /// Gets an enumerator for this <see cref="GodotDictionary"/>.
    /// </summary>
    /// <returns>An enumerator.</returns>
    public IEnumerator<KeyValuePair<Variant, Variant>> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return GetKeyValuePair(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class DebugView
    {
        private readonly GodotDictionary _dictionary;

        public DebugView(GodotDictionary dictionary)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<Variant, Variant>[] Items => [.. _dictionary];
    }

    /// <summary>
    /// Converts this <see cref="GodotDictionary"/> to a string.
    /// </summary>
    /// <returns>A string representation of this dictionary.</returns>
    public override unsafe string ToString()
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant selfVariant = new() { Dictionary = self, Type = VariantType.Dictionary };
        using NativeGodotString str = default;
        GodotBridge.GDExtensionInterface.variant_stringify(&selfVariant, &str);
        return str.ToString();
    }

    private void ThrowIfReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException(SR.InvalidOperation_DictionaryIsReadOnly);
        }
    }
}

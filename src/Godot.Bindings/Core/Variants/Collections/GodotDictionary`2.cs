using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Godot.NativeInterop;

namespace Godot.Collections;

internal interface IGenericGodotDictionary
{
    public GodotDictionary UnderlyingDictionary { get; }
}

/// <summary>
/// Typed wrapper around Godot's Dictionary class, a dictionary of Variant
/// typed elements allocated in the engine in C++. Useful when
/// interfacing with the engine. Otherwise prefer .NET collections
/// such as <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
/// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
[DebuggerTypeProxy(typeof(GodotDictionary<,>.DebugView))]
[DebuggerDisplay("Count = {Count}")]
public sealed class GodotDictionary<[MustBeVariant] TKey, [MustBeVariant] TValue> :
    IDictionary<TKey, TValue>,
    IReadOnlyDictionary<TKey, TValue>,
    IGenericGodotDictionary,
    IDisposable
{
    private unsafe static void WriteUnmanagedFunc(in GodotDictionary<TKey, TValue> value, void* destination) =>
        *(NativeGodotDictionary*)destination = value.NativeValue.DangerousSelfRef;

    private unsafe static GodotDictionary<TKey, TValue> ConvertFromUnmanagedFunc(void* ptr) =>
        GodotDictionary<TKey, TValue>.CreateTakingOwnership(*(NativeGodotDictionary*)ptr);

    private static NativeGodotVariant ConvertToVariantFunc(in GodotDictionary<TKey, TValue> from) =>
        from is not null
            ? NativeGodotVariant.CreateFromDictionaryCopying(from.NativeValue.DangerousSelfRef)
            : default;

    private static GodotDictionary<TKey, TValue> ConvertFromVariantFunc(in NativeGodotVariant variant) =>
        GodotDictionary<TKey, TValue>.CreateTakingOwnership(NativeGodotVariant.ConvertToDictionary(variant));

    static unsafe GodotDictionary()
    {
        Marshalling.GenericConversion<GodotDictionary<TKey, TValue>>.AssignToPtrCb = &WriteUnmanagedFunc;
        Marshalling.GenericConversion<GodotDictionary<TKey, TValue>>.FromPtrCb = &ConvertFromUnmanagedFunc;
        Marshalling.GenericConversion<GodotDictionary<TKey, TValue>>.ToVariantCb = &ConvertToVariantFunc;
        Marshalling.GenericConversion<GodotDictionary<TKey, TValue>>.FromVariantCb = &ConvertFromVariantFunc;
    }

    private readonly GodotDictionary _underlyingDict;

    GodotDictionary IGenericGodotDictionary.UnderlyingDictionary => _underlyingDict;

    internal ref NativeGodotDictionary.Movable NativeValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _underlyingDict.NativeValue;
    }

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    /// <summary>
    /// Constructs a new empty <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>A new Godot Dictionary.</returns>
    public GodotDictionary()
    {
        _underlyingDict = [];
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotDictionary{TKey, TValue}"/> from the given dictionary's elements.
    /// </summary>
    /// <param name="underlyingDictionary">The untyped dictionary to use as the underlying dictionary.</param>
    /// <returns>A new Godot Dictionary instance with the same underlying dictionary.</returns>
    private GodotDictionary(GodotDictionary underlyingDictionary)
    {
        _underlyingDict = underlyingDictionary;
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotDictionary{TKey, TValue}"/> from the given dictionary's elements.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="dictionary"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="dictionary">The dictionary to construct from.</param>
    /// <returns>A new Godot Dictionary.</returns>
    public GodotDictionary(IDictionary<TKey, TValue> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);

        // If the collection is another Godot Dictionary, we can add the items
        // with a single interop call.
        if (dictionary is GodotDictionary godotDictionary)
        {
            _underlyingDict = godotDictionary.Duplicate(deep: false);
            return;
        }
        else if (dictionary is GodotDictionary<TKey, TValue> typedGodotDictionary)
        {
            _underlyingDict = typedGodotDictionary._underlyingDict.Duplicate(deep: false);
            return;
        }

        _underlyingDict = [];

        foreach (var (key, value) in dictionary)
        {
            Add(key, value);
        }
    }

    /// <summary>
    /// Constructs a new <see cref="GodotDictionary{TKey, TValue}"/> from the
    /// value borrowed from <paramref name="nativeValueToOwn"/>, taking ownership
    /// of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static GodotDictionary<TKey, TValue> CreateTakingOwnership(NativeGodotDictionary nativeValueToOwn)
    {
        return new GodotDictionary<TKey, TValue>(GodotDictionary.CreateTakingOwnership(nativeValueToOwn));
    }

    /// <summary>
    /// Converts an untyped <see cref="GodotDictionary"/> to a typed <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="from">The untyped dictionary to convert.</param>
    [return: NotNullIfNotNull(nameof(from))]
    public static explicit operator GodotDictionary<TKey, TValue>?(GodotDictionary? from)
    {
        return from is not null ? new GodotDictionary<TKey, TValue>(from) : null;
    }

    /// <summary>
    /// Converts this typed <see cref="GodotDictionary{TKey, TValue}"/> to an untyped <see cref="GodotDictionary"/>.
    /// </summary>
    /// <param name="from">The typed dictionary to convert.</param>
    [return: NotNullIfNotNull(nameof(from))]
    public static explicit operator GodotDictionary?(GodotDictionary<TKey, TValue>? from)
    {
        return from?._underlyingDict;
    }

    /// <summary>
    /// Releases the unmanaged <see cref="GodotDictionary{TKey, TValue}"/> instance.
    /// </summary>
    ~GodotDictionary()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        // Always dispose `_underlyingDict` even if disposing is true.
        _underlyingDict.Dispose();

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
    public unsafe TValue this[TKey key]
    {
        get
        {
            using NativeGodotVariant variantKey = Marshalling.ConvertToVariant(in key);
            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

            NativeGodotVariant* variantValue = self.GetPtrw(variantKey);
            if (variantValue is null)
            {
                throw new KeyNotFoundException();
            }

            return Marshalling.ConvertFromVariant<TValue>(*variantValue);
        }
        set
        {
            ThrowIfReadOnly();

            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
            using NativeGodotVariant variantKey = Marshalling.ConvertToVariant(in key);

            NativeGodotVariant* itemPtr = self.GetPtrw(variantKey);
            itemPtr->Dispose();
            *itemPtr = Marshalling.ConvertToVariant(in value);
        }
    }

    /// <summary>
    /// Gets the collection of keys in this <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    public ICollection<TKey> Keys
    {
        get
        {
            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
            NativeGodotArray keyArray = NativeGodotDictionary.Keys(in self);
            return GodotArray<TKey>.CreateTakingOwnership(keyArray);
        }
    }

    /// <summary>
    /// Gets the collection of elements in this <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    public ICollection<TValue> Values
    {
        get
        {
            ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
            NativeGodotArray valuesArray = NativeGodotDictionary.Values(in self);
            return GodotArray<TValue>.CreateTakingOwnership(valuesArray);
        }
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    /// <summary>
    /// Returns the number of elements in this <see cref="GodotDictionary{TKey, TValue}"/>.
    /// This is also known as the size or length of the dictionary.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count => _underlyingDict.Count;

    /// <summary>
    /// Returns <see langword="true"/> if the dictionary is read-only.
    /// See <see cref="MakeReadOnly"/>.
    /// </summary>
    public bool IsReadOnly => _underlyingDict.IsReadOnly;

    /// <summary>
    /// Makes the <see cref="GodotDictionary{TKey, TValue}"/> read-only, i.e. disabled
    /// modying of the dictionary's elements. Does not apply to nested content,
    /// e.g. content of nested dictionaries.
    /// </summary>
    public void MakeReadOnly()
    {
        _underlyingDict.MakeReadOnly();
    }

    /// <summary>
    /// Adds an object <paramref name="value"/> at key <paramref name="key"/>
    /// to this <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The dictionary is read-only.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An element with the same <paramref name="key"/> already exists.
    /// </exception>
    /// <param name="key">The key at which to add the object.</param>
    /// <param name="value">The object to add.</param>
    public unsafe void Add(TKey key, TValue value)
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantKey = Marshalling.ConvertToVariant(in key);

        if (NativeGodotDictionary.Has(in self, variantKey))
        {
            throw new ArgumentException("An element with the same key already exists.", nameof(key));
        }

        NativeGodotVariant* itemPtr = self.GetPtrw(variantKey);
        itemPtr->Dispose();
        *itemPtr = Marshalling.ConvertToVariant(in value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
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
        _underlyingDict.Clear();
    }

    /// <summary>
    /// Checks if this <see cref="GodotDictionary{TKey, TValue}"/> contains the given key.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <returns>Whether or not this dictionary contains the given key.</returns>
    public bool ContainsKey(TKey key)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantKey = Marshalling.ConvertToVariant(in key);
        return NativeGodotDictionary.Has(in self, variantKey);
    }

    unsafe bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        using NativeGodotVariant variantItemKey = Marshalling.ConvertToVariant(item.Key);

        NativeGodotVariant* variantValue = self.GetPtrw(variantItemKey);
        if (variantValue is null)
        {
            return false;
        }

        using NativeGodotVariant variantItemValue = Marshalling.ConvertToVariant(item.Value);
        return NativeGodotVariant.Equals(variantItemValue, *variantValue);
    }

    /// <summary>
    /// Copies the elements of this <see cref="GodotDictionary{TKey, TValue}"/> to the given
    /// untyped C# array, starting at the given index.
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
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        int count = Count;
        for (int i = 0; i < count; i++)
        {
            array[arrayIndex] = GetKeyValuePair(i);
            arrayIndex++;
        }
    }

    /// <summary>
    /// Returns a copy of the <see cref="GodotDictionary{TKey, TValue}"/>.
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
    public GodotDictionary<TKey, TValue> Duplicate(bool deep = false)
    {
        return new GodotDictionary<TKey, TValue>(_underlyingDict.Duplicate(deep));
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
    public void Merge(GodotDictionary<TKey, TValue> dictionary, bool overwrite = false)
    {
        _underlyingDict.Merge(dictionary._underlyingDict, overwrite);
    }

    /// <summary>
    /// Compares this <see cref="GodotDictionary{TKey, TValue}"/> against the <paramref name="other"/>
    /// <see cref="GodotDictionary{TKey, TValue}"/> recursively. Returns <see langword="true"/> if the
    /// two dictionaries contain the same keys and values. The order of the entries does not matter.
    /// otherwise.
    /// </summary>
    /// <param name="other">The other dictionary to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the dictionaries contain the same keys and values,
    /// <see langword="false"/> otherwise.
    /// </returns>
    public bool RecursiveEqual(GodotDictionary<TKey, TValue> other)
    {
        return _underlyingDict.RecursiveEqual(other._underlyingDict);
    }

    /// <summary>
    /// Removes an element from this <see cref="GodotDictionary{TKey, TValue}"/> by key.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The dictionary is read-only.
    /// </exception>
    /// <param name="key">The key of the element to remove.</param>
    public bool Remove(TKey key)
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantKey = Marshalling.ConvertToVariant(in key);
        return NativeGodotDictionary.Erase(ref self, variantKey);
    }

    unsafe bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        ThrowIfReadOnly();

        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        using NativeGodotVariant variantItemKey = Marshalling.ConvertToVariant(item.Key);

        NativeGodotVariant* variantValue = self.GetPtrw(variantItemKey);
        if (variantValue is null)
        {
            return false;
        }

        using NativeGodotVariant variantItemValue = Marshalling.ConvertToVariant(item.Value);
        if (!NativeGodotVariant.Equals(variantItemValue, *variantValue))
        {
            return false;
        }

        return NativeGodotDictionary.Erase(ref self, variantItemKey);
    }

    /// <summary>
    /// Gets the value for the given <paramref name="key"/> in the dictionary.
    /// Returns <see langword="true"/> if an entry for the given key exists in
    /// the dictionary; otherwise, returns <see langword="false"/>.
    /// </summary>
    /// <param name="key">The key of the element to get.</param>
    /// <param name="value">The value at the given <paramref name="key"/>.</param>
    /// <returns>If an entry was found for the given <paramref name="key"/>.</returns>
    public unsafe bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        using NativeGodotVariant variantKey = Marshalling.ConvertToVariant(in key);

        NativeGodotVariant* variantValue = self.GetPtrw(variantKey);
        if (variantValue is null)
        {
            value = default;
            return false;
        }

        value = Marshalling.ConvertFromVariant<TValue>(*variantValue);
        return true;
    }

    private unsafe KeyValuePair<TKey, TValue> GetKeyValuePair(int index)
    {
        ref NativeGodotDictionary self = ref NativeValue.DangerousSelfRef;

        NativeGodotVariant key = NativeGodotDictionary.Keys(in self).GetPtrw()[index];
        NativeGodotVariant value = NativeGodotDictionary.Values(in self).GetPtrw()[index];

        return new KeyValuePair<TKey, TValue>(
            Marshalling.ConvertFromVariant<TKey>(key),
            Marshalling.ConvertFromVariant<TValue>(value));
    }

    // IEnumerable

    /// <summary>
    /// Gets an enumerator for this <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>An enumerator.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return GetKeyValuePair(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class DebugView
    {
        private readonly GodotDictionary<TKey, TValue> _dictionary;

        public DebugView(GodotDictionary<TKey, TValue> dictionary)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items => [.. _dictionary];
    }

    /// <summary>
    /// Converts this <see cref="GodotDictionary{TKey, TValue}"/> to a string.
    /// </summary>
    /// <returns>A string representation of this dictionary.</returns>
    public override string ToString() => _underlyingDict.ToString();

    /// <summary>
    /// Converts this <see cref="GodotDictionary{TKey, TValue}"/> to a <see cref="Variant"/>.
    /// </summary>
    /// <returns>A Variant that contains this dictionary.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(GodotDictionary<TKey, TValue> from) => Variant.CreateFrom(from);

    /// <summary>
    /// Converts the <see cref="Variant"/> to a <see cref="GodotDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>The dictionary contained in the Variant.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GodotDictionary<TKey, TValue>(Variant from) =>
        from.AsGodotDictionary<TKey, TValue>();

    private void ThrowIfReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Dictionary instance is read-only.");
        }
    }
}

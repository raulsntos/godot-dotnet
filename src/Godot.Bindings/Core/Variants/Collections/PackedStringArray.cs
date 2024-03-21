using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot.Collections;

/// <summary>
/// Wrapper around Godot's Packed String Array class, and array of strings
/// allocated in the engine in C++. Useful when interfacing with the engine.
/// Otherwise prefer .NET collections such as <see cref="Array"/> or
/// <see cref="List{T}"/>.
/// </summary>
[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(PackedArray), nameof(PackedArray.Create))]
public sealed class PackedStringArray :
    IList<string>,
    IReadOnlyList<string>,
    IDisposable
{
    internal readonly NativeGodotPackedStringArray.Movable NativeValue;

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    /// <summary>
    /// Constructs a new empty <see cref="PackedStringArray"/>.
    /// </summary>
    /// <returns>A new Packed String Array.</returns>
    public PackedStringArray()
    {
        NativeValue = NativeGodotPackedStringArray.Create().AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    private PackedStringArray(NativeGodotPackedStringArray nativeValueToOwn)
    {
        NativeValue = (nativeValueToOwn.IsAllocated
            ? nativeValueToOwn
            : NativeGodotPackedStringArray.Create()).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedStringArray"/> from the given collection's elements.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">The collection of elements to construct from.</param>
    /// <returns>A new Packed String Array.</returns>
    public PackedStringArray(IEnumerable<string> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);

        // If the collection is another Packed Array, we can add the items
        // with a single interop call.
        if (collection is PackedStringArray packedArray)
        {
            NativeValue = NativeGodotPackedStringArray.Duplicate(ref packedArray.NativeValue.DangerousSelfRef).AsMovable();
            return;
        }
        if (collection is string[] array)
        {
            NativeValue = NativeGodotPackedStringArray.Create(array).AsMovable();
            return;
        }

        NativeValue = NativeGodotPackedStringArray.Create().AsMovable();

        AddRangeCore(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedStringArray"/> from the given span.
    /// </summary>
    /// <param name="array">The elements to construct from.</param>
    /// <returns>A new Packed String Array.</returns>
    public PackedStringArray(ReadOnlySpan<string> array)
    {
        NativeValue = NativeGodotPackedStringArray.Create(array).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedStringArray"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static PackedStringArray CreateTakingOwnership(NativeGodotPackedStringArray nativeValueToOwn)
    {
        return new PackedStringArray(nativeValueToOwn);
    }

    /// <summary>
    /// Releases the unmanaged <see cref="PackedStringArray"/> instance.
    /// </summary>
    ~PackedStringArray()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="PackedStringArray"/>.
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
    /// Returns the item at the given <paramref name="index"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <value>The <see langword="string"/> item at the given <paramref name="index"/>.</value>
    public unsafe string this[int index]
    {
        get
        {
            NativeGodotString* ptrw = NativeValue.DangerousSelfRef.GetPtrw();
            return ptrw[index].ToString();
        }

        set
        {
            NativeGodotString* ptrw = NativeValue.DangerousSelfRef.GetPtrw();
            ptrw[index] = NativeGodotString.Create(value);
        }
    }

    /// <summary>
    /// Returns the number of elements in this <see cref="PackedStringArray"/>.
    /// This is also known as the size or length of the array.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count => NativeValue.DangerousSelfRef.Size;

    bool ICollection<string>.IsReadOnly => false;

    /// <summary>
    /// Adds an item to the end of this <see cref="PackedStringArray"/>.
    /// This is the same as <c>append</c> or <c>push_back</c> in GDScript.
    /// </summary>
    /// <param name="item">The <see langword="string"/> item to add.</param>
    public void Add(string item)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotString itemNative = NativeGodotString.Create(item);
        NativeGodotPackedStringArray.Append(ref self, itemNative);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of this <see cref="PackedStringArray"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">Collection of <see langword="string"/> items to add.</param>
    public void AddRange(IEnumerable<string> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        AddRangeCore(collection);
    }

    private void AddRangeCore(IEnumerable<string> collection)
    {
        // If we can retrieve the count of the collection without enumerating it
        // (e.g.: the collections is a List<T>), use it to resize the array once
        // instead of growing it as we add items.
        if (collection.TryGetNonEnumeratedCount(out int count))
        {
            int oldCount = Count;
            Resize(Count + count);

            using var enumerator = collection.GetEnumerator();

            for (int i = 0; i < count; i++)
            {
                enumerator.MoveNext();
                this[oldCount + i] = enumerator.Current;
            }

            return;
        }

        foreach (var item in collection)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Clears the array. This is the equivalent to using <see cref="Resize(int)"/>
    /// with a size of <c>0</c>
    /// </summary>
    public void Clear() => Resize(0);

    /// <summary>
    /// Returns <see langword="true"/> if the array contains the given value.
    /// </summary>
    /// <param name="item">The <see langword="string"/> item to look for.</param>
    /// <returns>Whether or not this array contains the given item.</returns>
    public bool Contains(string item) => IndexOf(item) != -1;

    /// <summary>
    /// Copies the elements of this <see cref="PackedStringArray"/> to the given
    /// <see langword="string"/> C# array, starting at the given index.
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
    public void CopyTo(string[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        unsafe
        {
            NativeGodotString* ptrw = NativeValue.DangerousSelfRef.GetPtrw();

            int count = Count;
            for (int i = 0; i < count; i++)
            {
                array[arrayIndex] = ptrw[i].ToString();
                arrayIndex++;
            }
        }
    }

    /// <summary>
    /// Returns a copy of the <see cref="PackedStringArray"/>.
    /// </summary>
    /// <returns>A new Packed String Array.</returns>
    public PackedStringArray Duplicate()
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedStringArray newArray = NativeGodotPackedStringArray.Duplicate(ref self);
        return CreateTakingOwnership(newArray);
    }

    /// <summary>
    /// Assigns the given value to all elements in the array. This can typically be
    /// used together with <see cref="Resize(int)"/> to create an array with a given
    /// size and initialized elements.
    /// </summary>
    /// <param name="value">The value to fill the array with.</param>
    public void Fill(string value)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotString valueNative = NativeGodotString.Create(value);
        NativeGodotPackedStringArray.Fill(ref self, valueNative);
    }

    /// <summary>
    /// Searches the array for a value and returns its index or <c>-1</c> if not found.
    /// </summary>
    /// <param name="item">The <see langword="string"/> item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(string item)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotString itemNative = NativeGodotString.Create(item);
        return (int)NativeGodotPackedStringArray.Find(ref self, itemNative);
    }

    /// <summary>
    /// Searches the array for a value and returns its index or <c>-1</c> if not found.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="item">The <see langword="string"/> item to search for.</param>
    /// <param name="index">The initial search index to start from.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(string item, int index)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotString itemNative = NativeGodotString.Create(item);
        return (int)NativeGodotPackedStringArray.Find(ref self, itemNative, index);
    }

    /// <summary>
    /// Searches the array for a value in reverse order and returns its index
    /// or <c>-1</c> if not found.
    /// </summary>
    /// <param name="item">The <see langword="string"/> item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int LastIndexOf(string item)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotString itemNative = NativeGodotString.Create(item);
        return (int)NativeGodotPackedStringArray.Rfind(ref self, itemNative, Count - 1);
    }

    /// <summary>
    /// Searches the array for a value in reverse order and returns its index
    /// or <c>-1</c> if not found.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="item">The <see langword="string"/> item to search for.</param>
    /// <param name="index">The initial search index to start from.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int LastIndexOf(string item, int index)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotString itemNative = NativeGodotString.Create(item);
        return (int)NativeGodotPackedStringArray.Rfind(ref self, itemNative, index);
    }

    /// <summary>
    /// Inserts a new element at a given position in the array. The position
    /// must be valid, or at the end of the array (<c>pos == Count - 1</c>).
    /// Existing items will be moved to the right.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="index">The index to insert at.</param>
    /// <param name="item">The <see langword="string"/> item to insert.</param>
    public void Insert(int index, string item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotString itemNative = NativeGodotString.Create(item);
        NativeGodotPackedStringArray.Insert(ref self, index, itemNative);
    }

    /// <summary>
    /// Removes the first occurrence of the specified <paramref name="item"/>
    /// from this <see cref="PackedStringArray"/>.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    public bool Remove(string item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes an element from the array by index.
    /// To remove an element by searching for its value, use
    /// <see cref="Remove(string)"/> instead.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="index">The index of the element to remove.</param>
    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedStringArray.RemoveAt(ref self, index);
    }

    /// <summary>
    /// Resizes the array to contain a different number of elements. If the array
    /// size is smaller, elements are cleared, if bigger, new elements are <c>0</c>.
    /// </summary>
    /// <param name="newSize">The new size of the array.</param>
    /// <returns><see cref="Error.Ok"/> if successful, or an error code.</returns>
    public Error Resize(int newSize)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        return (Error)NativeGodotPackedStringArray.Resize(ref self, newSize);
    }

    /// <summary>
    /// Reverses the order of the elements in the array.
    /// </summary>
    public void Reverse()
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedStringArray.Reverse(ref self);
    }

    /// <summary>
    /// Creates a copy of a range of elements in the source <see cref="PackedStringArray"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    public PackedStringArray Slice(int start)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, Count);

        return SliceCore(start, Count);
    }

    /// <summary>
    /// Creates a copy of a range of elements in the source <see cref="PackedStringArray"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// -or-
    /// <paramref name="length"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    public PackedStringArray Slice(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, Count);

        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, Count);

        return SliceCore(start, start + length);
    }

    private PackedStringArray SliceCore(int start, int end)
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedStringArray newArray = NativeGodotPackedStringArray.Slice(in self, start, end);
        return CreateTakingOwnership(newArray);
    }

    /// <summary>
    /// Sorts the elements of the array in ascending order.
    /// To sort with a custom predicate use
    /// <see cref="Enumerable.OrderBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>.
    /// </summary>
    public void Sort()
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedStringArray.Sort(ref self);
    }

    // IEnumerable

    /// <summary>
    /// Gets an enumerator for this <see cref="PackedStringArray"/>.
    /// </summary>
    /// <returns>An enumerator.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// Provides an enumerator for the elements of a <see cref="PackedStringArray"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<string>
    {
        private readonly PackedStringArray _array;
        private int _index;
        private string? _current;

        /// <inheritdoc/>
        public readonly string Current => _current!;

        readonly object? IEnumerator.Current => Current;

        internal Enumerator(PackedStringArray array)
        {
            _array = array;
            _index = 0;
            _current = default;
        }

        /// <inheritdoc/>
        public readonly void Dispose() { }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_index < _array.Count)
            {
                _current = _array[_index++];
                return true;
            }

            _index = _array.Count + 1;
            _current = default;
            return false;
        }

        void IEnumerator.Reset()
        {
            _index = 0;
            _current = default;
        }
    }

    private sealed class DebugView
    {
        private readonly PackedStringArray _array;

        public DebugView(PackedStringArray array)
        {
            ArgumentNullException.ThrowIfNull(array);
            _array = array;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public string[] Items => [.. _array];
    }

    /// <summary>
    /// Converts this <see cref="PackedStringArray"/> to a string.
    /// </summary>
    /// <returns>A string representation of this array.</returns>
    public unsafe override string ToString()
    {
        ref NativeGodotPackedStringArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant selfVariant = NativeGodotVariant.CreateFromPackedStringArrayCopying(self);
        using NativeGodotString str = default;
        GodotBridge.GDExtensionInterface.variant_stringify(selfVariant.GetUnsafeAddress(), str.GetUnsafeAddress());
        return str.ToString();
    }

    /// <summary>
    /// Converts this <see cref="PackedStringArray"/> to a C# array.
    /// </summary>
    /// <returns>A C# array representation of this array.</returns>
    public unsafe string[] ToArray()
    {
        return NativeValue.DangerousSelfRef.ToArray();
    }
}

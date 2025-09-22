using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot.Collections;
using Range = System.Range;

/// <summary>
/// Wrapper around Godot's Packed Int64 Array class, and array of 64-bit integers
/// allocated in the engine in C++. Useful when interfacing with the engine.
/// Otherwise prefer .NET collections such as <see cref="Array"/> or
/// <see cref="List{T}"/>.
/// </summary>
[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(PackedArray), nameof(PackedArray.Create))]
public sealed class PackedInt64Array :
    IList<long>,
    IReadOnlyList<long>,
    IDisposable
{
    internal readonly NativeGodotPackedInt64Array.Movable NativeValue;

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    /// <summary>
    /// Constructs a new empty <see cref="PackedInt64Array"/>.
    /// </summary>
    /// <returns>A new Packed Int64 Array.</returns>
    public PackedInt64Array()
    {
        NativeValue = NativeGodotPackedInt64Array.Create().AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    private PackedInt64Array(NativeGodotPackedInt64Array nativeValueToOwn)
    {
        NativeValue = (nativeValueToOwn.IsAllocated
            ? nativeValueToOwn
            : NativeGodotPackedInt64Array.Create()).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt64Array"/> from the given collection's elements.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">The collection of elements to construct from.</param>
    /// <returns>A new Packed Int64 Array.</returns>
    public PackedInt64Array(IEnumerable<long> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);

        // If the collection is another Packed Array, we can add the items
        // with a single interop call.
        if (collection is PackedInt64Array packedArray)
        {
            NativeValue = NativeGodotPackedInt64Array.Duplicate(ref packedArray.NativeValue.DangerousSelfRef).AsMovable();
            return;
        }
        if (collection is long[] array)
        {
            NativeValue = NativeGodotPackedInt64Array.Create(array).AsMovable();
            return;
        }

        NativeValue = NativeGodotPackedInt64Array.Create().AsMovable();

        AddRangeCore(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt64Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Int64 Array.</returns>
    public PackedInt64Array(ReadOnlySpan<long> collection)
    {
        NativeValue = NativeGodotPackedInt64Array.Create(collection).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt64Array"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static PackedInt64Array CreateTakingOwnership(NativeGodotPackedInt64Array nativeValueToOwn)
    {
        return new PackedInt64Array(nativeValueToOwn);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt64Array"/> from the value borrowed from
    /// <paramref name="nativeValueToCopy"/>, copying the value.
    /// Since the new instance is a copy of the value, the caller is responsible
    /// of disposing the new instance to avoid memory leaks.
    /// </summary>
    internal static PackedInt64Array CreateCopying(NativeGodotPackedInt64Array nativeValueToCopy)
    {
        return new PackedInt64Array(NativeGodotPackedInt64Array.Create(nativeValueToCopy));
    }

    /// <summary>
    /// Releases the unmanaged <see cref="PackedInt64Array"/> instance.
    /// </summary>
    ~PackedInt64Array()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="PackedInt64Array"/>.
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
    /// Creates a new span over the array.
    /// </summary>
    /// <returns>The span representation of the array.</returns>
    public Span<long> AsSpan()
    {
        return NativeValue.DangerousSelfRef.AsSpan();
    }

    /// <summary>
    /// Creates a new span over a portion of the array starting at a specified
    /// position to the end of the array.
    /// </summary>
    /// <returns>The span representation of the array.</returns>
    public Span<long> AsSpan(int start)
    {
        return AsSpan().Slice(start);
    }

    /// <summary>
    /// Creates a new span over the portion of the array beginning at a specified
    /// position for a specified length.
    /// </summary>
    /// <returns>The span representation of the array.</returns>
    public Span<long> AsSpan(int start, int length)
    {
        return AsSpan().Slice(start, length);
    }

    /// <summary>
    /// Creates a new span over the portion of the target array defined by an
    /// <see cref="Index"/> value.
    /// </summary>
    /// <returns>The span representation of the array.</returns>
    public Span<long> AsSpan(Index startIndex)
    {
        int actualIndex = startIndex.GetOffset(Count);
        return AsSpan().Slice(actualIndex);
    }

    /// <summary>
    /// Creates a new span over a portion of a target array defined by a
    /// <see cref="Range"/> value.
    /// </summary>
    /// <returns>The span representation of the array.</returns>
    public Span<long> AsSpan(Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(Count);
        return AsSpan().Slice(start, length);
    }

    /// <summary>
    /// Returns the item at the given <paramref name="index"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <value>The <see langword="long"/> item at the given <paramref name="index"/>.</value>
    public long this[int index]
    {
        get => AsSpan()[index];
        set => AsSpan()[index] = value;
    }

    /// <summary>
    /// Returns the number of elements in this <see cref="PackedInt64Array"/>.
    /// This is also known as the size or length of the array.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count => NativeValue.DangerousSelfRef.Size;

    bool ICollection<long>.IsReadOnly => false;

    /// <summary>
    /// Adds an item to the end of this <see cref="PackedInt64Array"/>.
    /// This is the same as <c>append</c> or <c>push_back</c> in GDScript.
    /// </summary>
    /// <param name="item">The <see langword="long"/> item to add.</param>
    public void Add(long item)
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array.Append(ref self, item);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of this <see cref="PackedInt64Array"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">Collection of <see langword="long"/> items to add.</param>
    public void AddRange(IEnumerable<long> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection is PackedInt64Array packedArray)
        {
            // If the collection is another Packed Array, we can add the items
            // with a Span copy instead of iterating each element.
            AddRangeCore(packedArray.AsSpan());
            return;
        }

        if (collection is long[] array)
        {
            // If the collection is a long array, we can add the items
            // with a Span copy instead of iterating each element.
            AddRangeCore(array.AsSpan());
            return;
        }

        AddRangeCore(collection);
    }

    private void AddRangeCore(ReadOnlySpan<long> span)
    {
        int oldCount = Count;
        Resize(Count + span.Length);
        span.CopyTo(AsSpan(oldCount));
        return;
    }

    private void AddRangeCore(IEnumerable<long> collection)
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
    /// <param name="item">The <see langword="long"/> item to look for.</param>
    /// <returns>Whether or not this array contains the given item.</returns>
    public bool Contains(long item) => IndexOf(item) != -1;

    /// <summary>
    /// Copies the elements of this <see cref="PackedInt64Array"/> to the given
    /// <see langword="long"/> C# array, starting at the given index.
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
    public void CopyTo(long[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        AsSpan().CopyTo(array.AsSpan(arrayIndex));
    }

    /// <summary>
    /// Returns a copy of the <see cref="PackedInt64Array"/>.
    /// </summary>
    /// <returns>A new Packed Int64 Array.</returns>
    public PackedInt64Array Duplicate()
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array newArray = NativeGodotPackedInt64Array.Duplicate(ref self);
        return CreateTakingOwnership(newArray);
    }

    /// <summary>
    /// Assigns the given value to all elements in the array. This can typically be
    /// used together with <see cref="Resize(int)"/> to create an array with a given
    /// size and initialized elements.
    /// </summary>
    /// <param name="value">The value to fill the array with.</param>
    public void Fill(long value)
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array.Fill(ref self, value);
    }

    /// <summary>
    /// Searches the array for a value and returns its index or <c>-1</c> if not found.
    /// </summary>
    /// <param name="item">The <see langword="long"/> item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(long item)
    {
        return AsSpan().IndexOf(item);
    }

    /// <summary>
    /// Searches the array for a value and returns its index or <c>-1</c> if not found.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="item">The <see langword="long"/> item to search for.</param>
    /// <param name="index">The initial search index to start from.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(long item, int index)
    {
        return AsSpan(index).IndexOf(item);
    }

    /// <summary>
    /// Searches the array for a value in reverse order and returns its index
    /// or <c>-1</c> if not found.
    /// </summary>
    /// <param name="item">The <see langword="long"/> item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int LastIndexOf(long item)
    {
        return AsSpan().LastIndexOf(item);
    }

    /// <summary>
    /// Searches the array for a value in reverse order and returns its index
    /// or <c>-1</c> if not found.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="item">The <see langword="long"/> item to search for.</param>
    /// <param name="index">The initial search index to start from.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int LastIndexOf(long item, int index)
    {
        return AsSpan(index).LastIndexOf(item);
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
    /// <param name="item">The <see langword="long"/> item to insert.</param>
    public void Insert(int index, long item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array.Insert(ref self, index, item);
    }

    /// <summary>
    /// Removes the first occurrence of the specified <paramref name="item"/>
    /// from this <see cref="PackedInt64Array"/>.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    public bool Remove(long item)
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
    /// <see cref="Remove(long)"/> instead.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="index">The index of the element to remove.</param>
    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array.RemoveAt(ref self, index);
    }

    /// <summary>
    /// Resizes the array to contain a different number of elements. If the array
    /// size is smaller, elements are cleared, if bigger, new elements are <c>0</c>.
    /// </summary>
    /// <param name="newSize">The new size of the array.</param>
    /// <returns><see cref="Error.Ok"/> if successful, or an error code.</returns>
    public Error Resize(int newSize)
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        return (Error)NativeGodotPackedInt64Array.Resize(ref self, newSize);
    }

    /// <summary>
    /// Reverses the order of the elements in the array.
    /// </summary>
    public void Reverse()
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array.Reverse(ref self);
    }

    /// <summary>
    /// Creates a copy of a range of elements in the source <see cref="PackedInt64Array"/>.
    /// Consider using <see cref="AsSpan(int)"/> instead.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    public PackedInt64Array Slice(int start)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, Count);

        return SliceCore(start, Count);
    }

    /// <summary>
    /// Creates a copy of a range of elements in the source <see cref="PackedInt64Array"/>.
    /// Consider using <see cref="AsSpan(int, int)"/> instead.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// -or-
    /// <paramref name="length"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    public PackedInt64Array Slice(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, Count);

        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, Count);

        return SliceCore(start, start + length);
    }

    private PackedInt64Array SliceCore(int start, int end)
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array newArray = NativeGodotPackedInt64Array.Slice(in self, start, end);
        return CreateTakingOwnership(newArray);
    }

    /// <summary>
    /// Sorts the elements of the array in ascending order.
    /// To sort with a custom predicate use
    /// <see cref="Enumerable.OrderBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>.
    /// </summary>
    public void Sort()
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        NativeGodotPackedInt64Array.Sort(ref self);
    }

    // IEnumerable

    /// <summary>
    /// Gets an enumerator for this <see cref="PackedInt64Array"/>.
    /// </summary>
    /// <returns>An enumerator.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<long> IEnumerable<long>.GetEnumerator()
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
    /// Provides an enumerator for the elements of a <see cref="PackedInt64Array"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<long>
    {
        private readonly PackedInt64Array _array;
        private int _index;
        private long _current;

        /// <inheritdoc/>
        public readonly long Current => _current;

        readonly object IEnumerator.Current => Current;

        internal Enumerator(PackedInt64Array array)
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
        private readonly PackedInt64Array _array;

        public DebugView(PackedInt64Array array)
        {
            ArgumentNullException.ThrowIfNull(array);
            _array = array;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public long[] Items => [.. _array];
    }

    /// <summary>
    /// Converts this <see cref="PackedInt64Array"/> to a string.
    /// </summary>
    /// <returns>A string representation of this array.</returns>
    public override unsafe string ToString()
    {
        ref NativeGodotPackedInt64Array self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant selfVariant = NativeGodotVariant.CreateFromPackedInt64ArrayCopying(self);
        using NativeGodotString str = default;
        GodotBridge.GDExtensionInterface.variant_stringify(&selfVariant, &str);
        return str.ToString();
    }
    /// <summary>
    /// Converts this <see cref="PackedInt64Array"/> to a C# array.
    /// Consider using <see cref="AsSpan()"/> instead.
    /// </summary>
    /// <returns>A C# array representation of this array.</returns>
    public unsafe long[] ToArray()
    {
        return AsSpan().ToArray();
    }
}

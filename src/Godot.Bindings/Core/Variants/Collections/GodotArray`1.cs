using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot.NativeInterop;

namespace Godot.Collections;

internal interface IGenericGodotArray
{
    public GodotArray UnderlyingArray { get; }
}

/// <summary>
/// Typed wrapper around Godot's Array class, an array of Variant
/// typed elements allocated in the engine in C++. Useful when
/// interfacing with the engine. Otherwise prefer .NET collections
/// such as arrays or <see cref="List{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the array.</typeparam>
[DebuggerTypeProxy(typeof(GodotArray<>.DebugView))]
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(GodotArray), nameof(GodotArray.Create))]
public sealed class GodotArray<[MustBeVariant] T> :
    IList<T>,
    IReadOnlyList<T>,
    IGenericGodotArray,
    IDisposable
{
    private static unsafe void WriteUnmanagedFunc(in GodotArray<T> value, void* destination) =>
        *(NativeGodotArray*)destination = value.NativeValue.DangerousSelfRef;

    private static unsafe GodotArray<T> ConvertFromUnmanagedFunc(void* ptr) =>
        GodotArray<T>.CreateTakingOwnership(*(NativeGodotArray*)ptr);

    private static NativeGodotVariant ConvertToVariantFunc(in GodotArray<T> from) =>
        from is not null
            ? NativeGodotVariant.CreateFromArrayCopying(from.NativeValue.DangerousSelfRef)
            : default;

    private static GodotArray<T> ConvertFromVariantFunc(in NativeGodotVariant variant) =>
        GodotArray<T>.CreateTakingOwnership(NativeGodotVariant.ConvertToArray(variant));

    static unsafe GodotArray()
    {
        Marshalling.GenericConversion<GodotArray<T>>.AssignToPtrCb = &WriteUnmanagedFunc;
        Marshalling.GenericConversion<GodotArray<T>>.FromPtrCb = &ConvertFromUnmanagedFunc;
        Marshalling.GenericConversion<GodotArray<T>>.ToVariantCb = &ConvertToVariantFunc;
        Marshalling.GenericConversion<GodotArray<T>>.FromVariantCb = &ConvertFromVariantFunc;
    }

    private readonly GodotArray _underlyingArray;

    GodotArray IGenericGodotArray.UnderlyingArray => _underlyingArray;

    internal ref NativeGodotArray.Movable NativeValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _underlyingArray.NativeValue;
    }

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    /// <summary>
    /// Constructs a new empty <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <returns>A new Godot Array.</returns>
    public GodotArray()
    {
        _underlyingArray = [];
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a typed <see cref="GodotArray{T}"/> from an untyped <see cref="GodotArray"/>.
    /// </summary>
    /// <param name="underlyingArray">The untyped array to use as the underlying array.</param>
    /// <returns>A new Godot Array instance with the same underlying array.</returns>
    private GodotArray(GodotArray underlyingArray)
    {
        _underlyingArray = underlyingArray;
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray{T}"/> from the given collection's elements.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">The collection of elements to construct from.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);

        // If the collection is another Godot Array, we can add the items
        // with a single interop call.
        if (collection is GodotArray array)
        {
            _underlyingArray = array.Duplicate(deep: false);
            return;
        }
        if (collection is GodotArray<T> typedArray)
        {
            _underlyingArray = typedArray._underlyingArray.Duplicate(deep: false);
            return;
        }

        _underlyingArray = [];

        AddRangeCore(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray{T}"/> from the given items.
    /// </summary>
    /// <param name="array">The items to put in the new array.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(ReadOnlySpan<T> array)
    {
        _underlyingArray = GodotArray.CreateTakingOwnership(NativeGodotArray.Create(array));
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray{T}"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static GodotArray<T> CreateTakingOwnership(NativeGodotArray nativeValueToOwn)
    {
        return new GodotArray<T>(GodotArray.CreateTakingOwnership(nativeValueToOwn));
    }

    /// <summary>
    /// Converts an untyped <see cref="GodotArray"/> to a typed <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <param name="from">The untyped array to convert.</param>
    /// <returns>
    /// A new Godot Array instance with the same underlying array,
    /// or <see langword="null"/> if <see paramref="from"/> was null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(from))]
    public static explicit operator GodotArray<T>?(GodotArray? from)
    {
        return from is not null ? new GodotArray<T>(from) : null;
    }

    /// <summary>
    /// Converts this typed <see cref="GodotArray{T}"/> to an untyped <see cref="GodotArray"/>.
    /// </summary>
    /// <param name="from">The typed array to convert.</param>
    /// <returns>
    /// The underlying Godot Array instance, or <see langword="null"/> if
    /// <see paramref="from"/> was null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(from))]
    public static explicit operator GodotArray?(GodotArray<T>? from)
    {
        return from?._underlyingArray;
    }

    /// <summary>
    /// Releases the unmanaged <see cref="GodotArray{T}"/> instance.
    /// </summary>
    ~GodotArray()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="GodotArray{T}"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        // Always dispose `_underlyingArray` even if disposing is true.
        _underlyingArray.Dispose();

        if (_weakReferenceToSelf is not null)
        {
            DisposablesTracker.UnregisterDisposable(_weakReferenceToSelf);
        }
    }

    /// <summary>
    /// Concatenates two <see cref="GodotArray{T}"/>s together, with the <paramref name="right"/>
    /// being added to the end of the <see cref="GodotArray{T}"/> specified in <paramref name="left"/>.
    /// For example, <c>[1, 2] + [3, 4]</c> results in <c>[1, 2, 3, 4]</c>.
    /// </summary>
    /// <param name="left">The first array.</param>
    /// <param name="right">The second array.</param>
    /// <returns>A new Godot Array with the contents of both arrays.</returns>
    public static GodotArray<T> operator +(GodotArray<T> left, GodotArray<T> right)
    {
        if (left is null)
        {
            if (right is null)
            {
                return [];
            }

            return right.Duplicate(deep: false);
        }

        if (right is null)
        {
            return left.Duplicate(deep: false);
        }

        return new GodotArray<T>(left._underlyingArray + right._underlyingArray);
    }

    /// <summary>
    /// Returns the item at the given <paramref name="index"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The property is assigned and the array is read-only.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <value>The <see cref="Variant"/> item at the given <paramref name="index"/>.</value>
    public unsafe T this[int index]
    {
        get
        {
            _underlyingArray.GetVariantBorrowElementAt(index, out NativeGodotVariant borrowElem);
            return Marshalling.ConvertFromVariant<T>(borrowElem);
        }
        set
        {
            ThrowIfReadOnly();

            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

            ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
            NativeGodotVariant* ptrw = self.GetPtrw();
            NativeGodotVariant* itemPtr = &ptrw[index];
            itemPtr->Dispose();
            *itemPtr = Marshalling.ConvertToVariant(in value);
        }
    }

    /// <summary>
    /// Returns the number of elements in this <see cref="GodotArray{T}"/>.
    /// This is also known as the size or length of the array.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count => _underlyingArray.Count;

    /// <summary>
    /// Returns <see langword="true"/> if the array is read-only.
    /// See <see cref="MakeReadOnly"/>.
    /// </summary>
    public bool IsReadOnly => _underlyingArray.IsReadOnly;

    /// <summary>
    /// Makes the <see cref="GodotArray{T}"/> read-only, i.e. disabled modying of the
    /// array's elements. Does not apply to nested content, e.g. content of
    /// nested arrays.
    /// </summary>
    public void MakeReadOnly()
    {
        _underlyingArray.MakeReadOnly();
    }

    /// <summary>
    /// Adds an item to the end of this <see cref="GodotArray{T}"/>.
    /// This is the same as <c>append</c> or <c>push_back</c> in GDScript.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="item">The <see cref="Variant"/> item to add.</param>
    public void Add(T item)
    {
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in item);
        NativeGodotArray.Append(ref self, variantValue);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of this <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">Collection of <see cref="Variant"/> items to add.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        ThrowIfReadOnly();
        ArgumentNullException.ThrowIfNull(collection);

        // If the collection is another Godot Array, we can add the items
        // with a single interop call.
        if (collection is GodotArray array)
        {
            ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
            NativeGodotArray collectionNative = array.NativeValue.DangerousSelfRef;
            NativeGodotArray.AppendArray(ref self, collectionNative);
            return;
        }
        if (collection is GodotArray<T> typedArray)
        {
            ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
            NativeGodotArray collectionNative = typedArray._underlyingArray.NativeValue.DangerousSelfRef;
            NativeGodotArray.AppendArray(ref self, collectionNative);
            return;
        }

        AddRangeCore(collection);
    }

    private void AddRangeCore(IEnumerable<T> collection)
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
    /// Finds the index of an existing value using binary search.
    /// If the value is not present in the array, it returns the bitwise
    /// complement of the insertion index that maintains sorting order.
    /// Note: Calling <see cref="BinarySearch(int, int, T)"/> on an unsorted
    /// array results in unexpected behavior.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.
    /// -or-
    /// <paramref name="count"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not denote
    /// a valid range in the <see cref="GodotArray{T}"/>.
    /// </exception>
    /// <param name="index">The starting index of the range to search.</param>
    /// <param name="count">The length of the range to search.</param>
    /// <param name="item">The object to locate.</param>
    /// <returns>
    /// The index of the item in the array, if <paramref name="item"/> is found;
    /// otherwise, a negative number that is the bitwise complement of the index
    /// of the next element that is larger than <paramref name="item"/> or, if
    /// there is no larger element, the bitwise complement of <see cref="Count"/>.
    /// </returns>
    public unsafe int BinarySearch(int index, int count, T item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Count - index);

        if (Count == 0)
        {
            // Special case for empty array to avoid an interop call.
            return -1;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant* ptrw = self.GetPtrw();

        using NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in item);

        int lo = index;
        int hi = index + Count - 1;
        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);

            NativeGodotVariant midItem = ptrw[mid];
            int order = NativeGodotVariant.Compare(midItem, variantValue);

            if (order == 0)
            {
                return mid;
            }
            if (order < 0)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return ~lo;
    }

    /// <summary>
    /// Finds the index of an existing value using binary search.
    /// If the value is not present in the array, it returns the bitwise
    /// complement of the insertion index that maintains sorting order.
    /// Note: Calling <see cref="BinarySearch(T)"/> on an unsorted
    /// array results in unexpected behavior.
    /// </summary>
    /// <param name="item">The object to locate.</param>
    /// <returns>
    /// The index of the item in the array, if <paramref name="item"/> is found;
    /// otherwise, a negative number that is the bitwise complement of the index
    /// of the next element that is larger than <paramref name="item"/> or, if
    /// there is no larger element, the bitwise complement of <see cref="Count"/>.
    /// </returns>
    public int BinarySearch(T item)
    {
        return BinarySearch(0, Count, item);
    }

    /// <summary>
    /// Clears the array. This is the equivalent to using <see cref="Resize(int)"/>
    /// with a size of <c>0</c>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    public void Clear()
    {
        _underlyingArray.Clear();
    }

    /// <summary>
    /// Returns <see langword="true"/> if the array contains the given value.
    /// </summary>
    /// <example>
    /// <code>
    /// GodotArray&lt;string&gt; arr = ["inside", "7"];
    /// GD.Print(arr.Contains("inside")); // True
    /// GD.Print(arr.Contains("outside")); // False
    /// GD.Print(arr.Contains(7)); // False
    /// GD.Print(arr.Contains("7")); // True
    /// </code>
    /// </example>
    /// <param name="item">The item to look for.</param>
    /// <returns>Whether or not this array contains the given item.</returns>
    public bool Contains(T item) => IndexOf(item) != -1;

    /// <summary>
    /// Copies the elements of this <see cref="GodotArray{T}"/> to the given
    /// C# array, starting at the given index.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0 or greater than the array's size.
    /// -or-
    /// The destination array was not big enough.
    /// </exception>
    /// <param name="array">The C# array to copy to.</param>
    /// <param name="arrayIndex">The index to start at.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        int count = Count;
        for (int i = 0; i < count; i++)
        {
            array[arrayIndex] = this[i];
            arrayIndex++;
        }
    }

    /// <summary>
    /// Duplicates this <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <param name="deep">If <see langword="true"/>, performs a deep copy.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray<T> Duplicate(bool deep = false)
    {
        return new GodotArray<T>(_underlyingArray.Duplicate(deep));
    }

    /// <summary>
    /// Assigns the given value to all elements in the array. This can typically be
    /// used together with <see cref="Resize(int)"/> to create an array with a given
    /// size and initialized elements.
    /// Note: If <paramref name="value"/> is of a reference type (<see cref="GodotObject"/>
    /// derived, <see cref="GodotArray"/> or <see cref="GodotDictionary"/>, etc.) then the array
    /// is filled with the references to the same object, i.e. no duplicates are
    /// created.
    /// </summary>
    /// <example>
    /// <code>
    /// var array = new GodotArray&lt;int&gt;();
    /// array.Resize(10);
    /// array.Fill(0); // Initialize the 10 elements to 0.
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="value">The value to fill the array with.</param>
    public void Fill(T value)
    {
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in value);
        NativeGodotArray.Fill(ref self, variantValue);
    }

    /// <summary>
    /// Searches the array for a value and returns its index or <c>-1</c> if not found.
    /// </summary>
    /// <param name="item">The <see cref="Variant"/> item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(T item)
    {
        if (Count == 0)
        {
            // Special case for empty array to avoid an interop call.
            return -1;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in item);
        return (int)NativeGodotArray.Find(in self, variantValue, 0);
    }

    /// <summary>
    /// Searches the array for a value and returns its index or <c>-1</c> if not found.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="item">The <see cref="Variant"/> item to search for.</param>
    /// <param name="index">The initial search index to start from.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(T item, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        if (Count == 0)
        {
            // Special case for empty array to avoid an interop call.
            return -1;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in item);
        return (int)NativeGodotArray.Find(in self, variantValue, index);
    }

    /// <summary>
    /// Searches the array for a value in reverse order and returns its index
    /// or <c>-1</c> if not found.
    /// </summary>
    /// <param name="item">The <see cref="Variant"/> item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int LastIndexOf(Variant item)
    {
        if (Count == 0)
        {
            // Special case for empty array to avoid an interop call.
            return -1;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in item);
        return (int)NativeGodotArray.Rfind(in self, variantValue, Count - 1);
    }

    /// <summary>
    /// Searches the array for a value in reverse order and returns its index
    /// or <c>-1</c> if not found.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="item">The <see cref="Variant"/> item to search for.</param>
    /// <param name="index">The initial search index to start from.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int LastIndexOf(Variant item, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

        if (Count == 0)
        {
            // Special case for empty array to avoid an interop call.
            return -1;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in item);
        return (int)NativeGodotArray.Rfind(in self, variantValue, index);
    }

    /// <summary>
    /// Inserts a new element at a given position in the array. The position
    /// must be valid, or at the end of the array (<c>pos == Count - 1</c>).
    /// Existing items will be moved to the right.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="index">The index to insert at.</param>
    /// <param name="item">The <see cref="Variant"/> item to insert.</param>
    public void Insert(int index, T item)
    {
        ThrowIfReadOnly();

        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        using NativeGodotVariant variantValue = Marshalling.ConvertToVariant(in item);
        _ = NativeGodotArray.Insert(ref self, index, variantValue);
    }

    /// <summary>
    /// Returns the maximum value contained in the array if all elements are of
    /// comparable types. If the elements can't be compared, <see langword="default"/>
    /// is returned.
    /// </summary>
    /// <returns>The maximum value contained in the array.</returns>
    public T Max()
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant resVariant = NativeGodotArray.Max(in self);
        return Marshalling.ConvertFromVariant<T>(resVariant);
    }

    /// <summary>
    /// Returns the minimum value contained in the array if all elements are of
    /// comparable types. If the elements can't be compared, <see langword="default"/>
    /// is returned.
    /// </summary>
    /// <returns>The minimum value contained in the array.</returns>
    public T Min()
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant resVariant = NativeGodotArray.Min(in self);
        return Marshalling.ConvertFromVariant<T>(resVariant);
    }

    /// <summary>
    /// Returns a random value from the target array.
    /// </summary>
    /// <example>
    /// <code>
    /// GodotArray&lt;int&gt; array = [1, 2, 3, 4];
    /// GD.Print(array.PickRandom()); // Prints either of the four numbers.
    /// </code>
    /// </example>
    /// <returns>A random element from the array.</returns>
    public T PickRandom()
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant resVariant = NativeGodotArray.PickRandom(in self);
        return Marshalling.ConvertFromVariant<T>(resVariant);
    }

    /// <summary>
    /// Compares this <see cref="GodotArray{T}"/> against the <paramref name="other"/>
    /// <see cref="GodotArray{T}"/> recursively. Returns <see langword="true"/> if the
    /// sizes and contents of the arrays are equal, <see langword="false"/>
    /// otherwise.
    /// </summary>
    /// <param name="other">The other array to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the sizes and contents of the arrays are equal,
    /// <see langword="false"/> otherwise.
    /// </returns>
    public bool RecursiveEqual(GodotArray<T> other)
    {
        return _underlyingArray.RecursiveEqual(other._underlyingArray);
    }

    /// <summary>
    /// Removes the first occurrence of the specified <paramref name="item"/>
    /// from this <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="item">The value to remove.</param>
    /// <returns>A <see langword="bool"/> indicating success or failure.</returns>
    public bool Remove(T item)
    {
        ThrowIfReadOnly();

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
    /// <see cref="Remove(T)"/> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="index">The index of the element to remove.</param>
    public void RemoveAt(int index)
    {
        _underlyingArray.RemoveAt(index);
    }

    /// <summary>
    /// Resizes this <see cref="GodotArray{T}"/> to the given size.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="newSize">The new size of the array.</param>
    /// <returns><see cref="Error.Ok"/> if successful, or an error code.</returns>
    public Error Resize(int newSize)
    {
        return _underlyingArray.Resize(newSize);
    }

    /// <summary>
    /// Reverses the order of the elements in the array.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    public void Reverse()
    {
        _underlyingArray.Reverse();
    }

    /// <summary>
    /// Shuffles the array such that the items will have a random order.
    /// This method uses the global random number generator common to methods
    /// such as <see cref="GD.Randi"/>. Call <see cref="GD.Randomize"/> to
    /// ensure that a new seed will be used each time if you want
    /// non-reproducible shuffling.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    public void Shuffle()
    {
        _underlyingArray.Shuffle();
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    public GodotArray<T> Slice(int start)
    {
        return SliceCore(start, Count, step: 1, deep: false);
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// -or-
    /// <paramref name="length"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    // The Slice method must have this signature to get implicit Range support.
    public GodotArray<T> Slice(int start, int length)
    {
        return SliceCore(start, start + length, step: 1, deep: false);
    }

    /// <summary>
    /// Returns the slice of the <see cref="GodotArray{T}"/>, from <paramref name="start"/>
    /// (inclusive) to <paramref name="end"/> (exclusive), as a new <see cref="GodotArray{T}"/>.
    /// The absolute value of <paramref name="start"/> and <paramref name="end"/>
    /// will be clamped to the array size.
    /// If either <paramref name="start"/> or <paramref name="end"/> are negative, they
    /// will be relative to the end of the array (i.e. <c>arr.GetSliceRange(0, -2)</c>
    /// is a shorthand for <c>arr.GetSliceRange(0, arr.Count - 2)</c>).
    /// If specified, <paramref name="step"/> is the relative index between source
    /// elements. It can be negative, then <paramref name="start"/> must be higher than
    /// <paramref name="end"/>. For example, <c>[0, 1, 2, 3, 4, 5].GetSliceRange(5, 1, -2)</c>
    /// returns <c>[5, 3]</c>.
    /// If <paramref name="deep"/> is true, each element will be copied by value
    /// rather than by reference.
    /// </summary>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="end">The zero-based index at which the range ends.</param>
    /// <param name="step">The relative index between source elements to take.</param>
    /// <param name="deep">If <see langword="true"/>, performs a deep copy.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    private GodotArray<T> SliceCore(int start, int end, int step = 1, bool deep = false)
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray newArray = NativeGodotArray.Slice(in self, start, end, step, deep);
        return CreateTakingOwnership(newArray);
    }

    /// <summary>
    /// Sorts the array.
    /// Note: The sorting algorithm used is not stable. This means that values
    /// considered equal may have their order changed when using <see cref="Sort"/>.
    /// Note: Strings are sorted in alphabetical order (as opposed to natural order).
    /// This may lead to unexpected behavior when sorting an array of strings ending
    /// with a sequence of numbers.
    /// To sort with a custom predicate use
    /// <see cref="Enumerable.OrderBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// GodotArray&lt;string&gt; strings = ["string1", "string2", "string10", "string11"];
    /// strings.Sort();
    /// GD.Print(strings); // Prints [string1, string10, string11, string2]
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    public void Sort()
    {
        _underlyingArray.Sort();
    }

    // IEnumerable<T>

    /// <summary>
    /// Gets an enumerator for this <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <returns>An enumerator.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        using var enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Provides an enumerator for the elements of a <see cref="GodotArray{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly GodotArray<T> _array;
        private int _index;
        private T? _current;

        /// <inheritdoc/>
        public readonly T Current => _current!;

        readonly object? IEnumerator.Current => Current;

        internal Enumerator(GodotArray<T> array)
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
        private readonly GodotArray<T> _array;

        public DebugView(GodotArray<T> array)
        {
            ArgumentNullException.ThrowIfNull(array);
            _array = array;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => [.. _array];
    }

    /// <summary>
    /// Converts this <see cref="GodotArray{T}"/> to a string.
    /// </summary>
    /// <returns>A string representation of this array.</returns>
    public override string ToString() => _underlyingArray.ToString();

    /// <summary>
    /// Converts this <see cref="GodotArray{T}"/> to a <see cref="Variant"/>.
    /// </summary>
    /// <returns>A Variant that contains this array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(GodotArray<T> from) => Variant.CreateFrom(from);

    /// <summary>
    /// Converts the <see cref="Variant"/> to a <see cref="GodotArray{T}"/>.
    /// </summary>
    /// <returns>The array contained in the Variant.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GodotArray<T>(Variant from) => from.AsGodotArray<T>();

    private void ThrowIfReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException(SR.InvalidOperation_ArrayIsReadOnly);
        }
    }
}

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
/// Wrapper around Godot's Array class, an array of Variant
/// typed elements allocated in the engine in C++. Useful when
/// interfacing with the engine. Otherwise prefer .NET collections
/// such as <see cref="Array"/> or <see cref="List{T}"/>.
/// </summary>
[DebuggerTypeProxy(typeof(DebugView))]
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(GodotArray), nameof(Create))]
public sealed class GodotArray :
    IList<Variant>,
    IReadOnlyList<Variant>,
    IDisposable
{
    internal NativeGodotArray.Movable NativeValue;

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    /// <summary>
    /// Constructs a new empty <see cref="GodotArray"/>.
    /// </summary>
    /// <returns>A new Godot Array.</returns>
    public GodotArray()
    {
        NativeValue = NativeGodotArray.Create().AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    private GodotArray(NativeGodotArray nativeValueToOwn)
    {
        NativeValue = (nativeValueToOwn.IsAllocated
            ? nativeValueToOwn
            : NativeGodotArray.Create()).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given collection's elements.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">The collection of elements to construct from.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(IEnumerable<Variant> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);

        // If the collection is another Godot Array, we can add the items
        // with a single interop call.
        if (collection is GodotArray array)
        {
            NativeValue = NativeGodotArray.Duplicate(ref array.NativeValue.DangerousSelfRef, deep: false).AsMovable();
            return;
        }
        if (collection is GodotArray<Variant> typedArray)
        {
            NativeValue = NativeGodotArray.Duplicate(ref typedArray.NativeValue.DangerousSelfRef, deep: false).AsMovable();
            return;
        }

        NativeValue = NativeGodotArray.Create().AsMovable();

        AddRangeCore(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given Variants.
    /// </summary>
    /// <param name="array">The Variants to put in the new array.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(ReadOnlySpan<Variant> array)
    {
        NativeValue = NativeGodotArray.Create(array).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given StringNames.
    /// </summary>
    /// <param name="array">The StringNames to put in the new array.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(ReadOnlySpan<StringName> array)
    {
        NativeValue = NativeGodotArray.Create(array).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given NodePaths.
    /// </summary>
    /// <param name="array">The NodePaths to put in the new array.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(ReadOnlySpan<NodePath> array)
    {
        NativeValue = NativeGodotArray.Create(array).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given RIDs.
    /// </summary>
    /// <param name="array">The RIDs to put in the new array.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(ReadOnlySpan<Rid> array)
    {
        NativeValue = NativeGodotArray.Create(array).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given objects.
    /// </summary>
    /// <param name="array">The objects to put in the new array.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray(ReadOnlySpan<GodotObject> array)
    {
        NativeValue = NativeGodotArray.Create(array).AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static GodotArray CreateTakingOwnership(NativeGodotArray nativeValueToOwn)
    {
        return new GodotArray(nativeValueToOwn);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Godot Array.</returns>
    public static GodotArray Create(ReadOnlySpan<Variant> span) =>
        new GodotArray(span);

    /// <summary>
    /// Constructs a new <see cref="GodotArray"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Godot Array.</returns>
    public static GodotArray<T> Create<[MustBeVariant] T>(ReadOnlySpan<T> span) =>
        new GodotArray<T>(span);

    /// <summary>
    /// Releases the unmanaged <see cref="GodotArray"/> instance.
    /// </summary>
    ~GodotArray()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="GodotArray"/>.
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
    /// Concatenates two <see cref="GodotArray"/>s together, with the <paramref name="right"/>
    /// being added to the end of the <see cref="GodotArray"/> specified in <paramref name="left"/>.
    /// For example, <c>[1, 2] + [3, 4]</c> results in <c>[1, 2, 3, 4]</c>.
    /// </summary>
    /// <param name="left">The first array.</param>
    /// <param name="right">The second array.</param>
    /// <returns>A new Godot Array with the contents of both arrays.</returns>
    public static GodotArray operator +(GodotArray left, GodotArray right)
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

        int leftCount = left.Count;
        int rightCount = right.Count;

        GodotArray newArray = left.Duplicate(deep: false);
        newArray.Resize(leftCount + rightCount);

        for (int i = 0; i < rightCount; i++)
        {
            newArray[i + leftCount] = right[i];
        }

        return newArray;
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
    public unsafe Variant this[int index]
    {
        get
        {
            GetVariantBorrowElementAt(index, out NativeGodotVariant borrowElem);
            return Variant.CreateCopying(borrowElem);
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
            *itemPtr = NativeGodotVariant.Create(value.NativeValue.DangerousSelfRef);
        }
    }

    /// <summary>
    /// Returns the number of elements in this <see cref="GodotArray"/>.
    /// This is also known as the size or length of the array.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count => NativeValue.DangerousSelfRef.Size;

    /// <summary>
    /// Returns <see langword="true"/> if the array is read-only.
    /// See <see cref="MakeReadOnly"/>.
    /// </summary>
    public bool IsReadOnly => NativeValue.DangerousSelfRef.IsReadOnly;

    /// <summary>
    /// Makes the <see cref="GodotArray"/> read-only, i.e. disabled modying of the
    /// array's elements. Does not apply to nested content, e.g. content of
    /// nested arrays.
    /// </summary>
    public void MakeReadOnly()
    {
        if (IsReadOnly)
        {
            // Avoid interop call when the array is already read-only.
            return;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray.MakeReadOnly(ref self);
    }

    /// <summary>
    /// Adds an item to the end of this <see cref="GodotArray"/>.
    /// This is the same as <c>append</c> or <c>push_back</c> in GDScript.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="item">The <see cref="Variant"/> item to add.</param>
    public void Add(Variant item)
    {
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantValue = item.NativeValue.DangerousSelfRef;
        NativeGodotArray.Append(ref self, variantValue);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of this <see cref="GodotArray"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <param name="collection">Collection of <see cref="Variant"/> items to add.</param>
    public void AddRange<[MustBeVariant] T>(IEnumerable<T> collection)
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
            NativeGodotArray collectionNative = typedArray.NativeValue.DangerousSelfRef;
            NativeGodotArray.AppendArray(ref self, collectionNative);
            return;
        }

        AddRangeCore(collection);
    }

    private void AddRangeCore<[MustBeVariant] T>(IEnumerable<T> collection)
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
                this[oldCount + i] = Variant.From(enumerator.Current);
            }

            return;
        }

        foreach (var item in collection)
        {
            Add(Variant.From(item));
        }
    }

    /// <summary>
    /// Finds the index of an existing value using binary search.
    /// If the value is not present in the array, it returns the bitwise
    /// complement of the insertion index that maintains sorting order.
    /// Note: Calling <see cref="BinarySearch(int, int, Variant)"/> on an
    /// unsorted array results in unexpected behavior.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.
    /// -or-
    /// <paramref name="count"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not denote
    /// a valid range in the <see cref="GodotArray"/>.
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
    public unsafe int BinarySearch(int index, int count, Variant item)
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
        NativeGodotVariant variantValue = item.NativeValue.DangerousSelfRef;

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
    /// Note: Calling <see cref="BinarySearch(Variant)"/> on an unsorted
    /// array results in unexpected behavior.
    /// </summary>
    /// <param name="item">The object to locate.</param>
    /// <returns>
    /// The index of the item in the array, if <paramref name="item"/> is found;
    /// otherwise, a negative number that is the bitwise complement of the index
    /// of the next element that is larger than <paramref name="item"/> or, if
    /// there is no larger element, the bitwise complement of <see cref="Count"/>.
    /// </returns>
    public int BinarySearch(Variant item)
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
    public void Clear() => Resize(0);

    /// <summary>
    /// Returns <see langword="true"/> if the array contains the given value.
    /// </summary>
    /// <example>
    /// <code>
    /// GodotArray arr = ["inside", 7];
    /// GD.Print(arr.Contains("inside")); // True
    /// GD.Print(arr.Contains("outside")); // False
    /// GD.Print(arr.Contains(7)); // True
    /// GD.Print(arr.Contains("7")); // False
    /// </code>
    /// </example>
    /// <param name="item">The <see cref="Variant"/> item to look for.</param>
    /// <returns>Whether or not this array contains the given item.</returns>
    public bool Contains(Variant item) => IndexOf(item) != -1;

    /// <summary>
    /// Copies the elements of this <see cref="GodotArray"/> to the given
    /// <see cref="Variant"/> C# array, starting at the given index.
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
    public void CopyTo(Variant[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length - Count);

        unsafe
        {
            NativeGodotVariant* ptrw = NativeValue.DangerousSelfRef.GetPtrw();

            int count = Count;
            for (int i = 0; i < count; i++)
            {
                array[arrayIndex] = Variant.CreateCopying(ptrw[i]);
                arrayIndex++;
            }
        }
    }

    /// <summary>
    /// Returns a copy of the <see cref="GodotArray"/>.
    /// If <paramref name="deep"/> is <see langword="true"/>, a deep copy if performed:
    /// all nested arrays and dictionaries are duplicated and will not be shared with
    /// the original array. If <see langword="false"/>, a shallow copy is made and
    /// references to the original nested arrays and dictionaries are kept, so that
    /// modifying a sub-array or dictionary in the copy will also impact those
    /// referenced in the source array. Note that any <see cref="GodotObject"/> derived
    /// elements will be shallow copied regardless of the <paramref name="deep"/>
    /// setting.
    /// </summary>
    /// <param name="deep">If <see langword="true"/>, performs a deep copy.</param>
    /// <returns>A new Godot Array.</returns>
    public GodotArray Duplicate(bool deep = false)
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray newArray = NativeGodotArray.Duplicate(in self, deep);
        return CreateTakingOwnership(newArray);
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
    /// var array = new GodotArray();
    /// array.Resize(10);
    /// array.Fill(0); // Initialize the 10 elements to 0.
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="value">The value to fill the array with.</param>
    public void Fill(Variant value)
    {
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantValue = value.NativeValue.DangerousSelfRef;
        NativeGodotArray.Fill(ref self, variantValue);
    }

    /// <summary>
    /// Searches the array for a value and returns its index or <c>-1</c> if not found.
    /// </summary>
    /// <param name="item">The <see cref="Variant"/> item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(Variant item)
    {
        if (Count == 0)
        {
            // Special case for empty array to avoid an interop call.
            return -1;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantValue = item.NativeValue.DangerousSelfRef;
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
    public int IndexOf(Variant item, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        if (Count == 0)
        {
            // Special case for empty array to avoid an interop call.
            return -1;
        }

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantValue = item.NativeValue.DangerousSelfRef;
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
        NativeGodotVariant variantValue = item.NativeValue.DangerousSelfRef;
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
        NativeGodotVariant variantValue = item.NativeValue.DangerousSelfRef;
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
    public void Insert(int index, Variant item)
    {
        ThrowIfReadOnly();

        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant variantValue = item.NativeValue.DangerousSelfRef;
        _ = NativeGodotArray.Insert(ref self, index, variantValue);
    }

    /// <summary>
    /// Returns the maximum value contained in the array if all elements are of
    /// comparable types. If the elements can't be compared, <see langword="null"/>
    /// is returned.
    /// </summary>
    /// <returns>The maximum value contained in the array.</returns>
    public Variant Max()
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant resVariant = NativeGodotArray.Max(in self);
        return Variant.CreateTakingOwnership(resVariant);
    }

    /// <summary>
    /// Returns the minimum value contained in the array if all elements are of
    /// comparable types. If the elements can't be compared, <see langword="null"/>
    /// is returned.
    /// </summary>
    /// <returns>The minimum value contained in the array.</returns>
    public Variant Min()
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant resVariant = NativeGodotArray.Min(in self);
        return Variant.CreateTakingOwnership(resVariant);
    }

    /// <summary>
    /// Returns a random value from the target array.
    /// </summary>
    /// <example>
    /// <code>
    /// GodotArray array = [1, 2, 3, 4];
    /// GD.Print(array.PickRandom()); // Prints either of the four numbers.
    /// </code>
    /// </example>
    /// <returns>A random element from the array.</returns>
    public Variant PickRandom()
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant resVariant = NativeGodotArray.PickRandom(in self);
        return Variant.CreateTakingOwnership(resVariant);
    }

    /// <summary>
    /// Compares this <see cref="GodotArray"/> against the <paramref name="other"/>
    /// <see cref="GodotArray"/> recursively. Returns <see langword="true"/> if the
    /// sizes and contents of the arrays are equal, <see langword="false"/>
    /// otherwise.
    /// </summary>
    /// <param name="other">The other array to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the sizes and contents of the arrays are equal,
    /// <see langword="false"/> otherwise.
    /// </returns>
    public bool RecursiveEqual(GodotArray other)
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray otherVariant = other.NativeValue.DangerousSelfRef;
        return NativeGodotArray.OperatorEqual(self, otherVariant);
    }

    /// <summary>
    /// Removes the first occurrence of the specified <paramref name="item"/>
    /// from this <see cref="GodotArray"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="item">The value to remove.</param>
    public bool Remove(Variant item)
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
    /// <see cref="Remove(Variant)"/> instead.
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
        ThrowIfReadOnly();

        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray.RemoveAt(ref self, index);
    }

    /// <summary>
    /// Resizes the array to contain a different number of elements. If the array
    /// size is smaller, elements are cleared, if bigger, new elements are
    /// <see langword="null"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    /// <param name="newSize">The new size of the array.</param>
    /// <returns><see cref="Error.Ok"/> if successful, or an error code.</returns>
    public Error Resize(int newSize)
    {
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        return (Error)NativeGodotArray.Resize(ref self, newSize);
    }

    /// <summary>
    /// Reverses the order of the elements in the array.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    public void Reverse()
    {
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray.Reverse(ref self);
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
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray.Shuffle(ref self);
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="GodotArray"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    public GodotArray Slice(int start)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, Count);

        return SliceCore(start, Count, step: 1, deep: false);
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="GodotArray"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than 0 or greater than the array's size.
    /// -or-
    /// <paramref name="length"/> is less than 0 or greater than the array's size.
    /// </exception>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    public GodotArray Slice(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, Count);

        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, Count);

        return SliceCore(start, start + length, step: 1, deep: false);
    }

    /// <summary>
    /// Returns the slice of the <see cref="GodotArray"/>, from <paramref name="start"/>
    /// (inclusive) to <paramref name="end"/> (exclusive), as a new <see cref="GodotArray"/>.
    /// The absolute value of <paramref name="start"/> and <paramref name="end"/>
    /// will be clamped to the array size.
    /// If either <paramref name="start"/> or <paramref name="end"/> are negative, they
    /// will be relative to the end of the array (i.e. <c>arr.SliceCore(0, -2)</c>
    /// is a shorthand for <c>arr.SliceCore(0, arr.Count - 2)</c>).
    /// If specified, <paramref name="step"/> is the relative index between source
    /// elements. It can be negative, then <paramref name="start"/> must be higher than
    /// <paramref name="end"/>. For example, <c>[0, 1, 2, 3, 4, 5].SliceCore(5, 1, -2)</c>
    /// returns <c>[5, 3]</c>.
    /// If <paramref name="deep"/> is true, each element will be copied by value
    /// rather than by reference.
    /// </summary>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="end">The zero-based index at which the range ends.</param>
    /// <param name="step">The relative index between source elements to take.</param>
    /// <param name="deep">If <see langword="true"/>, performs a deep copy.</param>
    /// <returns>A new array that contains the elements inside the slice range.</returns>
    private GodotArray SliceCore(int start, int end, int step = 1, bool deep = false)
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
    /// GodotArray strings = ["string1", "string2", "string10", "string11"];
    /// strings.Sort();
    /// GD.Print(strings); // Prints [string1, string10, string11, string2]
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// The array is read-only.
    /// </exception>
    public void Sort()
    {
        ThrowIfReadOnly();

        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotArray.Sort(ref self);
    }

    // IEnumerable

    /// <summary>
    /// Gets an enumerator for this <see cref="GodotArray"/>.
    /// </summary>
    /// <returns>An enumerator.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
    {
        using var enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Provides an enumerator for the elements of a <see cref="GodotArray"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<Variant>
    {
        private readonly GodotArray _array;
        private int _index;
        private Variant _current;

        /// <inheritdoc/>
        public readonly Variant Current => _current;

        readonly object IEnumerator.Current => Current;

        internal Enumerator(GodotArray array)
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
        private readonly GodotArray _array;

        public DebugView(GodotArray array)
        {
            ArgumentNullException.ThrowIfNull(array);
            _array = array;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Variant[] Items => [.. _array];
    }

    /// <summary>
    /// Converts this <see cref="GodotArray"/> to a string.
    /// </summary>
    /// <returns>A string representation of this array.</returns>
    public unsafe override string ToString()
    {
        ref NativeGodotArray self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant selfVariant = new() { Array = self, Type = VariantType.Array };
        using NativeGodotString str = default;
        GodotBridge.GDExtensionInterface.variant_stringify(selfVariant.GetUnsafeAddress(), str.GetUnsafeAddress());
        return str.ToString();
    }

    /// <summary>
    /// The variant returned via the <paramref name="elem"/> parameter is owned by the Array and must not be disposed.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than the array's size.
    /// </exception>
    internal unsafe void GetVariantBorrowElementAt(int index, out NativeGodotVariant elem)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

        NativeGodotVariant* ptrw = NativeValue.DangerousSelfRef.GetPtrw();
        elem = ptrw[index];
    }

    private void ThrowIfReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Array instance is read-only.");
        }
    }
}

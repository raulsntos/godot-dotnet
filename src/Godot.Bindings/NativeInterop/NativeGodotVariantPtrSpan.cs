using System.Runtime.CompilerServices;

namespace Godot.NativeInterop;

/// <summary>
/// Represents a contiguous region of memory that contains pointers to <see cref="NativeGodotVariant"/>,
/// similar to a <c>Span&lt;NativeGodotVariant*&gt;</c>.
/// </summary>
internal unsafe readonly ref struct NativeGodotVariantPtrSpan
{
    private readonly ref NativeGodotVariant* _reference;
    private readonly int _length;

    internal NativeGodotVariantPtrSpan(NativeGodotVariant** pointer, int length)
    {
        _reference = ref *pointer;
        _length = length;
    }

    internal NativeGodotVariantPtrSpan(ref NativeGodotVariant* reference, int length)
    {
        _reference = ref reference;
        _length = length;
    }

    internal ref NativeGodotVariant* GetPinnableReference()
    {
        return ref _reference;
    }

    public ref NativeGodotVariant this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _reference[index];
    }

    /// <summary>
    /// Returns the number of arguments.
    /// </summary>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
    }
}

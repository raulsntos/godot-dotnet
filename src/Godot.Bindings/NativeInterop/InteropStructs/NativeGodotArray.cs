using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

// IMPORTANT:
// A correctly constructed value needs to call the native default constructor to allocate `_p`.
// Don't pass a C# default constructed `NativeGodotArray` to native code, unless it's going to
// be re-assigned a new value (the copy constructor checks if `_p` is null so that's fine).
partial struct NativeGodotArray
{
    [FieldOffset(0)]
    private unsafe ArrayPrivate* _p;

    [StructLayout(LayoutKind.Sequential)]
    private readonly ref struct ArrayPrivate
    {
        private readonly uint _safeRefCount;

        private readonly NativeGodotVector<NativeGodotVariant> _vector;

        private unsafe readonly NativeGodotVariant* _readOnly;

        // There are more fields here, but we don't care as we never store this in C#

        internal readonly int Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _vector.Size;
        }

        internal readonly unsafe bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _readOnly is not null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe readonly NativeGodotVariant* GetPtrw()
        {
            return _vector.GetPtrw();
        }
    }

    internal readonly unsafe bool IsAllocated
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null;
    }

    internal readonly unsafe int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null ? _p->Size : 0;
    }

    internal readonly unsafe bool IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _p is not null && _p->IsReadOnly;
    }

    internal unsafe readonly NativeGodotVariant* GetPtrw()
    {
        return _p->GetPtrw();
    }

    internal unsafe static NativeGodotArray Create<[MustBeVariant] T>(ReadOnlySpan<T> value)
    {
        NativeGodotArray destination = Create();

        Resize(ref destination, value.Length);

        NativeGodotVariant* buffer = destination.GetPtrw();

        for (int i = 0; i < value.Length; i++)
        {
            buffer[i] = Marshalling.ConvertToVariant(value[i]);
        }

        return destination;
    }

    internal unsafe readonly T[] ToArray<[MustBeVariant] T>()
    {
        int size = Size;
        if (size == 0)
        {
            return [];
        }

        NativeGodotVariant* buffer = GetPtrw();

        T[] destination = new T[size];
        for (int i = 0; i < size; i++)
        {
            destination[i] = Marshalling.ConvertFromVariant<T>(buffer[i]);
        }

        return destination;
    }
}

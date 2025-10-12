using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

[StructLayout(LayoutKind.Sequential)]
internal readonly ref struct NativeGodotVector<T> where T : unmanaged, allows ref struct
{
    private readonly nint _writeProxy;

    private readonly unsafe T* _ptr;

    public readonly unsafe int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (typeof(T) == typeof(byte))
            {
                return (int)NativeGodotPackedByteArray.GetSize(in Unsafe.AsRef<NativeGodotPackedByteArray>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(int))
            {
                return (int)NativeGodotPackedInt32Array.GetSize(in Unsafe.AsRef<NativeGodotPackedInt32Array>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(long))
            {
                return (int)NativeGodotPackedInt64Array.GetSize(in Unsafe.AsRef<NativeGodotPackedInt64Array>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(float))
            {
                return (int)NativeGodotPackedFloat32Array.GetSize(in Unsafe.AsRef<NativeGodotPackedFloat32Array>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(double))
            {
                return (int)NativeGodotPackedFloat64Array.GetSize(in Unsafe.AsRef<NativeGodotPackedFloat64Array>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(NativeGodotString))
            {
                return (int)NativeGodotPackedStringArray.GetSize(in Unsafe.AsRef<NativeGodotPackedStringArray>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(Vector2))
            {
                return (int)NativeGodotPackedVector2Array.GetSize(in Unsafe.AsRef<NativeGodotPackedVector2Array>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(Vector3))
            {
                return (int)NativeGodotPackedVector3Array.GetSize(in Unsafe.AsRef<NativeGodotPackedVector3Array>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(Color))
            {
                return (int)NativeGodotPackedColorArray.GetSize(in Unsafe.AsRef<NativeGodotPackedColorArray>((void*)_writeProxy));
            }
            else if (typeof(T) == typeof(Vector4))
            {
                return (int)NativeGodotPackedVector4Array.GetSize(in Unsafe.AsRef<NativeGodotPackedVector4Array>((void*)_writeProxy));
            }
            throw new UnreachableException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly unsafe T* GetPtrw()
    {
        return _ptr;
    }
}

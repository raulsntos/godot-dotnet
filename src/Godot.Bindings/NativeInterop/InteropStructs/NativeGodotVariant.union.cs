using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Godot.NativeInterop;

#pragma warning disable IDE1006 // Naming Styles

partial struct NativeGodotVariant
{
    // VariantType is generated as an enum of type long, so we can't use for the field as it must only take 32-bits.
    [FieldOffset(0)]
    private int _typeField;

    // There's padding here

    [FieldOffset(8)]
    private NativeGodotVariantData _data;

    [StructLayout(LayoutKind.Explicit)]
    private unsafe ref struct NativeGodotVariantData
    {
        [FieldOffset(0)] public bool _bool;
        [FieldOffset(0)] public long _int;
        [FieldOffset(0)] public double _float;
        [FieldOffset(0)] public Transform2D* _transform2d;
        [FieldOffset(0)] public Aabb* _aabb;
        [FieldOffset(0)] public Basis* _basis;
        [FieldOffset(0)] public Transform3D* _transform3d;
        [FieldOffset(0)] public Projection* _projection;
        [FieldOffset(0)] private readonly NativeGodotVariantMem _mem;

        // The following fields are not in the C++ union, but this is how they're stored in _mem.
        [FieldOffset(0)] public NativeGodotStringName _m_string_name;
        [FieldOffset(0)] public NativeGodotString _m_string;
        [FieldOffset(0)] public Vector4 _m_vector4;
        [FieldOffset(0)] public Vector4I _m_vector4i;
        [FieldOffset(0)] public Vector3 _m_vector3;
        [FieldOffset(0)] public Vector3I _m_vector3i;
        [FieldOffset(0)] public Vector2 _m_vector2;
        [FieldOffset(0)] public Vector2I _m_vector2i;
        [FieldOffset(0)] public Rect2 _m_rect2;
        [FieldOffset(0)] public Rect2I _m_rect2i;
        [FieldOffset(0)] public Plane _m_plane;
        [FieldOffset(0)] public Quaternion _m_quaternion;
        [FieldOffset(0)] public Color _m_color;
        [FieldOffset(0)] public NativeGodotNodePath _m_node_path;
        [FieldOffset(0)] public Rid _m_rid;
        [FieldOffset(0)] public NativeGodotObjectData _m_obj_data;
        [FieldOffset(0)] public NativeGodotCallable _m_callable;
        [FieldOffset(0)] public NativeGodotSignal _m_signal;
        [FieldOffset(0)] public NativeGodotDictionary _m_dictionary;
        [FieldOffset(0)] public NativeGodotArray _m_array;

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeGodotObjectData
        {
            public ulong id;
            public nint obj;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct NativeGodotVariantMem
        {
            private readonly real_t _mem0;
            private readonly real_t _mem1;
            private readonly real_t _mem2;
            private readonly real_t _mem3;
        }
    }

    public VariantType Type
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (VariantType)_typeField;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _typeField = (int)value;
    }

    public bool Bool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._bool;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._bool = value;
    }

    public long Int
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._int;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._int = value;
    }

    public double Float
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._float;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._float = value;
    }

    public readonly unsafe Transform2D* Transform2D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data._transform2d;
    }

    public readonly unsafe Aabb* Aabb
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data._aabb;
    }

    public readonly unsafe Basis* Basis
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data._basis;
    }

    public readonly unsafe Transform3D* Transform3D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data._transform3d;
    }

    public readonly unsafe Projection* Projection
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data._projection;
    }

    public NativeGodotStringName StringName
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_string_name;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_string_name = value;
    }

    public NativeGodotString String
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_string;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_string = value;
    }

    public Vector4 Vector4
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_vector4;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_vector4 = value;
    }

    public Vector4I Vector4I
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_vector4i;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_vector4i = value;
    }

    public Vector3 Vector3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_vector3;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_vector3 = value;
    }

    public Vector3I Vector3I
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_vector3i;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_vector3i = value;
    }

    public Vector2 Vector2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_vector2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_vector2 = value;
    }

    public Vector2I Vector2I
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_vector2i;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_vector2i = value;
    }

    public Rect2 Rect2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_rect2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_rect2 = value;
    }

    public Rect2I Rect2I
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_rect2i;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_rect2i = value;
    }

    public Plane Plane
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_plane;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_plane = value;
    }

    public Quaternion Quaternion
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_quaternion;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_quaternion = value;
    }

    public Color Color
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_color;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_color = value;
    }

    public NativeGodotNodePath NodePath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_node_path;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_node_path = value;
    }

    public Rid Rid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_rid;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_rid = value;
    }

    public NativeGodotCallable Callable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_callable;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_callable = value;
    }

    public NativeGodotSignal Signal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_signal;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_signal = value;
    }

    public NativeGodotDictionary Dictionary
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_dictionary;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_dictionary = value;
    }

    public NativeGodotArray Array
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _data._m_array;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data._m_array = value;
    }

    public readonly nint Object
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data._m_obj_data.obj;
    }
}

#pragma warning restore IDE1006 // Naming Styles

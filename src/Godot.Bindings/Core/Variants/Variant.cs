using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Godot.Collections;
using Godot.NativeInterop;
using Godot.NativeInterop.Marshallers;

namespace Godot;

/// <summary>
/// Represents a variety of types that are compatible with Godot APIs.
/// It is often used in APIs where a variety of types are allowed or when
/// the type is dynamic or unspecified.
/// Prefer using specific types or generics when possible, <see cref="Variant"/>
/// are only meant to be used when necessary to interact with untyped engine APIs.
/// </summary>
[SuppressMessage("Design", "CA1001:Type owns disposable field(s) but is not disposable", Justification = "The type does implement IDisposable, this is a bug in the analyzer. See: https://github.com/dotnet/roslyn-analyzers/issues/6151")]
public partial struct Variant : IDisposable
{
    internal NativeGodotVariant.Movable NativeValue;
    private object? _obj;
    private readonly Disposer? _disposer;

    private sealed class Disposer : IDisposable
    {
        private readonly NativeGodotVariant.Movable _native;

        private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

        public Disposer(in NativeGodotVariant.Movable nativeVar)
        {
            _native = nativeVar;
            _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
        }

        ~Disposer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            _native.DangerousSelfRef.Dispose();

            if (_weakReferenceToSelf is not null)
            {
                DisposablesTracker.UnregisterDisposable(_weakReferenceToSelf);
            }
        }
    }

    private Variant(in NativeGodotVariant nativeValue)
    {
        NativeValue = nativeValue.AsMovable();
        _obj = null;

        switch (nativeValue.Type)
        {
            case VariantType.Nil:
            case VariantType.Bool:
            case VariantType.Int:
            case VariantType.Float:
            case VariantType.Vector2:
            case VariantType.Vector2I:
            case VariantType.Rect2:
            case VariantType.Rect2I:
            case VariantType.Vector3:
            case VariantType.Vector3I:
            case VariantType.Vector4:
            case VariantType.Vector4I:
            case VariantType.Plane:
            case VariantType.Quaternion:
            case VariantType.Color:
            case VariantType.Rid:
                _disposer = null;
                break;
            default:
            {
                _disposer = new Disposer(NativeValue);
                break;
            }
        }
    }

    /// <summary>
    /// Constructs a new <see cref="Variant"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static Variant CreateTakingOwnership(in NativeGodotVariant nativeValueToOwn) =>
        new(nativeValueToOwn);

    /// <summary>
    /// Constructs a new <see cref="Variant"/> from the value borrowed from
    /// <paramref name="nativeValueToCopy"/>, copying the value.
    /// Since the new instance is a copy of the value, the caller is responsible
    /// of disposing the new instance to avoid memory leaks.
    /// </summary>
    internal static Variant CreateCopying(in NativeGodotVariant nativeValueToCopy) =>
        new(NativeGodotVariant.Create(nativeValueToCopy));

    /// <summary>
    /// Releases the unmanaged instance associated with this Variant, if any.
    /// </summary>
    public void Dispose()
    {
        _disposer?.Dispose();
        NativeValue = default;
        _obj = null;
    }

    /// <summary>
    /// Enumerates the elements of the variant, if it can be enumerated.
    /// </summary>
    /// <returns>
    /// An enumerator to enumerate the variant's elements.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The variant can't be enumerated.
    /// </exception>
    public readonly VariantEnumerator Enumerate()
    {
        return new VariantEnumerator(this);
    }

    /// <summary>
    /// The type contained in this Variant instance.
    /// </summary>
    public VariantType VariantType => NativeValue.DangerousSelfRef.Type;

    /// <summary>
    /// Converts the value of this Variant to a string representation.
    /// </summary>
    /// <returns>String representation of the Variant's value.</returns>
    public override string ToString() => AsString();

    /// <summary>
    /// Gets the underlying value of the Variant and returns it as an <see cref="object"/>.
    /// If the Variant type is a struct, the value will be boxed.
    /// </summary>
    /// <returns>Value of the variant as an <see cref="object"/>.</returns>
    public object? AsSystemObject() =>
        _obj ??= NativeValue.DangerousSelfRef.Type switch
        {
            VariantType.Nil => null,
            VariantType.Bool => AsBool(),
            VariantType.Int => AsInt64(),
            VariantType.Float => AsDouble(),
            VariantType.String => AsString(),
            VariantType.Vector2 => AsVector2(),
            VariantType.Vector2I => AsVector2I(),
            VariantType.Rect2 => AsRect2(),
            VariantType.Rect2I => AsRect2I(),
            VariantType.Vector3 => AsVector3(),
            VariantType.Vector3I => AsVector3I(),
            VariantType.Transform2D => AsTransform2D(),
            VariantType.Vector4 => AsVector4(),
            VariantType.Vector4I => AsVector4I(),
            VariantType.Plane => AsPlane(),
            VariantType.Quaternion => AsQuaternion(),
            VariantType.Aabb => AsAabb(),
            VariantType.Basis => AsBasis(),
            VariantType.Transform3D => AsTransform3D(),
            VariantType.Projection => AsProjection(),
            VariantType.Color => AsColor(),
            VariantType.StringName => AsStringName(),
            VariantType.NodePath => AsNodePath(),
            VariantType.Rid => AsRid(),
            VariantType.Object => AsGodotObject(),
            VariantType.Callable => AsCallable(),
            VariantType.Signal => AsSignal(),
            VariantType.Dictionary => AsGodotDictionary(),
            VariantType.Array => AsGodotArray(),
            VariantType.PackedByteArray => AsPackedByteArray(),
            VariantType.PackedInt32Array => AsPackedInt32Array(),
            VariantType.PackedInt64Array => AsPackedInt64Array(),
            VariantType.PackedFloat32Array => AsPackedFloat32Array(),
            VariantType.PackedFloat64Array => AsPackedFloat64Array(),
            VariantType.PackedStringArray => AsPackedStringArray(),
            VariantType.PackedVector2Array => AsPackedVector2Array(),
            VariantType.PackedVector3Array => AsPackedVector3Array(),
            VariantType.PackedColorArray => AsPackedColorArray(),
            VariantType.PackedVector4Array => AsPackedVector4Array(),
            _ =>
                throw new InvalidOperationException($"Invalid Variant type: {NativeValue.DangerousSelfRef.Type}"),
        };

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant From<[MustBeVariant] T>(in T from) =>
        CreateTakingOwnership(Marshalling.ConvertToVariant(in from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T As<[MustBeVariant] T>() =>
        Marshalling.ConvertFromVariant<T>(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AsBool() =>
        NativeGodotVariant.ConvertToBool(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char AsChar() =>
        (char)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte AsSByte() =>
        (sbyte)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short AsInt16() =>
        (short)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt32() =>
        (int)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long AsInt64() =>
        NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte AsByte() =>
        (byte)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort AsUInt16() =>
        (ushort)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AsUInt32() =>
        (uint)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong AsUInt64() =>
        (ulong)NativeGodotVariant.ConvertToInt(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float AsSingle() =>
        (float)NativeGodotVariant.ConvertToFloat(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double AsDouble() =>
        NativeGodotVariant.ConvertToFloat(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string AsString()
    {
        using NativeGodotString value = NativeGodotVariant.GetOrConvertToString(NativeValue.DangerousSelfRef);
        return value.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 AsVector2() =>
        NativeGodotVariant.ConvertToVector2(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2I AsVector2I() =>
        NativeGodotVariant.ConvertToVector2I(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect2 AsRect2() =>
        NativeGodotVariant.ConvertToRect2(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect2I AsRect2I() =>
        NativeGodotVariant.ConvertToRect2I(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Transform2D AsTransform2D() =>
        NativeGodotVariant.ConvertToTransform2D(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 AsVector3() =>
        NativeGodotVariant.ConvertToVector3(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3I AsVector3I() =>
        NativeGodotVariant.ConvertToVector3I(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Basis AsBasis() =>
        NativeGodotVariant.ConvertToBasis(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Quaternion AsQuaternion() =>
        NativeGodotVariant.ConvertToQuaternion(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Transform3D AsTransform3D() =>
        NativeGodotVariant.ConvertToTransform3D(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4 AsVector4() =>
        NativeGodotVariant.ConvertToVector4(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4I AsVector4I() =>
        NativeGodotVariant.ConvertToVector4I(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Projection AsProjection() =>
        NativeGodotVariant.ConvertToProjection(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Aabb AsAabb() =>
        NativeGodotVariant.ConvertToAabb(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color AsColor() =>
        NativeGodotVariant.ConvertToColor(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Plane AsPlane() =>
        NativeGodotVariant.ConvertToPlane(NativeValue.DangerousSelfRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Callable AsCallable() =>
        Callable.CreateTakingOwnership(NativeGodotVariant.ConvertToCallable(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Signal AsSignal() =>
        Signal.CreateTakingOwnership(NativeGodotVariant.ConvertToSignal(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedByteArray AsPackedByteArray() =>
        PackedByteArray.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedByteArray(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedInt32Array AsPackedInt32Array() =>
        PackedInt32Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedInt32Array(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedInt64Array AsPackedInt64Array() =>
        PackedInt64Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedInt64Array(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedFloat32Array AsPackedFloat32Array() =>
        PackedFloat32Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedFloat32Array(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedFloat64Array AsPackedFloat64Array() =>
        PackedFloat64Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedFloat64Array(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedStringArray AsPackedStringArray() =>
        PackedStringArray.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedStringArray(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedVector2Array AsPackedVector2Array() =>
        PackedVector2Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedVector2Array(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedVector3Array AsPackedVector3Array() =>
        PackedVector3Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedVector3Array(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedColorArray AsPackedColorArray() =>
        PackedColorArray.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedColorArray(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PackedVector4Array AsPackedVector4Array() =>
        PackedVector4Array.CreateTakingOwnership(NativeGodotVariant.ConvertToPackedVector4Array(NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GodotDictionary<TKey, TValue> AsGodotDictionary<[MustBeVariant] TKey, [MustBeVariant] TValue>()
    {
        return GodotDictionary<TKey, TValue>.CreateTakingOwnership(NativeGodotVariant.ConvertToDictionary(NativeValue.DangerousSelfRef));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GodotArray<T> AsGodotArray<[MustBeVariant] T>()
    {
        return GodotArray<T>.CreateTakingOwnership(NativeGodotVariant.ConvertToArray(NativeValue.DangerousSelfRef));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GodotObject? AsGodotObject()
    {
        nint nativePtr = NativeGodotVariant.ConvertToObject(NativeValue.DangerousSelfRef);
        return GodotObjectMarshaller.GetOrCreateManagedInstance(nativePtr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringName AsStringName()
    {
        return StringName.CreateTakingOwnership(NativeGodotVariant.ConvertToStringName(NativeValue.DangerousSelfRef));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NodePath AsNodePath()
    {
        return NodePath.CreateTakingOwnership(NativeGodotVariant.ConvertToNodePath(NativeValue.DangerousSelfRef));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rid AsRid()
    {
        return NativeGodotVariant.ConvertToRid(NativeValue.DangerousSelfRef);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GodotDictionary AsGodotDictionary()
    {
        return GodotDictionary.CreateTakingOwnership(NativeGodotVariant.ConvertToDictionary(NativeValue.DangerousSelfRef));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GodotArray AsGodotArray()
    {
        return GodotArray.CreateTakingOwnership(NativeGodotVariant.ConvertToArray(NativeValue.DangerousSelfRef));
    }

    // Explicit conversion operators to supported types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator bool(Variant from) => from.AsBool();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator char(Variant from) => from.AsChar();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator sbyte(Variant from) => from.AsSByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator short(Variant from) => from.AsInt16();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator int(Variant from) => from.AsInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator long(Variant from) => from.AsInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator byte(Variant from) => from.AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ushort(Variant from) => from.AsUInt16();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator uint(Variant from) => from.AsUInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(Variant from) => from.AsUInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator float(Variant from) => from.AsSingle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator double(Variant from) => from.AsDouble();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator string(Variant from) => from.AsString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector2(Variant from) => from.AsVector2();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector2I(Variant from) => from.AsVector2I();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Rect2(Variant from) => from.AsRect2();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Rect2I(Variant from) => from.AsRect2I();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Transform2D(Variant from) => from.AsTransform2D();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector3(Variant from) => from.AsVector3();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector3I(Variant from) => from.AsVector3I();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Basis(Variant from) => from.AsBasis();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Quaternion(Variant from) => from.AsQuaternion();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Transform3D(Variant from) => from.AsTransform3D();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector4(Variant from) => from.AsVector4();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector4I(Variant from) => from.AsVector4I();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Projection(Variant from) => from.AsProjection();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Aabb(Variant from) => from.AsAabb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Color(Variant from) => from.AsColor();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Plane(Variant from) => from.AsPlane();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Callable(Variant from) => from.AsCallable();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Signal(Variant from) => from.AsSignal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedByteArray(Variant from) => from.AsPackedByteArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedInt32Array(Variant from) => from.AsPackedInt32Array();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedInt64Array(Variant from) => from.AsPackedInt64Array();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedFloat32Array(Variant from) => from.AsPackedFloat32Array();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedFloat64Array(Variant from) => from.AsPackedFloat64Array();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedStringArray(Variant from) => from.AsPackedStringArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedVector2Array(Variant from) => from.AsPackedVector2Array();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedVector3Array(Variant from) => from.AsPackedVector3Array();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedColorArray(Variant from) => from.AsPackedColorArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator PackedVector4Array(Variant from) => from.AsPackedVector4Array();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GodotObject?(Variant from) => from.AsGodotObject();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator StringName(Variant from) => from.AsStringName();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator NodePath(Variant from) => from.AsNodePath();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Rid(Variant from) => from.AsRid();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GodotDictionary(Variant from) => from.AsGodotDictionary();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GodotArray(Variant from) => from.AsGodotArray();

    // While we provide implicit conversion operators, normal methods are still needed for
    // casts that are not done implicitly (e.g.: enum to integer, etc).

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(bool from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(char from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(sbyte from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(short from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(int from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(long from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(byte from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(ushort from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(uint from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(ulong from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(float from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(double from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(string from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Vector2 from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Vector2I from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Rect2 from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Rect2I from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Transform2D from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Vector3 from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Vector3I from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Basis from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Quaternion from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Transform3D from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Vector4 from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Vector4I from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Projection from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Aabb from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Color from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Plane from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Callable from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Signal from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom<[MustBeVariant] TKey, [MustBeVariant] TValue>(GodotDictionary<TKey, TValue> from)
    {
        NativeGodotVariant variant = from is not null
            ? NativeGodotVariant.CreateFromDictionaryCopying(from.NativeValue.DangerousSelfRef)
            : default;
        return CreateTakingOwnership(variant);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom<[MustBeVariant] T>(GodotArray<T> from)
    {
        NativeGodotVariant variant = from is not null
            ? NativeGodotVariant.CreateFromArrayCopying(from.NativeValue.DangerousSelfRef)
            : default;
        return CreateTakingOwnership(variant);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(GodotObject from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(StringName from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(NodePath from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(Rid from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(GodotDictionary from) => from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Variant CreateFrom(GodotArray from) => from;

    // Implicit conversion operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(bool from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromBool(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(char from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(sbyte from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(short from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(int from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(long from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(byte from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(ushort from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(uint from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(ulong from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromInt((long)from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(float from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromFloat(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(double from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromFloat(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(string from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(from)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Vector2 from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromVector2(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Vector2I from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromVector2I(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Rect2 from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromRect2(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Rect2I from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromRect2I(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Transform2D from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromTransform2D(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Vector3 from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromVector3(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Vector3I from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromVector3I(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Basis from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromBasis(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Quaternion from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromQuaternion(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Transform3D from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromTransform3D(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Vector4 from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromVector4(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Vector4I from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromVector4I(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Projection from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromProjection(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Aabb from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromAabb(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Color from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromColor(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Plane from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPlane(from));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Callable from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromCallableCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Signal from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromSignalCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedByteArray from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedByteArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedInt32Array from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedInt32ArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedInt64Array from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedInt64ArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedFloat32Array from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedFloat32ArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedFloat64Array from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedFloat64ArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedStringArray from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedStringArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedVector2Array from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedVector2ArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedVector3Array from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedVector3ArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedColorArray from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedColorArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(PackedVector4Array from) =>
        CreateTakingOwnership(NativeGodotVariant.CreateFromPackedVector4ArrayCopying(from.NativeValue.DangerousSelfRef));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(GodotObject? from)
    {
        nint nativePtr = GodotObject.GetNativePtr(from);
        return CreateTakingOwnership(NativeGodotVariant.CreateFromObject(nativePtr));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(StringName from)
    {
        return CreateTakingOwnership(from is not null ? NativeGodotVariant.CreateFromStringNameCopying(from.NativeValue.DangerousSelfRef) : default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(NodePath from)
    {
        NativeGodotVariant variantNative = from is not null
            ? NativeGodotVariant.CreateFromNodePathCopying(from.NativeValue.DangerousSelfRef)
            : default;
        return CreateTakingOwnership(variantNative);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(Rid from)
    {
        return CreateTakingOwnership(NativeGodotVariant.CreateFromRid(from));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(GodotDictionary from)
    {
        NativeGodotVariant variantNative = from is not null
            ? NativeGodotVariant.CreateFromDictionaryCopying(from.NativeValue.DangerousSelfRef)
            : default;
        return CreateTakingOwnership(variantNative);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Variant(GodotArray from)
    {
        NativeGodotVariant variantNative = from is not null
            ? NativeGodotVariant.CreateFromArrayCopying(from.NativeValue.DangerousSelfRef)
            : default;
        return CreateTakingOwnership(variantNative);
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

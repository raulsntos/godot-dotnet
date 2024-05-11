using System;

namespace Godot.Collections;

/// <summary>
/// Provides methods for creating packed arrays.
/// </summary>
public static class PackedArray
{
    /// <summary>
    /// Constructs a new <see cref="PackedByteArray"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Byte Array.</returns>
    public static PackedByteArray Create(ReadOnlySpan<byte> span)
    {
        return new PackedByteArray(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt32Array"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Int32 Array.</returns>
    public static PackedInt32Array Create(ReadOnlySpan<int> span)
    {
        return new PackedInt32Array(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt64Array"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Int64 Array.</returns>
    public static PackedInt64Array Create(ReadOnlySpan<long> span)
    {
        return new PackedInt64Array(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedFloat32Array"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Float32 Array.</returns>
    public static PackedFloat32Array Create(ReadOnlySpan<float> span)
    {
        return new PackedFloat32Array(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedFloat64Array"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Float64 Array.</returns>
    public static PackedFloat64Array Create(ReadOnlySpan<double> span)
    {
        return new PackedFloat64Array(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedStringArray"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed String Array.</returns>
    public static PackedStringArray Create(ReadOnlySpan<string> span)
    {
        return new PackedStringArray(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedVector2Array"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Vector2 Array.</returns>
    public static PackedVector2Array Create(ReadOnlySpan<Vector2> span)
    {
        return new PackedVector2Array(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedVector3Array"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Vector3 Array.</returns>
    public static PackedVector3Array Create(ReadOnlySpan<Vector3> span)
    {
        return new PackedVector3Array(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedColorArray"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Color Array.</returns>
    public static PackedColorArray Create(ReadOnlySpan<Color> span)
    {
        return new PackedColorArray(span);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedVector4Array"/> from the given span.
    /// </summary>
    /// <param name="span">The elements to construct from.</param>
    /// <returns>A new Packed Vector3 Array.</returns>
    public static PackedVector4Array Create(ReadOnlySpan<Vector4> span)
    {
        return new PackedVector4Array(span);
    }
}

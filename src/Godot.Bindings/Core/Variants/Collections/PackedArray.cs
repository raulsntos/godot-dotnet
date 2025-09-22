using System;

namespace Godot.Collections;

/// <summary>
/// Provides methods for creating packed arrays.
/// </summary>
public static class PackedArray
{
    /// <summary>
    /// Constructs a new <see cref="PackedByteArray"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Byte Array.</returns>
    public static PackedByteArray Create(ReadOnlySpan<byte> collection)
    {
        return new PackedByteArray(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt32Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Int32 Array.</returns>
    public static PackedInt32Array Create(ReadOnlySpan<int> collection)
    {
        return new PackedInt32Array(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedInt64Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Int64 Array.</returns>
    public static PackedInt64Array Create(ReadOnlySpan<long> collection)
    {
        return new PackedInt64Array(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedFloat32Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Float32 Array.</returns>
    public static PackedFloat32Array Create(ReadOnlySpan<float> collection)
    {
        return new PackedFloat32Array(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedFloat64Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Float64 Array.</returns>
    public static PackedFloat64Array Create(ReadOnlySpan<double> collection)
    {
        return new PackedFloat64Array(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedStringArray"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed String Array.</returns>
    public static PackedStringArray Create(ReadOnlySpan<string> collection)
    {
        return new PackedStringArray(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedVector2Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Vector2 Array.</returns>
    public static PackedVector2Array Create(ReadOnlySpan<Vector2> collection)
    {
        return new PackedVector2Array(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedVector3Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Vector3 Array.</returns>
    public static PackedVector3Array Create(ReadOnlySpan<Vector3> collection)
    {
        return new PackedVector3Array(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedColorArray"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Color Array.</returns>
    public static PackedColorArray Create(ReadOnlySpan<Color> collection)
    {
        return new PackedColorArray(collection);
    }

    /// <summary>
    /// Constructs a new <see cref="PackedVector4Array"/> from the given collection's elements.
    /// </summary>
    /// <param name="collection">The elements to construct from.</param>
    /// <returns>A new Packed Vector3 Array.</returns>
    public static PackedVector4Array Create(ReadOnlySpan<Vector4> collection)
    {
        return new PackedVector4Array(collection);
    }
}

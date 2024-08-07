using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Godot;

/// <summary>
/// 4-element structure that can be used to represent 4D grid coordinates or sets of integers.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vector4I : IEquatable<Vector4I>
{
    /// <summary>
    /// Enumerated index values for the axes.
    /// Returned by <see cref="MaxAxisIndex"/> and <see cref="MinAxisIndex"/>.
    /// </summary>
    public enum Axis
    {
        /// <summary>
        /// The vector's X axis.
        /// </summary>
        X,
        /// <summary>
        /// The vector's Y axis.
        /// </summary>
        Y,
        /// <summary>
        /// The vector's Z axis.
        /// </summary>
        Z,
        /// <summary>
        /// The vector's W axis.
        /// </summary>
        W,
    }

    /// <summary>
    /// The vector's X component. Also accessible by using the index position <c>[0]</c>.
    /// </summary>
    public int X;

    /// <summary>
    /// The vector's Y component. Also accessible by using the index position <c>[1]</c>.
    /// </summary>
    public int Y;

    /// <summary>
    /// The vector's Z component. Also accessible by using the index position <c>[2]</c>.
    /// </summary>
    public int Z;

    /// <summary>
    /// The vector's W component. Also accessible by using the index position <c>[3]</c>.
    /// </summary>
    public int W;

    /// <summary>
    /// Access vector components using their <paramref name="index"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not 0, 1, 2 or 3.
    /// </exception>
    /// <value>
    /// <c>[0]</c> is equivalent to <see cref="X"/>,
    /// <c>[1]</c> is equivalent to <see cref="Y"/>,
    /// <c>[2]</c> is equivalent to <see cref="Z"/>.
    /// <c>[3]</c> is equivalent to <see cref="W"/>.
    /// </value>
    public int this[int index]
    {
        readonly get
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
        set
        {
            switch (index)
            {
                case 0:
                    X = value;
                    return;
                case 1:
                    Y = value;
                    return;
                case 2:
                    Z = value;
                    return;
                case 3:
                    W = value;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    /// <summary>
    /// Helper method for deconstruction into a tuple.
    /// </summary>
    public readonly void Deconstruct(out int x, out int y, out int z, out int w)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
    }

    /// <summary>
    /// Returns a new vector with all components in absolute values (i.e. positive).
    /// </summary>
    /// <returns>A vector with <see cref="int.Abs(int)"/> called on each component.</returns>
    public readonly Vector4I Abs()
    {
        return new Vector4I
        (
            int.Abs(X),
            int.Abs(Y),
            int.Abs(Z),
            int.Abs(W)
        );
    }

    /// <summary>
    /// Returns a new vector with all components clamped between the
    /// components of <paramref name="min"/> and <paramref name="max"/> using
    /// <see cref="int.Clamp(int, int, int)"/>.
    /// </summary>
    /// <param name="min">The vector with minimum allowed values.</param>
    /// <param name="max">The vector with maximum allowed values.</param>
    /// <returns>The vector with all components clamped.</returns>
    public readonly Vector4I Clamp(Vector4I min, Vector4I max)
    {
        return new Vector4I
        (
            int.Clamp(X, min.X, max.X),
            int.Clamp(Y, min.Y, max.Y),
            int.Clamp(Z, min.Z, max.Z),
            int.Clamp(W, min.W, max.W)
        );
    }

    /// <summary>
    /// Returns a new vector with all components clamped between
    /// <paramref name="min"/> and <paramref name="max"/> using
    /// <see cref="int.Clamp(int, int, int)"/>.
    /// </summary>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The vector with all components clamped.</returns>
    public readonly Vector4I Clamp(int min, int max)
    {
        return new Vector4I
        (
            int.Clamp(X, min, max),
            int.Clamp(Y, min, max),
            int.Clamp(Z, min, max),
            int.Clamp(W, min, max)
        );
    }

    /// <summary>
    /// Returns the squared distance between this vector and <paramref name="to"/>.
    /// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
    /// you need to compare vectors or need the squared distance for some formula.
    /// </summary>
    /// <param name="to">The other vector to use.</param>
    /// <returns>The squared distance between the two vectors.</returns>
    public readonly int DistanceSquaredTo(Vector4I to)
    {
        return (to - this).LengthSquared();
    }

    /// <summary>
    /// Returns the distance between this vector and <paramref name="to"/>.
    /// </summary>
    /// <seealso cref="DistanceSquaredTo(Vector4I)"/>
    /// <param name="to">The other vector to use.</param>
    /// <returns>The distance between the two vectors.</returns>
    public readonly real_t DistanceTo(Vector4I to)
    {
        return (to - this).Length();
    }

    /// <summary>
    /// Returns the length (magnitude) of this vector.
    /// </summary>
    /// <seealso cref="LengthSquared"/>
    /// <returns>The length of this vector.</returns>
    public readonly real_t Length()
    {
        int x2 = X * X;
        int y2 = Y * Y;
        int z2 = Z * Z;
        int w2 = W * W;

        return real_t.Sqrt(x2 + y2 + z2 + w2);
    }

    /// <summary>
    /// Returns the squared length (squared magnitude) of this vector.
    /// This method runs faster than <see cref="Length"/>, so prefer it if
    /// you need to compare vectors or need the squared length for some formula.
    /// </summary>
    /// <returns>The squared length of this vector.</returns>
    public readonly int LengthSquared()
    {
        int x2 = X * X;
        int y2 = Y * Y;
        int z2 = Z * Z;
        int w2 = W * W;

        return x2 + y2 + z2 + w2;
    }

    /// <summary>
    /// Returns the result of the component-wise maximum between this vector
    /// and <paramref name="with"/> using <see cref="int.Max(int, int)"/>.
    /// </summary>
    /// <param name="with">The other vector to use.</param>
    /// <returns>The resulting maximum vector.</returns>
    public readonly Vector4I Max(Vector4I with)
    {
        return new Vector4I
        (
            int.Max(X, with.X),
            int.Max(Y, with.Y),
            int.Max(Z, with.Z),
            int.Max(W, with.W)
        );
    }

    /// <summary>
    /// Returns the result of the component-wise maximum between this vector
    /// and <paramref name="with"/> using <see cref="int.Max(int, int)"/>.
    /// </summary>
    /// <param name="with">The other value to use.</param>
    /// <returns>The resulting maximum vector.</returns>
    public readonly Vector4I Max(int with)
    {
        return new Vector4I
        (
            int.Max(X, with),
            int.Max(Y, with),
            int.Max(Z, with),
            int.Max(W, with)
        );
    }

    /// <summary>
    /// Returns the result of the component-wise minimum between this vector
    /// and <paramref name="with"/> using <see cref="int.Min(int, int)"/>.
    /// </summary>
    /// <param name="with">The other vector to use.</param>
    /// <returns>The resulting minimum vector.</returns>
    public readonly Vector4I Min(Vector4I with)
    {
        return new Vector4I
        (
            int.Min(X, with.X),
            int.Min(Y, with.Y),
            int.Min(Z, with.Z),
            int.Min(W, with.W)
        );
    }

    /// <summary>
    /// Returns the result of the component-wise minimum between this vector
    /// and <paramref name="with"/> using <see cref="int.Min(int, int)"/>.
    /// </summary>
    /// <param name="with">The other value to use.</param>
    /// <returns>The resulting minimum vector.</returns>
    public readonly Vector4I Min(int with)
    {
        return new Vector4I
        (
            int.Min(X, with),
            int.Min(Y, with),
            int.Min(Z, with),
            int.Min(W, with)
        );
    }

    /// <summary>
    /// Returns the axis of the vector's highest value. See <see cref="Axis"/>.
    /// If all components are equal, this method returns <see cref="Axis.X"/>.
    /// </summary>
    /// <returns>The index of the highest axis.</returns>
    public readonly Axis MaxAxisIndex()
    {
        int max_index = 0;
        int max_value = X;
        for (int i = 1; i < 4; i++)
        {
            if (this[i] > max_value)
            {
                max_index = i;
                max_value = this[i];
            }
        }
        return (Axis)max_index;
    }

    /// <summary>
    /// Returns the axis of the vector's lowest value. See <see cref="Axis"/>.
    /// If all components are equal, this method returns <see cref="Axis.W"/>.
    /// </summary>
    /// <returns>The index of the lowest axis.</returns>
    public readonly Axis MinAxisIndex()
    {
        int min_index = 0;
        int min_value = X;
        for (int i = 1; i < 4; i++)
        {
            if (this[i] <= min_value)
            {
                min_index = i;
                min_value = this[i];
            }
        }
        return (Axis)min_index;
    }

    /// <summary>
    /// Returns a vector with each component set to one or negative one, depending
    /// on the signs of this vector's components, or zero if the component is zero,
    /// by calling <see cref="int.Sign(int)"/> on each component.
    /// </summary>
    /// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
    public readonly Vector4I Sign()
    {
        return new Vector4I
        (
            int.Sign(X),
            int.Sign(Y),
            int.Sign(Z),
            int.Sign(W)
        );
    }

    /// <summary>
    /// Returns a new vector with each component snapped to the closest multiple of the corresponding component
    /// in <paramref name="step"/>.
    /// </summary>
    /// <param name="step">A vector value representing the step size to snap to.</param>
    /// <returns>The snapped vector.</returns>
    public readonly Vector4I Snapped(Vector4I step)
    {
        return new Vector4I
        (
            (int)Mathf.Snapped(X, step.X),
            (int)Mathf.Snapped(Y, step.Y),
            (int)Mathf.Snapped(Z, step.Z),
            (int)Mathf.Snapped(W, step.W)
        );
    }

    /// <summary>
    /// Returns a new vector with each component snapped to the closest multiple of <paramref name="step"/>.
    /// </summary>
    /// <param name="step">The step size to snap to.</param>
    /// <returns>The snapped vector.</returns>
    public readonly Vector4I Snapped(int step)
    {
        return new Vector4I
        (
            (int)Mathf.Snapped(X, step),
            (int)Mathf.Snapped(Y, step),
            (int)Mathf.Snapped(Z, step),
            (int)Mathf.Snapped(W, step)
        );
    }

    /// <summary>
    /// Min vector, a vector with all components equal to <see cref="int.MinValue"/>. Can be used as a negative integer equivalent of <see cref="Vector4.Inf"/>.
    /// </summary>
    /// <value>Equivalent to <c>new Vector4I(int.MinValue, int.MinValue, int.MinValue, int.MinValue)</c>.</value>
    public static Vector4I MinValue => new Vector4I(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
    /// <summary>
    /// Max vector, a vector with all components equal to <see cref="int.MaxValue"/>. Can be used as an integer equivalent of <see cref="Vector4.Inf"/>.
    /// </summary>
    /// <value>Equivalent to <c>new Vector4I(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)</c>.</value>
    public static Vector4I MaxValue => new Vector4I(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

    /// <summary>
    /// Zero vector, a vector with all components set to <c>0</c>.
    /// </summary>
    /// <value>Equivalent to <c>new Vector4I(0, 0, 0, 0)</c>.</value>
    public static Vector4I Zero => new Vector4I(0, 0, 0, 0);
    /// <summary>
    /// One vector, a vector with all components set to <c>1</c>.
    /// </summary>
    /// <value>Equivalent to <c>new Vector4I(1, 1, 1, 1)</c>.</value>
    public static Vector4I One => new Vector4I(1, 1, 1, 1);

    /// <summary>
    /// Constructs a new <see cref="Vector4I"/> with the given components.
    /// </summary>
    /// <param name="x">The vector's X component.</param>
    /// <param name="y">The vector's Y component.</param>
    /// <param name="z">The vector's Z component.</param>
    /// <param name="w">The vector's W component.</param>
    public Vector4I(int x, int y, int z, int w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Adds each component of the <see cref="Vector4I"/>
    /// with the components of the given <see cref="Vector4I"/>.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>The added vector.</returns>
    public static Vector4I operator +(Vector4I left, Vector4I right)
    {
        return new Vector4I
        (
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z,
            left.W + right.W
        );
    }

    /// <summary>
    /// Subtracts each component of the <see cref="Vector4I"/>
    /// by the components of the given <see cref="Vector4I"/>.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>The subtracted vector.</returns>
    public static Vector4I operator -(Vector4I left, Vector4I right)
    {
        return new Vector4I
        (
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z,
            left.W - right.W
        );
    }

    /// <summary>
    /// Returns the negative value of the <see cref="Vector4I"/>.
    /// This is the same as writing <c>new Vector4I(-v.X, -v.Y, -v.Z, -v.W)</c>.
    /// This operation flips the direction of the vector while
    /// keeping the same magnitude.
    /// </summary>
    /// <param name="vec">The vector to negate/flip.</param>
    /// <returns>The negated/flipped vector.</returns>
    public static Vector4I operator -(Vector4I vec)
    {
        return new Vector4I
        (
            -vec.X,
            -vec.Y,
            -vec.Z,
            -vec.W
        );
    }

    /// <summary>
    /// Multiplies each component of the <see cref="Vector4I"/>
    /// by the given <see langword="int"/>.
    /// </summary>
    /// <param name="vec">The vector to multiply.</param>
    /// <param name="scale">The scale to multiply by.</param>
    /// <returns>The multiplied vector.</returns>
    public static Vector4I operator *(Vector4I vec, int scale)
    {
        return new Vector4I
        (
            vec.X * scale,
            vec.Y * scale,
            vec.Z * scale,
            vec.W * scale
        );
    }

    /// <summary>
    /// Multiplies each component of the <see cref="Vector4I"/>
    /// by the given <see langword="int"/>.
    /// </summary>
    /// <param name="scale">The scale to multiply by.</param>
    /// <param name="vec">The vector to multiply.</param>
    /// <returns>The multiplied vector.</returns>
    public static Vector4I operator *(int scale, Vector4I vec)
    {
        return new Vector4I
        (
            scale * vec.X,
            scale * vec.Y,
            scale * vec.Z,
            scale * vec.W
        );
    }

    /// <summary>
    /// Multiplies each component of the <see cref="Vector4I"/>
    /// by the components of the given <see cref="Vector4I"/>.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>The multiplied vector.</returns>
    public static Vector4I operator *(Vector4I left, Vector4I right)
    {
        return new Vector4I
        (
            left.X * right.X,
            left.Y * right.Y,
            left.Z * right.Z,
            left.W * right.W
        );
    }

    /// <summary>
    /// Divides each component of the <see cref="Vector4I"/>
    /// by the given <see langword="int"/>.
    /// </summary>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisor">The divisor value.</param>
    /// <returns>The divided vector.</returns>
    public static Vector4I operator /(Vector4I vec, int divisor)
    {
        return new Vector4I
        (
            vec.X / divisor,
            vec.Y / divisor,
            vec.Z / divisor,
            vec.W / divisor
        );
    }

    /// <summary>
    /// Divides each component of the <see cref="Vector4I"/>
    /// by the components of the given <see cref="Vector4I"/>.
    /// </summary>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisorv">The divisor vector.</param>
    /// <returns>The divided vector.</returns>
    public static Vector4I operator /(Vector4I vec, Vector4I divisorv)
    {
        return new Vector4I
        (
            vec.X / divisorv.X,
            vec.Y / divisorv.Y,
            vec.Z / divisorv.Z,
            vec.W / divisorv.W
        );
    }

    /// <summary>
    /// Gets the remainder of each component of the <see cref="Vector4I"/>
    /// with the components of the given <see langword="int"/>.
    /// This operation uses truncated division, which is often not desired
    /// as it does not work well with negative numbers.
    /// Consider using <see cref="Mathf.PosMod(int, int)"/> instead
    /// if you want to handle negative numbers.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.Print(new Vector4I(10, -20, 30, -40) % 7); // Prints "(3, -6, 2, -5)"
    /// </code>
    /// </example>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisor">The divisor value.</param>
    /// <returns>The remainder vector.</returns>
    public static Vector4I operator %(Vector4I vec, int divisor)
    {
        return new Vector4I
        (
            vec.X % divisor,
            vec.Y % divisor,
            vec.Z % divisor,
            vec.W % divisor
        );
    }

    /// <summary>
    /// Gets the remainder of each component of the <see cref="Vector4I"/>
    /// with the components of the given <see cref="Vector4I"/>.
    /// This operation uses truncated division, which is often not desired
    /// as it does not work well with negative numbers.
    /// Consider using <see cref="Mathf.PosMod(int, int)"/> instead
    /// if you want to handle negative numbers.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.Print(new Vector4I(10, -20, 30, -40) % new Vector4I(6, 7, 8, 9)); // Prints "(4, -6, 6, -4)"
    /// </code>
    /// </example>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisorv">The divisor vector.</param>
    /// <returns>The remainder vector.</returns>
    public static Vector4I operator %(Vector4I vec, Vector4I divisorv)
    {
        return new Vector4I
        (
            vec.X % divisorv.X,
            vec.Y % divisorv.Y,
            vec.Z % divisorv.Z,
            vec.W % divisorv.W
        );
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vectors are equal.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the vectors are equal.</returns>
    public static bool operator ==(Vector4I left, Vector4I right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vectors are not equal.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the vectors are not equal.</returns>
    public static bool operator !=(Vector4I left, Vector4I right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Compares two <see cref="Vector4I"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is less than
    /// the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y, Z and finally W values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is less than the right.</returns>
    public static bool operator <(Vector4I left, Vector4I right)
    {
        if (left.X == right.X)
        {
            if (left.Y == right.Y)
            {
                if (left.Z == right.Z)
                {
                    return left.W < right.W;
                }
                return left.Z < right.Z;
            }
            return left.Y < right.Y;
        }
        return left.X < right.X;
    }

    /// <summary>
    /// Compares two <see cref="Vector4I"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is greater than
    /// the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y, Z and finally W values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is greater than the right.</returns>
    public static bool operator >(Vector4I left, Vector4I right)
    {
        if (left.X == right.X)
        {
            if (left.Y == right.Y)
            {
                if (left.Z == right.Z)
                {
                    return left.W > right.W;
                }
                return left.Z > right.Z;
            }
            return left.Y > right.Y;
        }
        return left.X > right.X;
    }

    /// <summary>
    /// Compares two <see cref="Vector4I"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is less than
    /// or equal to the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y, Z and finally W values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is less than or equal to the right.</returns>
    public static bool operator <=(Vector4I left, Vector4I right)
    {
        if (left.X == right.X)
        {
            if (left.Y == right.Y)
            {
                if (left.Z == right.Z)
                {
                    return left.W <= right.W;
                }
                return left.Z < right.Z;
            }
            return left.Y < right.Y;
        }
        return left.X < right.X;
    }

    /// <summary>
    /// Compares two <see cref="Vector4I"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is greater than
    /// or equal to the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y, Z and finally W values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is greater than or equal to the right.</returns>
    public static bool operator >=(Vector4I left, Vector4I right)
    {
        if (left.X == right.X)
        {
            if (left.Y == right.Y)
            {
                if (left.Z == right.Z)
                {
                    return left.W >= right.W;
                }
                return left.Z > right.Z;
            }
            return left.Y > right.Y;
        }
        return left.X > right.X;
    }

    /// <summary>
    /// Converts this <see cref="Vector4I"/> to a <see cref="Vector4"/>.
    /// </summary>
    /// <param name="value">The vector to convert.</param>
    public static implicit operator Vector4(Vector4I value)
    {
        return new Vector4
        (
            value.X,
            value.Y,
            value.Z,
            value.W
        );
    }

    /// <summary>
    /// Converts a <see cref="Vector4"/> to a <see cref="Vector4I"/> by truncating
    /// components' fractional parts (rounding towards zero). For a different
    /// behavior consider passing the result of <see cref="Vector4.Ceil"/>,
    /// <see cref="Vector4.Floor"/> or <see cref="Vector4.Round"/> to this conversion operator instead.
    /// </summary>
    /// <param name="value">The vector to convert.</param>
    public static explicit operator Vector4I(Vector4 value)
    {
        return new Vector4I
        (
            (int)value.X,
            (int)value.Y,
            (int)value.Z,
            (int)value.W
        );
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vector is equal
    /// to the given object (<paramref name="obj"/>).
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>Whether or not the vector and the object are equal.</returns>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Vector4I other && Equals(other);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vectors are equal.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>Whether or not the vectors are equal.</returns>
    public readonly bool Equals(Vector4I other)
    {
        return X == other.X
            && Y == other.Y
            && Z == other.Z
            && W == other.W;
    }

    /// <summary>
    /// Serves as the hash function for <see cref="Vector4I"/>.
    /// </summary>
    /// <returns>A hash code for this vector.</returns>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    /// <summary>
    /// Converts this <see cref="Vector4I"/> to a string.
    /// </summary>
    /// <returns>A string representation of this vector.</returns>
    public override readonly string ToString() => ToString(null);

    /// <summary>
    /// Converts this <see cref="Vector4I"/> to a string with the given <paramref name="format"/>.
    /// </summary>
    /// <returns>A string representation of this vector.</returns>
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return $"({X.ToString(format, CultureInfo.InvariantCulture)}, {Y.ToString(format, CultureInfo.InvariantCulture)}, {Z.ToString(format, CultureInfo.InvariantCulture)}), {W.ToString(format, CultureInfo.InvariantCulture)})";
    }
}

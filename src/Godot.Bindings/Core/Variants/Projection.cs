using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Godot;

/// <summary>
/// A 4x4 matrix used for 3D projective transformations. It can represent transformations such as
/// translation, rotation, scaling, shearing, and perspective division. It consists of four
/// <see cref="Vector4"/> columns.
/// For purely linear transformations (translation, rotation, and scale), it is recommended to use
/// <see cref="Transform3D"/>, as it is more performant and has a lower memory footprint.
/// Used internally as <see cref="Camera3D"/>'s projection matrix.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Projection : IEquatable<Projection>
{
    /// <summary>
    /// Enumerated index values for the planes.
    /// </summary>
    public enum Planes
    {
        /// <summary>
        /// The projection's near plane.
        /// </summary>
        Near,
        /// <summary>
        /// The projection's far plane.
        /// </summary>
        Far,
        /// <summary>
        /// The projection's left plane.
        /// </summary>
        Left,
        /// <summary>
        /// The projection's top plane.
        /// </summary>
        Top,
        /// <summary>
        /// The projection's right plane.
        /// </summary>
        Right,
        /// <summary>
        /// The projection's bottom plane.
        /// </summary>
        Bottom,
    }

    /// <summary>
    /// The projection's X column. Also accessible by using the index position <c>[0]</c>.
    /// </summary>
    public Vector4 X;

    /// <summary>
    /// The projection's Y column. Also accessible by using the index position <c>[1]</c>.
    /// </summary>
    public Vector4 Y;

    /// <summary>
    /// The projection's Z column. Also accessible by using the index position <c>[2]</c>.
    /// </summary>
    public Vector4 Z;

    /// <summary>
    /// The projection's W column. Also accessible by using the index position <c>[3]</c>.
    /// </summary>
    public Vector4 W;

    /// <summary>
    /// Access whole columns in the form of <see cref="Vector4"/>.
    /// </summary>
    /// <param name="column">Which column vector.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="column"/> is not 0, 1, 2 or 3.
    /// </exception>
    public Vector4 this[int column]
    {
        readonly get
        {
            return column switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new ArgumentOutOfRangeException(nameof(column)),
            };
        }
        set
        {
            switch (column)
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
                    throw new ArgumentOutOfRangeException(nameof(column));
            }
        }
    }

    /// <summary>
    /// Access single values.
    /// </summary>
    /// <param name="column">Which column vector.</param>
    /// <param name="row">Which row of the column.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="column"/> or <paramref name="row"/> are not 0, 1, 2 or 3.
    /// </exception>
    public real_t this[int column, int row]
    {
        readonly get
        {
            return column switch
            {
                0 => X[row],
                1 => Y[row],
                2 => Z[row],
                3 => W[row],
                _ => throw new ArgumentOutOfRangeException(nameof(column)),
            };
        }
        set
        {
            switch (column)
            {
                case 0:
                    X[row] = value;
                    return;
                case 1:
                    Y[row] = value;
                    return;
                case 2:
                    Z[row] = value;
                    return;
                case 3:
                    W[row] = value;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column));
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions from a depth range of
    /// <c>-1</c> to <c>1</c> to one that ranges from <c>0</c> to <c>1</c>, and flips the projected
    /// positions vertically, according to <paramref name="flipY"/>.
    /// </summary>
    /// <param name="flipY">If the projection should be flipped vertically.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateDepthCorrection(bool flipY)
    {
        return new Projection
        (
            new Vector4(1, 0, 0, 0),
            new Vector4(0, flipY ? -1 : 1, 0, 0),
            new Vector4(0, 0, 0.5f, 0),
            new Vector4(0, 0, 0.5f, 1)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that scales a given projection to fit around
    /// a given <see cref="Aabb"/> in projection space.
    /// </summary>
    /// <param name="aabb">The Aabb to fit the projection around.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateFitAabb(Aabb aabb)
    {
        Vector3 min = aabb.Position;
        Vector3 max = aabb.Position + aabb.Size;

        return new Projection
        (
            new Vector4(2 / (max.X - min.X), 0, 0, 0),
            new Vector4(0, 2 / (max.Y - min.Y), 0, 0),
            new Vector4(0, 0, 2 / (max.Z - min.Z), 0),
            new Vector4(-(max.X + min.X) / (max.X - min.X), -(max.Y + min.Y) / (max.Y - min.Y), -(max.Z + min.Z) / (max.Z - min.Z), 1)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> for projecting positions onto a head-mounted display with
    /// the given X:Y aspect ratio, distance between eyes, display width, distance to lens, oversampling factor,
    /// and depth clipping planes.
    /// <paramref name="eye"/> creates the projection for the left eye when set to 1,
    /// or the right eye when set to 2.
    /// </summary>
    /// <param name="eye">
    /// The eye to create the projection for.
    /// The left eye when set to 1, the right eye when set to 2.
    /// </param>
    /// <param name="aspect">The aspect ratio.</param>
    /// <param name="intraocularDist">The distance between the eyes.</param>
    /// <param name="displayWidth">The display width.</param>
    /// <param name="displayToLens">The distance to the lens.</param>
    /// <param name="oversample">The oversampling factor.</param>
    /// <param name="zNear">The near clipping distance.</param>
    /// <param name="zFar">The far clipping distance.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateForHmd(int eye, real_t aspect, real_t intraocularDist, real_t displayWidth, real_t displayToLens, real_t oversample, real_t zNear, real_t zFar)
    {
        real_t f1 = intraocularDist * 0.5f / displayToLens;
        real_t f2 = (displayWidth - intraocularDist) * 0.5f / displayToLens;
        real_t f3 = displayWidth / 4.0f / displayToLens;

        real_t add = (f1 + f2) * (oversample - 1.0f) / 2.0f;
        f1 += add;
        f2 += add;
        f3 *= oversample;

        f3 /= aspect;

        return eye switch
        {
            1 => CreateFrustum
            (
                -f2 * zNear,
                f1 * zNear,
                -f3 * zNear,
                f3 * zNear,
                zNear,
                zFar
            ),
            2 => CreateFrustum
            (
                -f1 * zNear,
                f2 * zNear,
                -f3 * zNear,
                f3 * zNear,
                zNear,
                zFar
            ),
            _ => Zero,
        };
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions in a frustum with
    /// the given clipping planes.
    /// </summary>
    /// <param name="left">The left clipping distance.</param>
    /// <param name="right">The right clipping distance.</param>
    /// <param name="bottom">The bottom clipping distance.</param>
    /// <param name="top">The top clipping distance.</param>
    /// <param name="depthNear">The near clipping distance.</param>
    /// <param name="depthFar">The far clipping distance.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateFrustum(real_t left, real_t right, real_t bottom, real_t top, real_t depthNear, real_t depthFar)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(right, left);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(top, bottom);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(depthFar, depthNear);

        real_t x = 2 * depthNear / (right - left);
        real_t y = 2 * depthNear / (top - bottom);

        real_t a = (right + left) / (right - left);
        real_t b = (top + bottom) / (top - bottom);
        real_t c = -(depthFar + depthNear) / (depthFar - depthNear);
        real_t d = -2 * depthFar * depthNear / (depthFar - depthNear);

        return new Projection
        (
            new Vector4(x, 0, 0, 0),
            new Vector4(0, y, 0, 0),
            new Vector4(a, b, c, -1),
            new Vector4(0, 0, d, 0)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions in a frustum with
    /// the given size, X:Y aspect ratio, offset, and clipping planes.
    /// <paramref name="flipFov"/> determines whether the projection's field of view is flipped over its diagonal.
    /// </summary>
    /// <param name="size">The frustum size.</param>
    /// <param name="aspect">The aspect ratio.</param>
    /// <param name="offset">The offset to apply.</param>
    /// <param name="depthNear">The near clipping distance.</param>
    /// <param name="depthFar">The far clipping distance.</param>
    /// <param name="flipFov">If the field of view is flipped over the projection's diagonal.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateFrustumAspect(real_t size, real_t aspect, Vector2 offset, real_t depthNear, real_t depthFar, bool flipFov)
    {
        if (!flipFov)
        {
            size *= aspect;
        }

        return CreateFrustum
        (
            -size / 2 + offset.X,
            +size / 2 + offset.X,
            -size / aspect / 2 + offset.Y,
            +size / aspect / 2 + offset.Y,
            depthNear,
            depthFar
        );
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions into the given <see cref="Rect2"/>.
    /// </summary>
    /// <param name="rect">The Rect2 to project positions into.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateLightAtlasRect(Rect2 rect)
    {
        return new Projection
        (
            new Vector4(rect.Size.X, 0, 0, 0),
            new Vector4(0, rect.Size.Y, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(rect.Position.X, rect.Position.Y, 0, 1)
        );
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions using an orthogonal projection with
    /// the given clipping planes.
    /// </summary>
    /// <param name="left">The left clipping distance.</param>
    /// <param name="right">The right clipping distance.</param>
    /// <param name="bottom">The bottom clipping distance.</param>
    /// <param name="top">The top clipping distance.</param>
    /// <param name="zNear">The near clipping distance.</param>
    /// <param name="zFar">The far clipping distance.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateOrthogonal(real_t left, real_t right, real_t bottom, real_t top, real_t zNear, real_t zFar)
    {
        Projection projection = Identity;
        projection.X.X = 2.0f / (right - left);
        projection.W.X = -((right + left) / (right - left));
        projection.Y.Y = 2.0f / (top - bottom);
        projection.W.Y = -((top + bottom) / (top - bottom));
        projection.Z.Z = -2.0f / (zFar - zNear);
        projection.W.Z = -((zFar + zNear) / (zFar - zNear));
        projection.W.W = 1.0f;
        return projection;
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions using an orthogonal projection with
    /// the given size, X:Y aspect ratio, and clipping planes.
    /// <paramref name="flipFov"/> determines whether the projection's field of view is flipped over its diagonal.
    /// </summary>
    /// <param name="size">The frustum size.</param>
    /// <param name="aspect">The aspect ratio.</param>
    /// <param name="zNear">The near clipping distance.</param>
    /// <param name="zFar">The far clipping distance.</param>
    /// <param name="flipFov">If the field of view is flipped over the projection's diagonal.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreateOrthogonalAspect(real_t size, real_t aspect, real_t zNear, real_t zFar, bool flipFov)
    {
        if (!flipFov)
        {
            size *= aspect;
        }

        return CreateOrthogonal
        (
            -size / 2,
            +size / 2,
            -size / aspect / 2,
            +size / aspect / 2,
            zNear,
            zFar
        );
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions using a perspective projection with
    /// the given Y-axis field of view (in degrees), X:Y aspect ratio, and clipping planes.
    /// <paramref name="flipFov"/> determines whether the projection's field of view is flipped over its diagonal.
    /// </summary>
    /// <param name="fovyDegrees">The vertical field of view (in degrees).</param>
    /// <param name="aspect">The aspect ratio.</param>
    /// <param name="zNear">The near clipping distance.</param>
    /// <param name="zFar">The far clipping distance.</param>
    /// <param name="flipFov">If the field of view is flipped over the projection's diagonal.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreatePerspective(real_t fovyDegrees, real_t aspect, real_t zNear, real_t zFar, bool flipFov)
    {
        if (flipFov)
        {
            fovyDegrees = GetFovy(fovyDegrees, 1.0f / aspect);
        }

        real_t radians = real_t.DegreesToRadians(fovyDegrees / 2.0f);
        real_t deltaZ = zFar - zNear;
        (real_t sin, real_t cos) = real_t.SinCos(radians);

        if ((deltaZ == 0) || (sin == 0) || (aspect == 0))
        {
            return Zero;
        }

        real_t cotangent = cos / sin;

        Projection projection = Identity;
        projection.X.X = cotangent / aspect;
        projection.Y.Y = cotangent;
        projection.Z.Z = -(zFar + zNear) / deltaZ;
        projection.Z.W = -1;
        projection.W.Z = -2 * zNear * zFar / deltaZ;
        projection.W.W = 0;
        return projection;
    }

    /// <summary>
    /// Creates a new <see cref="Projection"/> that projects positions using a perspective projection with
    /// the given Y-axis field of view (in degrees), X:Y aspect ratio, and clipping distances.
    /// The projection is adjusted for a head-mounted display with the given distance between eyes and distance
    /// to a point that can be focused on.
    /// <paramref name="eye"/> creates the projection for the left eye when set to 1,
    /// or the right eye when set to 2.
    /// <paramref name="flipFov"/> determines whether the projection's field of view is flipped over its diagonal.
    /// </summary>
    /// <param name="fovyDegrees">The vertical field of view (in degrees).</param>
    /// <param name="aspect">The aspect ratio.</param>
    /// <param name="zNear">The near clipping distance.</param>
    /// <param name="zFar">The far clipping distance.</param>
    /// <param name="flipFov">If the field of view is flipped over the projection's diagonal.</param>
    /// <param name="eye">
    /// The eye to create the projection for.
    /// The left eye when set to 1, the right eye when set to 2.
    /// </param>
    /// <param name="intraocularDist">The distance between the eyes.</param>
    /// <param name="convergenceDist">The distance to a point of convergence that can be focused on.</param>
    /// <returns>The created projection.</returns>
    public static Projection CreatePerspectiveHmd(real_t fovyDegrees, real_t aspect, real_t zNear, real_t zFar, bool flipFov, int eye, real_t intraocularDist, real_t convergenceDist)
    {
        if (flipFov)
        {
            fovyDegrees = GetFovy(fovyDegrees, 1.0f / aspect);
        }

        real_t ymax = zNear * real_t.Tan(real_t.DegreesToRadians(fovyDegrees / 2.0f));
        real_t xmax = ymax * aspect;
        real_t frustumshift = intraocularDist / 2.0f * zNear / convergenceDist;
        real_t left;
        real_t right;
        real_t modeltranslation;
        switch (eye)
        {
            case 1:
                left = -xmax + frustumshift;
                right = xmax + frustumshift;
                modeltranslation = intraocularDist / 2.0f;
                break;
            case 2:
                left = -xmax - frustumshift;
                right = xmax - frustumshift;
                modeltranslation = -intraocularDist / 2.0f;
                break;
            default:
                left = -xmax;
                right = xmax;
                modeltranslation = 0;
                break;
        }
        Projection projection = CreateFrustum(left, right, -ymax, ymax, zNear, zFar);
        Projection cm = Identity;
        cm.W.X = modeltranslation;
        return projection * cm;
    }

    /// <summary>
    /// Returns a scalar value that is the signed factor by which areas are scaled by this matrix.
    /// If the sign is negative, the matrix flips the orientation of the area.
    /// The determinant can be used to calculate the invertibility of a matrix or solve linear systems
    /// of equations involving the matrix, among other applications.
    /// </summary>
    /// <returns>The determinant calculated from this projection.</returns>
    public readonly real_t Determinant()
    {
        return X.W * Y.Z * Z.Y * W.X - X.Z * Y.W * Z.Y * W.X -
               X.W * Y.Y * Z.Z * W.X + X.Y * Y.W * Z.Z * W.X +
               X.Z * Y.Y * Z.W * W.X - X.Y * Y.Z * Z.W * W.X -
               X.W * Y.Z * Z.X * W.Y + X.Z * Y.W * Z.X * W.Y +
               X.W * Y.X * Z.Z * W.Y - X.X * Y.W * Z.Z * W.Y -
               X.Z * Y.X * Z.W * W.Y + X.X * Y.Z * Z.W * W.Y +
               X.W * Y.Y * Z.X * W.Z - X.Y * Y.W * Z.X * W.Z -
               X.W * Y.X * Z.Y * W.Z + X.X * Y.W * Z.Y * W.Z +
               X.Y * Y.X * Z.W * W.Z - X.X * Y.Y * Z.W * W.Z -
               X.Z * Y.Y * Z.X * W.W + X.Y * Y.Z * Z.X * W.W +
               X.Z * Y.X * Z.Y * W.W - X.X * Y.Z * Z.Y * W.W -
               X.Y * Y.X * Z.Z * W.W + X.X * Y.Y * Z.Z * W.W;
    }

    /// <summary>
    /// Returns the X:Y aspect ratio of this <see cref="Projection"/>'s viewport.
    /// </summary>
    /// <returns>The aspect ratio from this projection's viewport.</returns>
    public readonly real_t GetAspect()
    {
        Vector2 vpHe = GetViewportHalfExtents();
        return vpHe.X / vpHe.Y;
    }

    /// <summary>
    /// Returns the horizontal field of view of the projection (in degrees).
    /// </summary>
    /// <returns>The horizontal field of view of this projection.</returns>
    public readonly real_t GetFov()
    {
        Plane rightPlane = new Plane(X.W - X.X, Y.W - Y.X, Z.W - Z.X, -W.W + W.X).Normalized();
        if (Z.X == 0 && Z.Y == 0)
        {
            return real_t.RadiansToDegrees(real_t.Acos(real_t.Abs(rightPlane.Normal.X))) * 2.0f;
        }
        else
        {
            Plane leftPlane = new Plane(X.W + X.X, Y.W + Y.X, Z.W + Z.X, W.W + W.X).Normalized();
            return real_t.RadiansToDegrees(real_t.Acos(real_t.Abs(leftPlane.Normal.X))) + real_t.RadiansToDegrees(real_t.Acos(real_t.Abs(rightPlane.Normal.X)));
        }
    }

    /// <summary>
    /// Returns the vertical field of view of the projection (in degrees) associated with
    /// the given horizontal field of view (in degrees) and aspect ratio.
    /// </summary>
    /// <param name="fovx">The horizontal field of view (in degrees).</param>
    /// <param name="aspect">The aspect ratio.</param>
    /// <returns>The vertical field of view of this projection.</returns>
    public static real_t GetFovy(real_t fovx, real_t aspect)
    {
        return real_t.RadiansToDegrees(real_t.Atan(aspect * real_t.Tan(real_t.DegreesToRadians(fovx) * 0.5f)) * 2.0f);
    }

    /// <summary>
    /// Returns the factor by which the visible level of detail is scaled by this <see cref="Projection"/>.
    /// </summary>
    /// <returns>The level of detail factor for this projection.</returns>
    public readonly real_t GetLodMultiplier()
    {
        if (IsOrthogonal())
        {
            return GetViewportHalfExtents().X;
        }
        else
        {
            real_t zn = GetZNear();
            real_t width = GetViewportHalfExtents().X * 2.0f;
            return 1.0f / (zn / width);
        }
    }

    /// <summary>
    /// Returns the number of pixels with the given pixel width displayed per meter, after
    /// this <see cref="Projection"/> is applied.
    /// </summary>
    /// <param name="forPixelWidth">The width for each pixel (in meters).</param>
    /// <returns>The number of pixels per meter.</returns>
    public readonly int GetPixelsPerMeter(int forPixelWidth)
    {
        Vector3 result = this * new Vector3(1, 0, -1);

        return (int)((result.X * 0.5f + 0.5f) * forPixelWidth);
    }

    /// <summary>
    /// Returns the clipping plane of this <see cref="Projection"/> whose index is given
    /// by <paramref name="plane"/>.
    /// <paramref name="plane"/> should be equal to one of <see cref="Planes.Near"/>,
    /// <see cref="Planes.Far"/>, <see cref="Planes.Left"/>, <see cref="Planes.Top"/>,
    /// <see cref="Planes.Right"/>, or <see cref="Planes.Bottom"/>.
    /// </summary>
    /// <param name="plane">The kind of clipping plane to get from the projection.</param>
    /// <returns>The clipping plane of this projection.</returns>
    public readonly Plane GetProjectionPlane(Planes plane)
    {
        Plane newPlane = plane switch
        {
            Planes.Near => new Plane(X.W + X.Z, Y.W + Y.Z, Z.W + Z.Z, W.W + W.Z),
            Planes.Far => new Plane(X.W - X.Z, Y.W - Y.Z, Z.W - Z.Z, W.W - W.Z),
            Planes.Left => new Plane(X.W + X.X, Y.W + Y.X, Z.W + Z.X, W.W + W.X),
            Planes.Top => new Plane(X.W - X.Y, Y.W - Y.Y, Z.W - Z.Y, W.W - W.Y),
            Planes.Right => new Plane(X.W - X.X, Y.W - Y.X, Z.W - Z.X, W.W - W.X),
            Planes.Bottom => new Plane(X.W + X.Y, Y.W + Y.Y, Z.W + Z.Y, W.W + W.Y),
            _ => new Plane(),
        };
        newPlane.Normal = -newPlane.Normal;
        return newPlane.Normalized();
    }

    /// <summary>
    /// Returns the dimensions of the far clipping plane of the projection, divided by two.
    /// </summary>
    /// <returns>The half extents for this projection's far plane.</returns>
    public readonly Vector2 GetFarPlaneHalfExtents()
    {
        Plane farPlane = GetProjectionPlane(Planes.Far);
        Plane rightPlane = GetProjectionPlane(Planes.Right);
        Plane topPlane = GetProjectionPlane(Planes.Top);

        Vector3 res = farPlane.Intersect3(rightPlane, topPlane) ?? Vector3.Zero;
        return new Vector2(res.X, res.Y);
    }

    /// <summary>
    /// Returns the dimensions of the viewport plane that this <see cref="Projection"/>
    /// projects positions onto, divided by two.
    /// </summary>
    /// <returns>The half extents for this projection's viewport plane.</returns>
    public readonly Vector2 GetViewportHalfExtents()
    {
        Plane nearPlane = GetProjectionPlane(Planes.Near);
        Plane rightPlane = GetProjectionPlane(Planes.Right);
        Plane topPlane = GetProjectionPlane(Planes.Top);

        Vector3 res = nearPlane.Intersect3(rightPlane, topPlane) ?? Vector3.Zero;
        return new Vector2(res.X, res.Y);
    }

    /// <summary>
    /// Returns the distance for this <see cref="Projection"/> beyond which positions are clipped.
    /// </summary>
    /// <returns>The distance beyond which positions are clipped.</returns>
    public readonly real_t GetZFar()
    {
        return GetProjectionPlane(Planes.Far).D;
    }

    /// <summary>
    /// Returns the distance for this <see cref="Projection"/> before which positions are clipped.
    /// </summary>
    /// <returns>The distance before which positions are clipped.</returns>
    public readonly real_t GetZNear()
    {
        return -GetProjectionPlane(Planes.Near).D;
    }

    /// <summary>
    /// Returns a copy of this <see cref="Projection"/> with the signs of the values of the Y column flipped.
    /// </summary>
    /// <returns>The flipped projection.</returns>
    public readonly Projection FlippedY()
    {
        return this with { Y = -Y };
    }

    /// <summary>
    /// Returns a <see cref="Projection"/> with the near clipping distance adjusted to be
    /// <paramref name="newZNear"/>.
    /// Note: The original <see cref="Projection"/> must be a perspective projection.
    /// </summary>
    /// <param name="newZNear">The near clipping distance to adjust the projection to.</param>
    /// <returns>The adjusted projection.</returns>
    public readonly Projection PerspectiveZNearAdjusted(real_t newZNear)
    {
        Projection projection = this;
        real_t zFar = GetZFar();
        real_t zNear = newZNear;
        real_t deltaZ = zFar - zNear;
        projection.Z.Z = -(zFar + zNear) / deltaZ;
        projection.W.Z = -2 * zNear * zFar / deltaZ;
        return projection;
    }

    /// <summary>
    /// Returns a <see cref="Projection"/> with the X and Y values from the given <see cref="Vector2"/>
    /// added to the first and second values of the final column respectively.
    /// </summary>
    /// <param name="offset">The offset to apply to the projection.</param>
    /// <returns>The offsetted projection.</returns>
    public readonly Projection JitterOffseted(Vector2 offset)
    {
        Projection projection = this;
        projection.W.X += offset.X;
        projection.W.Y += offset.Y;
        return projection;
    }

    /// <summary>
    /// Returns a <see cref="Projection"/> that performs the inverse of this <see cref="Projection"/>'s
    /// projective transformation.
    /// </summary>
    /// <returns>The inverted projection.</returns>
    public readonly Projection Inverse()
    {
        Projection projection = this;
        int i, j, k;
        // Locations of pivot matrix.
        int[] pivotI = new int[4];
        int[] pivotJ = new int[4];
        // Value of current pivot element.
        real_t pivotValue;
        // Temporary storage.
        real_t hold;
        real_t determinant = 1.0f;
        for (k = 0; k < 4; k++)
        {
            // Locate k'th pivot element.
            // Initialize for search.
            pivotValue = projection[k][k];
            pivotI[k] = k;
            pivotJ[k] = k;
            for (i = k; i < 4; i++)
            {
                for (j = k; j < 4; j++)
                {
                    if (real_t.Abs(projection[i][j]) > real_t.Abs(pivotValue))
                    {
                        pivotI[k] = i;
                        pivotJ[k] = j;
                        pivotValue = projection[i][j];
                    }
                }
            }

            // Product of pivots, gives determinant when finished.
            determinant *= pivotValue;
            if (Mathf.IsZeroApprox(determinant))
            {
                return Zero;
            }

            // "Interchange" rows (with sign change stuff).
            i = pivotI[k];
            if (i != k)
            {
                // If rows are different.
                for (j = 0; j < 4; j++)
                {
                    hold = -projection[k][j];
                    projection[k, j] = projection[i][j];
                    projection[i, j] = hold;
                }
            }

            // "Interchange" columns.
            j = pivotJ[k];
            if (j != k)
            {
                // If columns are different.
                for (i = 0; i < 4; i++)
                {
                    hold = -projection[i][k];
                    projection[i, k] = projection[i][j];
                    projection[i, j] = hold;
                }
            }

            // Divide column by minus pivot value.
            for (i = 0; i < 4; i++)
            {
                if (i != k)
                {
                    projection[i, k] /= -pivotValue;
                }
            }

            // Reduce the matrix.
            for (i = 0; i < 4; i++)
            {
                hold = projection[i][k];
                for (j = 0; j < 4; j++)
                {
                    if (i != k && j != k)
                    {
                        projection[i, j] += hold * projection[k][j];
                    }
                }
            }

            // Divide row by pivot.
            for (j = 0; j < 4; j++)
            {
                if (j != k)
                {
                    projection[k, j] /= pivotValue;
                }
            }

            // Replace pivot by reciprocal (at last we can touch it).
            projection[k, k] = 1.0f / pivotValue;
        }

        // That was most of the work, one final pass of row/column interchange to finish.
        for (k = 4 - 2; k >= 0; k--)
        {
            // Don't need to work with 1 by 1 corner.
            // Rows to swap correspond to pivot COLUMN.
            i = pivotJ[k];
            if (i != k)
            {
                // If rows are different.
                for (j = 0; j < 4; j++)
                {
                    hold = projection[k][j];
                    projection[k, j] = -projection[i][j];
                    projection[i, j] = hold;
                }
            }

            // Columns to swap correspond to pivot ROW.
            j = pivotI[k];
            if (j != k)
            {
                // If columns are different.
                for (i = 0; i < 4; i++)
                {
                    hold = projection[i][k];
                    projection[i, k] = -projection[i][j];
                    projection[i, j] = hold;
                }
            }
        }
        return projection;
    }

    /// <summary>
    /// Returns <see langword="true"/> if this <see cref="Projection"/> performs an orthogonal projection.
    /// </summary>
    /// <returns>If the projection performs an orthogonal projection.</returns>
    public readonly bool IsOrthogonal()
    {
        return W.W == 1.0f;
    }

    /// <summary>
    /// Zero projection, a projection with all components set to <c>0</c>.
    /// </summary>
    /// <value>Equivalent to <c>new Projection(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero)</c>.</value>
    public static Projection Zero => new Projection
    (
        new Vector4(0, 0, 0, 0),
        new Vector4(0, 0, 0, 0),
        new Vector4(0, 0, 0, 0),
        new Vector4(0, 0, 0, 0)
    );

    /// <summary>
    /// The identity projection, with no distortion applied.
    /// This is used as a replacement for <c>Projection()</c> in GDScript.
    /// Do not use <c>new Projection()</c> with no arguments in C#, because it sets all values to zero.
    /// </summary>
    /// <value>Equivalent to <c>new Projection(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1))</c>.</value>
    public static Projection Identity => new Projection
    (
        new Vector4(1, 0, 0, 0),
        new Vector4(0, 1, 0, 0),
        new Vector4(0, 0, 1, 0),
        new Vector4(0, 0, 0, 1)
    );

    /// <summary>
    /// Constructs a projection from 4 vectors (matrix columns).
    /// </summary>
    /// <param name="x">The X column, or column index 0.</param>
    /// <param name="y">The Y column, or column index 1.</param>
    /// <param name="z">The Z column, or column index 2.</param>
    /// <param name="w">The W column, or column index 3.</param>
    public Projection(Vector4 x, Vector4 y, Vector4 z, Vector4 w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Constructs a projection from 16 scalars.
    /// </summary>
    /// <param name="xx">The X column vector's X component, accessed via <c>p.X.X</c> or <c>[0][0]</c>.</param>
    /// <param name="xy">The X column vector's Y component, accessed via <c>p.X.Y</c> or <c>[0][1]</c>.</param>
    /// <param name="xz">The X column vector's Z component, accessed via <c>p.X.Z</c> or <c>[0][2]</c>.</param>
    /// <param name="xw">The X column vector's W component, accessed via <c>p.X.W</c> or <c>[0][3]</c>.</param>
    /// <param name="yx">The Y column vector's X component, accessed via <c>p.Y.X</c> or <c>[1][0]</c>.</param>
    /// <param name="yy">The Y column vector's Y component, accessed via <c>p.Y.Y</c> or <c>[1][1]</c>.</param>
    /// <param name="yz">The Y column vector's Z component, accessed via <c>p.Y.Z</c> or <c>[1][2]</c>.</param>
    /// <param name="yw">The Y column vector's W component, accessed via <c>p.Y.W</c> or <c>[1][3]</c>.</param>
    /// <param name="zx">The Z column vector's X component, accessed via <c>p.Z.X</c> or <c>[2][0]</c>.</param>
    /// <param name="zy">The Z column vector's Y component, accessed via <c>p.Z.Y</c> or <c>[2][1]</c>.</param>
    /// <param name="zz">The Z column vector's Z component, accessed via <c>p.Z.Z</c> or <c>[2][2]</c>.</param>
    /// <param name="zw">The Z column vector's W component, accessed via <c>p.Z.W</c> or <c>[2][3]</c>.</param>
    /// <param name="wx">The W column vector's X component, accessed via <c>p.W.X</c> or <c>[3][0]</c>.</param>
    /// <param name="wy">The W column vector's Y component, accessed via <c>p.W.Y</c> or <c>[3][1]</c>.</param>
    /// <param name="wz">The W column vector's Z component, accessed via <c>p.W.Z</c> or <c>[3][2]</c>.</param>
    /// <param name="ww">The W column vector's W component, accessed via <c>p.W.W</c> or <c>[3][3]</c>.</param>
    public Projection(real_t xx, real_t xy, real_t xz, real_t xw, real_t yx, real_t yy, real_t yz, real_t yw, real_t zx, real_t zy, real_t zz, real_t zw, real_t wx, real_t wy, real_t wz, real_t ww)
    {
        X = new Vector4(xx, xy, xz, xw);
        Y = new Vector4(yx, yy, yz, yw);
        Z = new Vector4(zx, zy, zz, zw);
        W = new Vector4(wx, wy, wz, ww);
    }

    /// <summary>
    /// Constructs a new <see cref="Projection"/> from a <see cref="Transform3D"/>.
    /// </summary>
    /// <param name="transform">The <see cref="Transform3D"/>.</param>
    public Projection(Transform3D transform)
    {
        X = new Vector4(transform.Basis.Row0.X, transform.Basis.Row1.X, transform.Basis.Row2.X, 0);
        Y = new Vector4(transform.Basis.Row0.Y, transform.Basis.Row1.Y, transform.Basis.Row2.Y, 0);
        Z = new Vector4(transform.Basis.Row0.Z, transform.Basis.Row1.Z, transform.Basis.Row2.Z, 0);
        W = new Vector4(transform.Origin.X, transform.Origin.Y, transform.Origin.Z, 1);
    }

    /// <summary>
    /// Composes these two projections by multiplying them
    /// together. This has the effect of applying the right
    /// and then the left projection.
    /// </summary>
    /// <param name="left">The parent transform.</param>
    /// <param name="right">The child transform.</param>
    /// <returns>The composed projection.</returns>
    public static Projection operator *(Projection left, Projection right)
    {
        return new Projection
        (
            new Vector4
            (
                left.X.X * right.X.X + left.Y.X * right.X.Y + left.Z.X * right.X.Z + left.W.X * right.X.W,
                left.X.Y * right.X.X + left.Y.Y * right.X.Y + left.Z.Y * right.X.Z + left.W.Y * right.X.W,
                left.X.Z * right.X.X + left.Y.Z * right.X.Y + left.Z.Z * right.X.Z + left.W.Z * right.X.W,
                left.X.W * right.X.X + left.Y.W * right.X.Y + left.Z.W * right.X.Z + left.W.W * right.X.W
            ),
            new Vector4
            (
                left.X.X * right.Y.X + left.Y.X * right.Y.Y + left.Z.X * right.Y.Z + left.W.X * right.Y.W,
                left.X.Y * right.Y.X + left.Y.Y * right.Y.Y + left.Z.Y * right.Y.Z + left.W.Y * right.Y.W,
                left.X.Z * right.Y.X + left.Y.Z * right.Y.Y + left.Z.Z * right.Y.Z + left.W.Z * right.Y.W,
                left.X.W * right.Y.X + left.Y.W * right.Y.Y + left.Z.W * right.Y.Z + left.W.W * right.Y.W
            ),
            new Vector4
            (
                left.X.X * right.Z.X + left.Y.X * right.Z.Y + left.Z.X * right.Z.Z + left.W.X * right.Z.W,
                left.X.Y * right.Z.X + left.Y.Y * right.Z.Y + left.Z.Y * right.Z.Z + left.W.Y * right.Z.W,
                left.X.Z * right.Z.X + left.Y.Z * right.Z.Y + left.Z.Z * right.Z.Z + left.W.Z * right.Z.W,
                left.X.W * right.Z.X + left.Y.W * right.Z.Y + left.Z.W * right.Z.Z + left.W.W * right.Z.W
            ),
            new Vector4
            (
                left.X.X * right.W.X + left.Y.X * right.W.Y + left.Z.X * right.W.Z + left.W.X * right.W.W,
                left.X.Y * right.W.X + left.Y.Y * right.W.Y + left.Z.Y * right.W.Z + left.W.Y * right.W.W,
                left.X.Z * right.W.X + left.Y.Z * right.W.Y + left.Z.Z * right.W.Z + left.W.Z * right.W.W,
                left.X.W * right.W.X + left.Y.W * right.W.Y + left.Z.W * right.W.Z + left.W.W * right.W.W
            )
        );
    }

    /// <summary>
    /// Returns a Vector4 transformed (multiplied) by the projection.
    /// For transforming by inverse of a projection <c>projection.Inverse() * vector</c>
    /// can be used instead. See <see cref="Inverse"/>.
    /// </summary>
    /// <param name="projection">The projection to apply.</param>
    /// <param name="vector">A Vector4 to transform.</param>
    /// <returns>The transformed Vector4.</returns>
    public static Vector4 operator *(Projection projection, Vector4 vector)
    {
        return new Vector4
        (
            projection.X.X * vector.X + projection.Y.X * vector.Y + projection.Z.X * vector.Z + projection.W.X * vector.W,
            projection.X.Y * vector.X + projection.Y.Y * vector.Y + projection.Z.Y * vector.Z + projection.W.Y * vector.W,
            projection.X.Z * vector.X + projection.Y.Z * vector.Y + projection.Z.Z * vector.Z + projection.W.Z * vector.W,
            projection.X.W * vector.X + projection.Y.W * vector.Y + projection.Z.W * vector.Z + projection.W.W * vector.W
        );
    }

    /// <summary>
    /// Returns a Vector4 transformed (multiplied) by the inverse projection.
    /// </summary>
    /// <param name="projection">The projection to apply.</param>
    /// <param name="vector">A Vector4 to transform.</param>
    /// <returns>The inversely transformed Vector4.</returns>
    public static Vector4 operator *(Vector4 vector, Projection projection)
    {
        return new Vector4
        (
            projection.X.X * vector.X + projection.X.Y * vector.Y + projection.X.Z * vector.Z + projection.X.W * vector.W,
            projection.Y.X * vector.X + projection.Y.Y * vector.Y + projection.Y.Z * vector.Z + projection.Y.W * vector.W,
            projection.Z.X * vector.X + projection.Z.Y * vector.Y + projection.Z.Z * vector.Z + projection.Z.W * vector.W,
            projection.W.X * vector.X + projection.W.Y * vector.Y + projection.W.Z * vector.Z + projection.W.W * vector.W
        );
    }

    /// <summary>
    /// Returns a Vector3 transformed (multiplied) by the projection.
    /// </summary>
    /// <param name="projection">The projection to apply.</param>
    /// <param name="vector">A Vector3 to transform.</param>
    /// <returns>The transformed Vector3.</returns>
    public static Vector3 operator *(Projection projection, Vector3 vector)
    {
        Vector3 ret = new Vector3
        (
            projection.X.X * vector.X + projection.Y.X * vector.Y + projection.Z.X * vector.Z + projection.W.X,
            projection.X.Y * vector.X + projection.Y.Y * vector.Y + projection.Z.Y * vector.Z + projection.W.Y,
            projection.X.Z * vector.X + projection.Y.Z * vector.Y + projection.Z.Z * vector.Z + projection.W.Z
        );
        return ret / (projection.X.W * vector.X + projection.Y.W * vector.Y + projection.Z.W * vector.Z + projection.W.W);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the projections are exactly equal.
    /// </summary>
    /// <param name="left">The left projection.</param>
    /// <param name="right">The right projection.</param>
    /// <returns>Whether or not the projections are exactly equal.</returns>
    public static bool operator ==(Projection left, Projection right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the projections are not exactly equal.
    /// </summary>
    /// <param name="left">The left projection.</param>
    /// <param name="right">The right projection.</param>
    /// <returns>Whether or not the projections are not exactly equal.</returns>
    public static bool operator !=(Projection left, Projection right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Constructs a new <see cref="Transform3D"/> from the <see cref="Projection"/>.
    /// </summary>
    /// <param name="projection">The <see cref="Projection"/>.</param>
    public static explicit operator Transform3D(Projection projection)
    {
        return new Transform3D
        (
            new Basis
            (
                new Vector3(projection.X.X, projection.X.Y, projection.X.Z),
                new Vector3(projection.Y.X, projection.Y.Y, projection.Y.Z),
                new Vector3(projection.Z.X, projection.Z.Y, projection.Z.Z)
            ),
            new Vector3(projection.W.X, projection.W.Y, projection.W.Z)
        );
    }

    /// <summary>
    /// Returns <see langword="true"/> if the projection is exactly equal
    /// to the given object (<paramref name="obj"/>).
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>Whether or not the vector and the object are equal.</returns>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Projection other && Equals(other);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the projections are exactly equal.
    /// </summary>
    /// <param name="other">The other projection.</param>
    /// <returns>Whether or not the projections are exactly equal.</returns>
    public readonly bool Equals(Projection other)
    {
        return X == other.X
            && Y == other.Y
            && Z == other.Z
            && W == other.W;
    }

    /// <summary>
    /// Serves as the hash function for <see cref="Projection"/>.
    /// </summary>
    /// <returns>A hash code for this projection.</returns>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    /// <summary>
    /// Converts this <see cref="Projection"/> to a string.
    /// </summary>
    /// <returns>A string representation of this projection.</returns>
    public override readonly string ToString() => ToString(null);

    /// <summary>
    /// Converts this <see cref="Projection"/> to a string with the given <paramref name="format"/>.
    /// </summary>
    /// <returns>A string representation of this projection.</returns>
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return $"""
            {X.X.ToString(format, CultureInfo.InvariantCulture)}, {X.Y.ToString(format, CultureInfo.InvariantCulture)}, {X.Z.ToString(format, CultureInfo.InvariantCulture)}, {X.W.ToString(format, CultureInfo.InvariantCulture)}
            {Y.X.ToString(format, CultureInfo.InvariantCulture)}, {Y.Y.ToString(format, CultureInfo.InvariantCulture)}, {Y.Z.ToString(format, CultureInfo.InvariantCulture)}, {Y.W.ToString(format, CultureInfo.InvariantCulture)}
            {Z.X.ToString(format, CultureInfo.InvariantCulture)}, {Z.Y.ToString(format, CultureInfo.InvariantCulture)}, {Z.Z.ToString(format, CultureInfo.InvariantCulture)}, {Z.W.ToString(format, CultureInfo.InvariantCulture)}
            {W.X.ToString(format, CultureInfo.InvariantCulture)}, {W.Y.ToString(format, CultureInfo.InvariantCulture)}, {W.Z.ToString(format, CultureInfo.InvariantCulture)}, {W.W.ToString(format, CultureInfo.InvariantCulture)}
            """;
    }
}

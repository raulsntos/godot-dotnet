using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Godot.NativeInterop;

namespace Godot;

/// <summary>
/// StringNames are immutable strings designed for general-purpose representation of unique names.
/// StringName ensures that only one instance of a given name exists (so two StringNames with the
/// same value are the same object).
/// Comparing them is much faster than with regular strings, because only the pointers are compared,
/// not the whole strings.
/// </summary>
public sealed class StringName : IDisposable, IEquatable<StringName?>
{
    internal readonly NativeGodotStringName.Movable NativeValue;

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    private readonly bool _isStatic;

    internal static StringName Empty { get; } = CreateStaticStringNameFromAsciiLiteral(""u8);

    /// <summary>
    /// Check whether this <see cref="StringName"/> is empty.
    /// </summary>
    /// <returns>If the <see cref="StringName"/> is empty.</returns>
    public bool IsEmpty => NativeValue.DangerousSelfRef.IsEmpty;

    private StringName(NativeGodotStringName nativeValueToOwn, bool isStatic = false)
    {
        NativeValue = nativeValueToOwn.AsMovable();
        _isStatic = isStatic;

        // Static StringNames must not be disposed, so don't register the disposable.
        if (!_isStatic)
        {
            _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
        }
    }

    /// <summary>
    /// Constructs a new <see cref="StringName"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static StringName CreateTakingOwnership(NativeGodotStringName nativeValueToOwn)
    {
        return new StringName(nativeValueToOwn);
    }

    /// <summary>
    /// Constructs a new static <see cref="StringName"/> from an ASCII literal.
    /// This is an internal method to avoid checking if the ASCII value is valid
    /// to use it in generated code where we know the value is already valid.
    /// For other use cases and more information about static StringNames see
    /// <see cref="CreateStaticFromAscii(ReadOnlySpan{byte})"/>.
    /// </summary>
    /// <param name="ascii">ASCII encoded string to construct the static <see cref="StringName"/> from.</param>
    internal static StringName CreateStaticStringNameFromAsciiLiteral(scoped ReadOnlySpan<byte> ascii)
    {
        return new StringName(NativeGodotStringName.Create(ascii, isStatic: true), isStatic: true);
    }

    /// <summary>
    /// Constructs a <see cref="StringName"/> from the given <paramref name="utf8"/> string.
    /// </summary>
    /// <param name="utf8">UTF-8 encoded string to construct the <see cref="StringName"/> from.</param>
    public static StringName CreateFromUtf8(ReadOnlySpan<byte> utf8)
    {
        return CreateTakingOwnership(NativeGodotStringName.Create(utf8));
    }

    /// <summary>
    /// Constructs a <b>static</b> <see cref="StringName"/> from the given <paramref name="ascii"/> string.
    /// Static StringNames are created once and never disposed, they are meant to be created from literals
    /// or constants that are alive for the entire duration of the application, they can easily introduce
    /// undefined behavior if used wrong. In case of doubt, avoid creating static StringNames.
    /// </summary>
    /// <param name="ascii">ASCII encoded string to construct the static <see cref="StringName"/> from.</param>
    public static StringName CreateStaticFromAscii(scoped ReadOnlySpan<byte> ascii)
    {
        if (!Ascii.IsValid(ascii))
        {
            throw new ArgumentException("The bytes are not using ASCII encoding or contains invalid characters.", nameof(ascii));
        }

        return CreateStaticStringNameFromAsciiLiteral(ascii);
    }

    /// <summary>
    /// Constructs a <see cref="StringName"/> from the given <paramref name="name"/> string.
    /// </summary>
    /// <param name="name">String to construct the <see cref="StringName"/> from.</param>
    public StringName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeValue = NativeGodotStringName.Create(name).AsMovable();
            _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
        }
    }

    /// <summary>
    /// Converts a string to a <see cref="StringName"/>.
    /// </summary>
    /// <param name="from">The string to convert.</param>
    [return: NotNullIfNotNull(nameof(from))]
    public static explicit operator StringName?(string? from) => from is not null ? new StringName(from) : null;

    /// <summary>
    /// Converts a <see cref="StringName"/> to a string.
    /// </summary>
    /// <param name="from">The <see cref="StringName"/> to convert.</param>
    [return: NotNullIfNotNull(nameof(from))]
    public static explicit operator string?(StringName? from) => from?.ToString();

    /// <summary>
    /// Releases the unmanaged <see cref="StringName"/> instance.
    /// </summary>
    ~StringName()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="StringName"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isStatic)
        {
            // Static StringNames must not be disposed.
            return;
        }

        // Always dispose `NativeValue` even if disposing is true
        NativeValue.DangerousSelfRef.Dispose();

        if (_weakReferenceToSelf is not null)
        {
            DisposablesTracker.UnregisterDisposable(_weakReferenceToSelf);
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="StringName"/>
    /// instances are equal.
    /// </summary>
    /// <param name="left">The left <see cref="StringName"/>.</param>
    /// <param name="right">The right <see cref="StringName"/>.</param>
    /// <returns>
    /// Whether or not the <see cref="StringName"/> instances are equal.
    /// </returns>
    public static bool operator ==(StringName? left, StringName? right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="StringName"/>
    /// instances are not equal.
    /// </summary>
    /// <param name="left">The left <see cref="StringName"/>.</param>
    /// <param name="right">The right <see cref="StringName"/>.</param>
    /// <returns>
    /// Whether or not the <see cref="StringName"/> instances are not equal.
    /// </returns>
    public static bool operator !=(StringName? left, StringName? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="StringName"/>
    /// instances are equal.
    /// </summary>
    /// <param name="other">The other <see cref="StringName"/>.</param>
    /// <returns>
    /// Whether or not the <see cref="StringName"/> instances are equal.
    /// </returns>
    public bool Equals([NotNullWhen(true)] StringName? other)
    {
        if (other is null)
        {
            return false;
        }
        return NativeValue.DangerousSelfRef == other.NativeValue.DangerousSelfRef;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="StringName"/> is
    /// equal to the given object (<paramref name="obj"/>).
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>
    /// Whether or not the <see cref="StringName"/> and the object are equal.
    /// </returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is StringName other && Equals(other));
    }

    /// <summary>
    /// Serves as the hash function for <see cref="StringName"/>.
    /// </summary>
    /// <returns>A hash code for this <see cref="StringName"/>.</returns>
    public override int GetHashCode()
    {
        return NativeValue.DangerousSelfRef.GetHashCode();
    }

    /// <summary>
    /// Converts this <see cref="StringName"/> to a string.
    /// </summary>
    /// <returns>A string representation of this <see cref="StringName"/>.</returns>
    public override string ToString()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }

        ref NativeGodotStringName self = ref NativeValue.DangerousSelfRef;
        using NativeGodotString str = NativeGodotString.Create(self);
        return str.ToString();
    }
}

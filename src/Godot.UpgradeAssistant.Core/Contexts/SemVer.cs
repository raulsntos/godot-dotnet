using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Represents a version using the semantic versioning standard.
/// </summary>
public readonly partial struct SemVer : IEquatable<SemVer>, IComparable<SemVer>, ISpanFormattable, ISpanParsable<SemVer>
{
    // 10 for the longest input: 2,147,483,647.
    private const int Int32NumberBufferLength = 10 + 1;

    /// <summary>
    /// The major part of the version, the first number.
    /// </summary>
    public int Major { get; }

    /// <summary>
    /// The minor part of the version, the second number.
    /// </summary>
    public int Minor { get; }

    /// <summary>
    /// The patch part of the version, the third number.
    /// </summary>
    public int Patch { get; }

    /// <summary>
    /// The pre-release part of the version.
    /// </summary>
    public string? Prerelease { get; }

    /// <summary>
    /// The build metadata part of the version.
    /// </summary>
    public string? BuildMetadata { get; }

    /// <summary>
    /// Regular expression to parse the semantic version in groups.
    /// Follows the recommendation in https://semver.org/
    /// </summary>
    [GeneratedRegex("^(?<major>0|[1-9]\\d*)\\.(?<minor>0|[1-9]\\d*)\\.(?<patch>0|[1-9]\\d*)(?:-(?<prerelease>(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex SemVerRegex { get; }

    /// <summary>
    /// Construct a <see cref="SemVer"/>.
    /// </summary>
    /// <param name="major">The major part of the version.</param>
    /// <param name="minor">The minor part of the version.</param>
    /// <param name="patch">The patch part of the version.</param>
    /// <param name="prerelease">The pre-release part of the version.</param>
    /// <param name="buildmetadata">The build metadata part of the version.</param>
    public SemVer(int major, int minor = 0, int patch = 0, string? prerelease = null, string? buildmetadata = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
        BuildMetadata = buildmetadata;
    }

    private static int Compare(SemVer left, SemVer right)
    {
        if (left.Major != right.Major)
        {
            return left.Major > right.Major ? 1 : -1;
        }

        if (left.Minor != right.Minor)
        {
            return left.Minor > right.Minor ? 1 : -1;
        }

        if (left.Patch != right.Patch)
        {
            return left.Patch > right.Patch ? 1 : -1;
        }

        if (string.IsNullOrEmpty(left.Prerelease) && string.IsNullOrEmpty(right.Prerelease))
        {
            return 0;
        }

        if (string.IsNullOrEmpty(left.Prerelease) || string.IsNullOrEmpty(right.Prerelease))
        {
            return string.IsNullOrEmpty(left.Prerelease) ? 1 : -1;
        }

        if (left.Prerelease != right.Prerelease)
        {
            string[] leftFields = left.Prerelease.Split('.');
            string[] rightFields = right.Prerelease.Split('.');

            int minFieldCount = Math.Min(leftFields.Length, rightFields.Length);

            for (int i = 0; i < minFieldCount; i++)
            {
                string leftField = leftFields[i];
                string rightField = rightFields[i];

                // For comparison purposes, we want 'dev' to be equivalent to 'alpha'.
                // This is a quirk of GodotSharp and Godot .NET using different names
                // for their SemVer prereleases. Using 'alpha' will ensure the correct
                // lexical order.
                if (i == 0)
                {
                    if (leftField == "dev")
                    {
                        leftField = "alpha";
                    }
                    if (rightField == "dev")
                    {
                        rightField = "alpha";
                    }
                }

                if (leftField == rightField)
                {
                    continue;
                }

                bool leftIsNumericOnly = int.TryParse(leftField, out int leftNumeric);
                bool rightIsNumericOnly = int.TryParse(rightField, out int rightNumeric);

                if (leftIsNumericOnly && rightIsNumericOnly)
                {
                    // Identifiers consisting of only digits are compared numerically.
                    if (leftNumeric == rightNumeric)
                    {
                        continue;
                    }

                    return leftNumeric > rightNumeric ? 1 : -1;
                }

                if (leftIsNumericOnly || rightIsNumericOnly)
                {
                    // Numeric identifiers always have lower precedence than non-numeric identifiers.
                    return rightIsNumericOnly ? 1 : -1;
                }

                // Identifiers with letters or hyphens are compared lexically in ASCII sort order.
                return string.Compare(leftField, rightField, StringComparison.Ordinal);
            }

            if (leftFields.Length != rightFields.Length)
            {
                // A larger set of pre-release fields has a higher precedence than a smaller set,
                // if all of the preceding identifiers are equal.
                return leftFields.Length > rightFields.Length ? 1 : -1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Compares two versions using the semantic versioning rules.
    /// </summary>
    /// <param name="left">One of the versions to compare.</param>
    /// <param name="right">The other version to compare.</param>
    /// <returns>Whether the two versions are the same.</returns>
    public static bool operator ==(SemVer left, SemVer right) => Compare(left, right) == 0;

    /// <summary>
    /// Compares two versions using the semantic versioning rules.
    /// </summary>
    /// <param name="left">One of the versions to compare.</param>
    /// <param name="right">The other version to compare.</param>
    /// <returns>Whether the two versions are not the same.</returns>
    public static bool operator !=(SemVer left, SemVer right) => Compare(left, right) != 0;

    /// <summary>
    /// Compares two versions using the semantic versioning rules.
    /// </summary>
    /// <param name="left">One of the versions to compare.</param>
    /// <param name="right">The other version to compare.</param>
    /// <returns>Whether version <paramref name="left"/> is smaller than <paramref name="right"/>.</returns>
    public static bool operator <(SemVer left, SemVer right) => Compare(left, right) < 0;

    /// <summary>
    /// Compares two versions using the semantic versioning rules.
    /// </summary>
    /// <param name="left">One of the versions to compare.</param>
    /// <param name="right">The other version to compare.</param>
    /// <returns>Whether version <paramref name="left"/> is bigger than <paramref name="right"/>.</returns>
    public static bool operator >(SemVer left, SemVer right) => Compare(left, right) > 0;

    /// <summary>
    /// Compares two versions using the semantic versioning rules.
    /// </summary>
    /// <param name="left">One of the versions to compare.</param>
    /// <param name="right">The other version to compare.</param>
    /// <returns>Whether version <paramref name="left"/> is smaller or equal to <paramref name="right"/>.</returns>
    public static bool operator <=(SemVer left, SemVer right) => Compare(left, right) <= 0;

    /// <summary>
    /// Compares two versions using the semantic versioning rules.
    /// </summary>
    /// <param name="left">One of the versions to compare.</param>
    /// <param name="right">The other version to compare.</param>
    /// <returns>Whether version <paramref name="left"/> is bigger or equal to <paramref name="right"/>.</returns>
    public static bool operator >=(SemVer left, SemVer right) => Compare(left, right) >= 0;

    /// <inheritdoc/>
    public int CompareTo(SemVer other)
    {
        return Compare(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(SemVer other)
    {
        return Compare(this, other) == 0;
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SemVer semver && Equals(semver);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, Prerelease, BuildMetadata);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(Prerelease) && string.IsNullOrEmpty(BuildMetadata))
        {
            // Fast path: formatting only the numeric components (at most 3 Int32s and 3 periods).
            Span<char> dest = stackalloc char[(3 * Int32NumberBufferLength) + 3];
            bool success = TryFormatVersionFast(dest, out int charsWritten);
            Debug.Assert(success);
            return dest[..charsWritten].ToString();
        }

        var sb = new StringBuilder();
        sb.Append(Major);
        sb.Append('.');
        sb.Append(Minor);
        sb.Append('.');
        sb.Append(Patch);
        if (!string.IsNullOrEmpty(Prerelease))
        {
            sb.Append('-');
            sb.Append(Prerelease);
        }
        if (!string.IsNullOrEmpty(BuildMetadata))
        {
            sb.Append('+');
            sb.Append(BuildMetadata);
        }
        return sb.ToString();
    }

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString();
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten)
    {
        return TryFormatCore(destination, out charsWritten);
    }

    /// <summary>
    /// Fast path formatting that only considers the numeric components of the version.
    /// </summary>
    private bool TryFormatVersionFast(Span<char> destination, out int charsWritten)
    {
        int totalCharsWritten = 0;

        for (int i = 0; i < 3; i++)
        {
            if (i != 0)
            {
                if (destination.IsEmpty)
                {
                    charsWritten = 0;
                    return false;
                }

                destination[0] = '.';
                destination = destination[1..];
                totalCharsWritten++;
            }

            int value = i switch
            {
                0 => Major,
                1 => Minor,
                _ => Patch,
            };

            if (!TryFormatNumericComponent(value, destination, out int valueCharsWritten))
            {
                charsWritten = 0;
                return false;
            }

            totalCharsWritten += valueCharsWritten;
            destination = destination[valueCharsWritten..];
        }

        charsWritten = totalCharsWritten;
        return true;
    }

    private bool TryFormatCore(Span<char> destination, out int charsWritten)
    {
        if (!TryFormatVersionFast(destination, out int totalCharsWritten))
        {
            charsWritten = 0;
            return false;
        }

        if (string.IsNullOrEmpty(Prerelease) && string.IsNullOrEmpty(BuildMetadata))
        {
            charsWritten = totalCharsWritten;
            return true;
        }

        destination = destination[totalCharsWritten..];

        if (!string.IsNullOrEmpty(Prerelease))
        {
            destination[0] = '-';
            destination = destination[1..];
            totalCharsWritten++;

            if (!Prerelease.TryCopyTo(destination))
            {
                charsWritten = 0;
                return false;
            }

            destination = destination[Prerelease.Length..];
            totalCharsWritten += Prerelease.Length;
        }

        if (!string.IsNullOrEmpty(BuildMetadata))
        {
            destination[0] = '+';
            destination = destination[1..];
            totalCharsWritten++;

            if (!BuildMetadata.TryCopyTo(destination))
            {
                charsWritten = 0;
                return false;
            }

            totalCharsWritten += BuildMetadata.Length;
        }

        charsWritten = totalCharsWritten;
        return true;
    }

    private static bool TryFormatNumericComponent(int value, Span<char> destination, out int charsWritten)
    {
        return ((uint)value).TryFormat(destination, out charsWritten, provider: CultureInfo.InvariantCulture);
    }

    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return TryFormatCore(destination, out charsWritten);
    }

    /// <inheritdoc/>
    public static SemVer Parse(string input)
    {
        TryParseCore(input, throwOnFailure: true, out var result);
        return result;
    }

    /// <inheritdoc/>
    public static SemVer Parse(ReadOnlySpan<char> input)
    {
        TryParseCore(input, throwOnFailure: true, out var result);
        return result;
    }

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? input, out SemVer result)
    {
        return TryParseCore(input, throwOnFailure: false, out result);
    }

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> input, out SemVer result)
    {
        return TryParseCore(input, throwOnFailure: false, out result);
    }

    private static bool TryParseCore(ReadOnlySpan<char> input, bool throwOnFailure, out SemVer result)
    {
        var match = SemVerRegex.Match(input.ToString());

        if (!match.Success)
        {
            if (throwOnFailure)
            {
                throw new FormatException(SR.FormatFormat_InvalidStringWithSemVer(input.ToString()));
            }

            result = default;
            return false;
        }

        if (!TryParseNumericComponent(match.Groups["major"].ValueSpan, nameof(input), throwOnFailure, out int major))
        {
            result = default;
            return false;
        }

        if (!TryParseNumericComponent(match.Groups["minor"].ValueSpan, nameof(input), throwOnFailure, out int minor))
        {
            result = default;
            return false;
        }

        if (!TryParseNumericComponent(match.Groups["patch"].ValueSpan, nameof(input), throwOnFailure, out int patch))
        {
            result = default;
            return false;
        }

        string prerelease = match.Groups["prerelease"].Value;
        string buildmetadata = match.Groups["buildmetadata"].Value;

        result = new SemVer(major, minor, patch, prerelease, buildmetadata);
        return true;
    }

    private static bool TryParseNumericComponent(ReadOnlySpan<char> component, string componentName, bool throwOnFailure, out int parsedComponent)
    {
        if (throwOnFailure)
        {
            parsedComponent = int.Parse(component, NumberStyles.Integer, CultureInfo.InvariantCulture);
            ArgumentOutOfRangeException.ThrowIfNegative(parsedComponent, componentName);
            return true;
        }

        return int.TryParse(component, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedComponent) && parsedComponent >= 0;
    }

    static SemVer IParsable<SemVer>.Parse(string s, IFormatProvider? provider)
    {
        TryParseCore(s, throwOnFailure: true, out var result);
        return result;
    }

    static bool IParsable<SemVer>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out SemVer result)
    {
        return TryParseCore(s, throwOnFailure: false, out result);
    }

    static SemVer ISpanParsable<SemVer>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        TryParseCore(s, throwOnFailure: true, out var result);
        return result;
    }

    static bool ISpanParsable<SemVer>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out SemVer result)
    {
        return TryParseCore(s, throwOnFailure: false, out result);
    }
}

using System;
using System.Buffers;

namespace Godot.Common;

internal ref struct SpanPascalCaseEnumerator
{
    private static readonly SearchValues<char> _lowercaseChars = SearchValues.Create("abcdefghijklmnopqrstuvwxyz");
    private static readonly SearchValues<char> _uppercaseChars = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
    private static readonly SearchValues<char> _numberChars = SearchValues.Create("0123456789");

    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;
    private bool _isEnumeratorActive;

    internal SpanPascalCaseEnumerator(ReadOnlySpan<char> buffer)
    {
        _remaining = buffer;
        _current = default;
        _isEnumeratorActive = true;
    }

    public readonly ReadOnlySpan<char> Current => _current;

    public readonly SpanPascalCaseEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        if (!_isEnumeratorActive)
        {
            // EOF previously reached or enumerator was never initialized.
            return false;
        }

        ReadOnlySpan<char> remaining = _remaining;

        // To find the next uppercase or lowercase character we use IndexOfAnyExcept
        // and the opposite set of characters so that characters outside the set are
        // considered part delimiters (e.g.: underscores).

        if (char.IsUpper(remaining[0]))
        {
            // Find the next uppercase character.
            int idxUpper = remaining[1..].IndexOfAnyExcept(_lowercaseChars);
            if (idxUpper == 0)
            {
                // The next character is still uppercase, so find the next non-uppercase character instead.
                int idxLower = remaining[1..].IndexOfAnyExcept(_uppercaseChars);
                if ((uint)idxLower < (uint)remaining.Length)
                {
                    // A delimiter character was found.
                    if (char.IsLower(remaining[idxLower + 1]))
                    {
                        // Here we don't add 1 to the index because we want the last uppercase character
                        // to be included in the next part, not in the current one.
                        _current = remaining[..idxLower];
                        _remaining = remaining[idxLower..];
                    }
                    else
                    {
                        _current = remaining[..(idxLower + 1)];
                        _remaining = remaining[(idxLower + 1)..];
                    }
                    return true;
                }
            }
            else if ((uint)idxUpper < (uint)remaining.Length)
            {
                // A delimiter character was found and it's not contiguous.
                _current = remaining[..(idxUpper + 1)];
                _remaining = remaining[(idxUpper + 1)..];
                return true;
            }
        }
        else if (char.IsLower(remaining[0]))
        {
            // Find the next non-lowercase character.
            int idxUpper = remaining[1..].IndexOfAnyExcept(_lowercaseChars);
            if ((uint)idxUpper < (uint)remaining.Length)
            {
                // A delimiter character was found.
                _current = remaining[..(idxUpper + 1)];
                _remaining = remaining[(idxUpper + 1)..];
                return true;
            }
        }
        else if (char.IsDigit(remaining[0]))
        {
            int idxNumber = remaining[1..].IndexOfAnyExcept(_numberChars);
            if ((uint)idxNumber < (uint)remaining.Length)
            {
                // A delimiter character was found.
                _current = remaining[..(idxNumber + 1)];
                _remaining = remaining[(idxNumber + 1)..];
                return true;
            }
        }
        else
        {
            // Unknown character, consider it a separate part.
            if (remaining.Length > 1)
            {
                _current = remaining[..1];
                _remaining = remaining[1..];
                return true;
            }
        }

        // We've reached EOF, but we still need to return 'true' for this final
        // iteration so that the caller can query the Current property once more.
        _current = remaining;
        _remaining = default;
        _isEnumeratorActive = false;
        return true;
    }
}

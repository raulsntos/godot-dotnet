using System;

namespace Godot.Common;

internal ref struct SpanSnakeCaseEnumerator
{
    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;
    private bool _isEnumeratorActive;

    internal SpanSnakeCaseEnumerator(ReadOnlySpan<char> buffer)
    {
        _remaining = buffer;
        _current = default;
        _isEnumeratorActive = true;
    }

    public readonly ReadOnlySpan<char> Current => _current;

    public readonly SpanSnakeCaseEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        if (!_isEnumeratorActive)
        {
            // EOF previously reached or enumerator was never initialized.
            return false;
        }

        ReadOnlySpan<char> remaining = _remaining;

        int idx = remaining.IndexOf('_');
        if ((uint)idx < (uint)remaining.Length)
        {
            // We don't add 1 to the index when slicing current because we want
            // to avoid including the underscore in the slice, but in the remaining
            // slice we want to skip it.
            _current = remaining[..idx];
            _remaining = remaining[(idx + 1)..];
            return true;
        }

        // We've reached EOF, but we still need to return 'true' for this final
        // iteration so that the caller can query the Current property once more.
        _current = remaining;
        _remaining = default;
        _isEnumeratorActive = false;
        return true;
    }
}

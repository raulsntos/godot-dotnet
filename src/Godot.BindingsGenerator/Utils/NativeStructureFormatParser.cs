using System;
using System.Globalization;

namespace Godot.BindingsGenerator;

internal sealed class NativeStructureFormatParser
{
    internal static NativeStructureFormatFieldEnumerator EnumerateFields(string format)
    {
        return new NativeStructureFormatFieldEnumerator(format);
    }

    internal readonly ref struct NativeStructureField
    {
        public ReadOnlySpan<char> Name { get; init; }

        public ReadOnlySpan<char> Type { get; init; }

        public bool HasDefaultValue { get; init; }

        public ReadOnlySpan<char> DefaultValue { get; init; }

        public bool IsArray => ArrayLength > 0;

        // If the type is an array, this contains the fixed size of the array.
        public int ArrayLength { get; init; }
    }

    internal ref struct NativeStructureFormatFieldEnumerator
    {
        private ReadOnlySpan<char> _remaining;
        private NativeStructureField _current;
        private bool _isEnumeratorActive;

        public NativeStructureFormatFieldEnumerator(ReadOnlySpan<char> buffer)
        {
            _remaining = buffer;
            _current = default;
            _isEnumeratorActive = true;
        }

        public readonly NativeStructureField Current => _current;

        public readonly NativeStructureFormatFieldEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (!_isEnumeratorActive)
            {
                // EOF previously reached or enumerator was never initialized.
                return false;
            }

            ReadOnlySpan<char> remaining = _remaining;

            int idx = remaining.IndexOf(';');
            if ((uint)idx < (uint)remaining.Length)
            {
                // We don't add 1 to the index when slicing current because we want
                // to avoid including the underscore in the slice, but in the remaining
                // slice we want to skip it.
                _current = ParseField(remaining[..idx]);
                _remaining = remaining[(idx + 1)..];
                return true;
            }

            // We've reached EOF, but we still need to return 'true' for this final
            // iteration so that the caller can query the Current property once more.
            _current = ParseField(remaining);
            _remaining = default;
            _isEnumeratorActive = false;
            return true;
        }

        private static NativeStructureField ParseField(ReadOnlySpan<char> buffer)
        {
            ReadOnlySpan<char> typeName;
            ReadOnlySpan<char> fieldName;
            ReadOnlySpan<char> defaultValue = default;
            bool hasDefaultValue = false;
            int arrayLength = -1;

            // The first segment until the first space character is always the type name.
            {
                int idx = buffer.IndexOf(' ');
                if ((uint)idx >= (uint)buffer.Length)
                {
                    throw new InvalidOperationException($"Invalid native structure format, expected space character after type name (buffer: '{buffer}').");
                }

                if (buffer[idx + 1] == '*')
                {
                    // The type is a pointer so include this in the type segment.
                    typeName = buffer[..(idx + 2)];
                    buffer = buffer[(idx + 2)..];
                }
                else
                {
                    typeName = buffer[..idx];
                    buffer = buffer[(idx + 1)..];
                }
            }

            // The second segment is always the field name.
            {
                int idx = buffer.IndexOf(' ');
                if ((uint)idx >= (uint)buffer.Length)
                {
                    // If there's no space character after the field name,
                    // then this is the last segment in this field.
                    fieldName = buffer;
                    buffer = default;
                }
                else
                {
                    fieldName = buffer[..idx];
                    buffer = buffer[(idx + 1)..];
                }

                if (fieldName[^1] == ']')
                {
                    // Array types include the fixed size after the field name.
                    idx = fieldName.LastIndexOf('[');
                    if ((uint)idx >= (uint)fieldName.Length)
                    {
                        throw new InvalidOperationException($"Invalid native structure format, expected array size after field name (buffer: '({typeName}) {fieldName}')");
                    }

                    ReadOnlySpan<char> arraySizeSpan = fieldName[(idx + 1)..^1];
                    fieldName = fieldName[..idx];

                    arrayLength = int.Parse(arraySizeSpan, CultureInfo.InvariantCulture);
                }
            }

            // The field may have a default value.
            if (!buffer.IsEmpty)
            {
                if (!buffer.StartsWith("= "))
                {
                    throw new InvalidOperationException($"Invalid native structure format, expected default value after field name (buffer: '{buffer}').");
                }

                defaultValue = buffer["= ".Length..];
                if (defaultValue.IsEmpty)
                {
                    throw new InvalidOperationException($"Invalid native structure format, expected default value after field name (buffer: '{buffer}').");
                }

                hasDefaultValue = true;
            }

            return new NativeStructureField()
            {
                Type = typeName,
                Name = fieldName,
                DefaultValue = defaultValue,
                HasDefaultValue = hasDefaultValue,
                ArrayLength = arrayLength,
            };
        }
    }
}

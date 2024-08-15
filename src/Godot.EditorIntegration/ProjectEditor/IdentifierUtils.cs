using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Godot.EditorIntegration.ProjectEditor;

internal static class IdentifierUtils
{
    public static string SanitizeQualifiedIdentifier(string qualifiedIdentifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(qualifiedIdentifier);

        string[] identifiers = qualifiedIdentifier.Split('.');

        for (int i = 0; i < identifiers.Length; i++)
        {
            string identifier = SanitizeIdentifier(identifiers[i]);
            if (string.IsNullOrEmpty(identifier))
            {
                // Default name for empty identifiers.
                identifier = "Empty";
            }
            identifiers[i] = identifier;
        }

        return string.Join(".", identifiers);
    }

    /// <summary>
    /// Skips invalid identifier characters including decimal digit numbers at the start of the identifier.
    /// </summary>
    private static void SkipInvalidCharacters(ReadOnlySpan<char> source, StringBuilder outputBuilder)
    {
        foreach (char @char in source)
        {
            switch (char.GetUnicodeCategory(@char))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.OtherLetter:
                    outputBuilder.Append(@char);
                    break;

                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DecimalDigitNumber:
                    // Identifiers may start with underscore.
                    if (outputBuilder.Length > 0 || @char == '_')
                    {
                        outputBuilder.Append(@char);
                    }
                    break;
            }
        }
    }

    private static string SanitizeIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> identifierSpan = identifier;
        if (identifier.Length > 511)
        {
            identifierSpan = identifierSpan[..511];
        }

        var identifierBuilder = new StringBuilder();

        if (identifierSpan[0] == '@')
        {
            identifierBuilder.Append('@');
            identifierSpan = identifierSpan[1..];
        }

        SkipInvalidCharacters(identifierSpan, identifierBuilder);

        if (identifierBuilder.Length == 0)
        {
            // All characters were invalid so now it's empty.
            return string.Empty;
        }

        identifier = identifierBuilder.ToString();

        if (identifier[0] != '@' && IsKeyword(identifier))
        {
            identifier = $"@{identifier}";
        }

        return identifier;
    }

    private static bool IsKeyword(string value)
    {
        // Identifiers that start with double underscore are meant to be used for reserved keywords.
        // Only existing keywords are enforced, but it may be useful to forbid any identifier
        // that begins with double underscore to prevent issues with future C# versions.
        if (value.StartsWith("__", StringComparison.Ordinal))
        {
            // Make an exception if the next character is also an underscore.
            if (!value.StartsWith("___", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return _keywords.Contains(value);
    }

    private static readonly HashSet<string> _keywords =
    [
        "as",
        "do",
        "if",
        "in",
        "is",
        "for",
        "int",
        "new",
        "out",
        "ref",
        "try",
        "base",
        "bool",
        "byte",
        "case",
        "char",
        "else",
        "enum",
        "goto",
        "lock",
        "long",
        "null",
        "this",
        "true",
        "uint",
        "void",
        "break",
        "catch",
        "class",
        "const",
        "event",
        "false",
        "fixed",
        "float",
        "sbyte",
        "short",
        "throw",
        "ulong",
        "using",
        "where",
        "while",
        "yield",
        "double",
        "extern",
        "object",
        "params",
        "public",
        "return",
        "sealed",
        "sizeof",
        "static",
        "string",
        "struct",
        "switch",
        "typeof",
        "unsafe",
        "ushort",
        "checked",
        "decimal",
        "default",
        "finally",
        "foreach",
        "partial",
        "private",
        "virtual",
        "abstract",
        "continue",
        "delegate",
        "explicit",
        "implicit",
        "internal",
        "operator",
        "override",
        "readonly",
        "volatile",
        "interface",
        "namespace",
        "protected",
        "unchecked",
        "stackalloc",
    ];
}

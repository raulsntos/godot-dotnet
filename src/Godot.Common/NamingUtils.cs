using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.Common;

/// <summary>
/// Helper methods to convert between the Godot's Core and .NET naming conventions.
/// </summary>
public static class NamingUtils
{
    private static readonly SearchValues<char> _numberSearchValues = SearchValues.Create("0123456789");

    // Hardcoded collection of PascalCase name conversions.
    private static readonly Dictionary<string, string> _pascalCaseNameOverrides = new()
    {
        { "BitMap", "Bitmap" },
        { "Environment", "GodotEnvironment" },
        { "JSONRPC", "JsonRpc" },
        { "Object", "GodotObject" },
        { "OpenXRIPBinding", "OpenXRIPBinding" },
        { "OpenXRIPBindingModifier", "OpenXRIPBindingModifier" },
        { "SkeletonModification2DCCDIK", "SkeletonModification2DCcdik" },
        { "SkeletonModification2DFABRIK", "SkeletonModification2DFabrik" },
        { "SkeletonModification3DCCDIK", "SkeletonModification3DCcdik" },
        { "SkeletonModification3DFABRIK", "SkeletonModification3DFabrik" },
        { "System", "System_" },
        { "Thread", "GodotThread" },
    };

    // Hardcoded collection of PascalCase part conversions.
    private static readonly Dictionary<string, string> _pascalCasePartOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        { "AA", "AA" }, // Anti Aliasing
        { "AO", "AO" }, // Ambient Occlusion
        { "FILENAME", "FileName" },
        { "FADEIN", "FadeIn" },
        { "FADEOUT", "FadeOut" },
        { "FX", "FX" },
        { "GI", "GI" }, // Global Illumination
        { "GZIP", "GZip" },
        { "HBOX", "HBox" }, // Horizontal Box
        { "ID", "Id" },
        { "IO", "IO" }, // Input/Output
        { "IOS", "IOS" }, // iOS
        { "IP", "IP" }, // Internet Protocol
        { "IV", "IV" }, // Initialization Vector
        { "MACOS", "MacOS" },
        { "METALFX", "MetalFX" },
        { "NODEPATH", "NodePath" },
        { "OPENXR", "OpenXR" },
        { "OS", "OS" }, // Operating System
        { "POSTPROCESSING", "PostProcessing" },
        { "PINGPONG", "PingPong" },
        { "SDFGIY", "SdfgiY" },
        { "SPIRV", "SpirV" },
        { "STDIN", "StdIn" },
        { "STDOUT", "StdOut" },
        { "USERNAME", "UserName" },
        { "UV", "UV" },
        { "UV2", "UV2" },
        { "VBOX", "VBox" }, // Vertical Box
        { "WHITESPACE", "WhiteSpace" },
        { "WM", "WM" },
        { "XR", "XR" },
        { "XRAPI", "XRApi" },
    };

    /// <summary>
    /// Converts a snake_case identifier to PascalCase, following .NET naming conventions.
    /// </summary>
    /// <remarks>
    /// This method is used to convert the names of methods, properties, enum constants,
    /// and field constants to PascalCase (e.g.: MY_CONSTANT -> MyConstant).
    /// </remarks>
    /// <param name="value">Identifier in snake_case format.</param>
    /// <returns>Identifier in PascalCase format.</returns>
    public static string SnakeToPascalCase(string value)
    {
        if (value.Length == 0)
        {
            return value;
        }

        return string.Create(value.Length, value, static (chars, value) =>
        {
            int index = 0;
            foreach (var part in new SpanSnakeCaseEnumerator(value))
            {
                if (index == 0 && part.IsEmpty)
                {
                    // If the first part is empty then the name starts with an underscore, preserve it.
                    chars[index++] = '_';
                    continue;
                }

                Debug.Assert(!part.IsEmpty, "A part can't be empty, the enumerator should have exited the foreach if we reached EOF.");

                if (_pascalCasePartOverrides.TryGetValue(part.ToString(), out string? partOverride))
                {
                    // Add the hardcoded part.
                    partOverride.CopyTo(chars[index..]);
                    index += partOverride.Length;
                    continue;
                }

                if (part.Length <= 2)
                {
                    // Acronym of length 1 or 2.
                    part.ToLowerInvariant(chars[index..]);
                    chars[index] = char.ToUpperInvariant(chars[index]);
                    index += part.Length;
                    continue;
                }

                // Add the part's first character as uppercase.
                chars[index++] = char.ToUpperInvariant(part[0]);
                // Add the rest of the part's characters as lowercase.
                index += part[1..].ToLowerInvariant(chars[index..]);
            }
            // Ensure every character after a digit is uppercase.
            {
                int numberIdx = -1;
                var remaining = chars;
                do
                {
                    numberIdx = remaining.IndexOfAny(_numberSearchValues);
                    if (numberIdx != -1 && remaining.Length > numberIdx + 1)
                    {
                        remaining[numberIdx + 1] = char.ToUpperInvariant(remaining[numberIdx + 1]);
                        remaining = remaining[(numberIdx + 1)..];
                    }
                    else
                    {
                        // We reached EOF.
                        remaining = default;
                    }
                } while (numberIdx != -1);
            }
        }).TrimEnd('\0');
    }

    /// <summary>
    /// Converts a snake_case identifier to camelCase, following .NET naming conventions.
    /// </summary>
    /// <remarks>
    /// This method is used to convert the names of method parameters to camelCase
    /// (e.g.: my_method_parameter -> myMethodParameter).
    /// </remarks>
    /// <param name="value">Identifier in snake_case format.</param>
    /// <returns>Identifier in camelCase format.</returns>
    public static string SnakeToCamelCase(string value)
    {
        if (value.Length == 0)
        {
            return value;
        }

        return string.Create(value.Length, value, static (chars, value) =>
        {
            int index = 0;
            foreach (var part in new SpanSnakeCaseEnumerator(value))
            {
                Debug.Assert(!part.IsEmpty, "A part can't be empty, the enumerator should have exited the foreach if we reached EOF.");

                if (_pascalCasePartOverrides.TryGetValue(part.ToString(), out string? partOverride))
                {
                    // Add the hardcoded part.
                    if (index == 0)
                    {
                        // This is the first part, so it should be all lowercase.
                        partOverride.AsSpan().ToLowerInvariant(chars);
                    }
                    else
                    {
                        partOverride.CopyTo(chars[index..]);
                    }
                    index += partOverride.Length;
                    continue;
                }

                if (part.Length <= 2)
                {
                    // Acronym of length 1 or 2.
                    part.ToLowerInvariant(chars[index..]);
                    if (index != 0)
                    {
                        chars[index] = char.ToUpperInvariant(chars[index]);
                    }
                    index += part.Length;
                    continue;
                }

                if (index == 0)
                {
                    // This is the first part, so it should be all lowercase.
                    index += part.ToLowerInvariant(chars);
                }
                else
                {
                    // Add the part's first character as uppercase.
                    chars[index++] = char.ToUpperInvariant(part[0]);
                    // Add the rest of the part's characters as lowercase.
                    index += part[1..].ToLowerInvariant(chars[index..]);
                }
            }
            // Ensure every character after a digit is uppercase.
            {
                int numberIdx = -1;
                var remaining = chars;
                do
                {
                    numberIdx = remaining.IndexOfAny(_numberSearchValues);
                    if (numberIdx != -1 && remaining.Length > numberIdx + 1)
                    {
                        remaining[numberIdx + 1] = char.ToUpperInvariant(remaining[numberIdx + 1]);
                        remaining = remaining[(numberIdx + 1)..];
                    }
                    else
                    {
                        // We reached EOF.
                        remaining = default;
                    }
                } while (numberIdx != -1);
            }
        }).TrimEnd('\0');
    }

    /// <summary>
    /// Converts a PascalCase identifier to PascalCase, following .NET naming conventions.
    /// </summary>
    /// <remarks>
    /// This method is used to convert the names of types to PascalCase
    /// (e.g.: HTTPRequest -> HttpRequest).
    /// </remarks>
    /// <param name="value">Identifier in PascalCase format.</param>
    /// <returns>Identifier in PascalCase format.</returns>
    public static string PascalToPascalCase(string value)
    {
        // Some special types include a dot in their name (e.g.: Variant.Type and Variant.Operator), remove it.
        if (value.Contains('.'))
        {
            value = value.Replace(".", "");
        }

        if (value.Length == 0)
        {
            return value;
        }

        if (value.Length <= 2)
        {
            // Acronyms of length 1 or 2 should be uppercase.
            return value.ToUpperInvariant();
        }

        if (_pascalCaseNameOverrides.TryGetValue(value, out string? nameOverride))
        {
            // Use hardcoded value for the identifier.
            return nameOverride;
        }

        return string.Create(value.Length, value, static (chars, value) =>
        {
            int index = 0;
            foreach (var part in new SpanPascalCaseEnumerator(value))
            {
                Debug.Assert(!part.IsEmpty, "A part can't be empty, the enumerator should have exited the foreach if we reached EOF.");

                if (_pascalCasePartOverrides.TryGetValue(part.ToString(), out string? partOverride))
                {
                    // Add the hardcoded part.
                    partOverride.CopyTo(chars[index..]);
                    index += partOverride.Length;
                    continue;
                }

                if (part.Length <= 2 && char.IsUpper(part[0]))
                {
                    // Acronym of length 1 or 2.
                    part.CopyTo(chars[index..]);
                    index += part.Length;
                    continue;
                }

                // Add the part's first character as uppercase.
                chars[index++] = char.ToUpperInvariant(part[0]);
                // Add the rest of the part's characters as lowercase.
                index += part[1..].ToLowerInvariant(chars[index..]);
            }
        });
    }

    /// <summary>
    /// Find the index of the part in the enum constant names where they stop sharing a common prefix.
    /// This is used to strip the common prefix from enum constant names.
    /// </summary>
    /// <remarks>
    /// Godot enum constants often share a common prefix that is redundant when the enum is used in C#
    /// (e.g.: IMAGE_FORMAT_PNG, IMAGE_FORMAT_JPEG -> ImageFormat.Png, ImageFormat.Jpeg).
    /// </remarks>
    /// <param name="engineEnum">The Godot enum information.</param>
    /// <returns>The index of the part where the enum constant names stop sharing a common prefix.</returns>
    /// <exception cref="ArgumentException">Enum contains no values.</exception>
    public static int DetermineEnumPrefix(GodotEnumInfo engineEnum)
    {
        if (engineEnum.Values.Length == 0)
        {
            throw new ArgumentException("Enum contains no values.", nameof(engineEnum));
        }

        if (engineEnum.Values.Length == 1)
        {
            // If there's only one value, extract the prefix from the name of the enum.

            int currentPart = 0;
            (int Start, int Length) referenceRange = default;
            string reference = engineEnum.Name.ToUpperInvariant();
            foreach (var part in new SpanPascalCaseEnumerator(engineEnum.Name))
            {
                // Since the enum name is PascalCase we need to maintain a separate range for the enum names
                // to account for the underscores (equal to the parts we've iterated so far).
                referenceRange = (referenceRange.Start + referenceRange.Length, part.Length);
                (int Start, int Length) currentRange = (referenceRange.Start + currentPart, part.Length);

                var referencePart = reference.AsSpan(referenceRange.Start, referenceRange.Length);
                if (!AreAllPartsEqualToReference(currentRange, referencePart, engineEnum))
                {
                    // We found the first part that doesn't match.
                    break;
                }

                currentPart++;
            }

            return currentPart;
        }

        {
            int currentPart = 0;
            (int Start, int Length) currentRange = default;
            foreach (var part in new SpanSnakeCaseEnumerator(engineEnum.Values[0].Name))
            {
                currentRange = (currentRange.Start + currentRange.Length, part.Length);

                // If this is not the first part, skip the underscore separator.
                if (currentPart != 0)
                {
                    currentRange.Start++;
                }

                if (!AreAllPartsEqual(currentRange, engineEnum))
                {
                    // We found the first part that doesn't match.

                    // HARDCODED: Some Flag enums have the prefix 'FLAG_' for everything except 'FLAGS_DEFAULT' (same for 'METHOD_FLAG_' and 'METHOD_FLAGS_DEFAULT').
                    if (part.SequenceEqual("FLAG") || part.SequenceEqual("FLAGS"))
                    {
                        currentPart++;
                        break;
                    }

                    break;
                }

                currentPart++;
            }

            return currentPart;
        }

        static bool AreAllPartsEqual((int Start, int Length) range, GodotEnumInfo @enum)
        {
            Debug.Assert(@enum.Values.Length > 1, "Enum must contain at least 2 constants.");

            string firstName = @enum.Values[0].Name;
            var firstNamePart = firstName.AsSpan(range.Start, range.Length);
            return AreAllPartsEqualToReference(range, firstNamePart, @enum);
        }

        static bool AreAllPartsEqualToReference((int Start, int Length) range, ReadOnlySpan<char> referencePart, GodotEnumInfo @enum)
        {
            foreach (var (currentName, _) in @enum.Values)
            {
                if (currentName.Length < range.Start || currentName.Length < range.Start + range.Length)
                {
                    // The current part's range doesn't fit this enum constant, so they can't be equal.
                    return false;
                }
                if (currentName.Length != range.Start + range.Length && currentName[range.Start + range.Length] != '_')
                {
                    // The range must go until the end of the name or the next character after the range must be
                    // an underscore, otherwise the part is bigger than the first name's part so they can't be equal.
                    return false;
                }

                var currentNamePart = currentName.AsSpan(range.Start, range.Length);
                if (!referencePart.SequenceEqual(currentNamePart))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Apply the prefix stripping to the enum constants. This modifies the enum in place.
    /// See <see cref="DetermineEnumPrefix"/> for more information.
    /// </summary>
    /// <param name="engineEnum">Godot enum information that is the source of truth.</param>
    /// <param name="enum">C# enum information that will be modified.</param>
    /// <param name="prefixLength">The parts of the name to strip from the beginning.</param>
    public static void ApplyPrefixToEnumConstants(GodotEnumInfo engineEnum, EnumInfo @enum, int prefixLength)
    {
        if (prefixLength <= 0)
        {
            return;
        }

        for (int i = 0; i < engineEnum.Values.Length; i++)
        {
            var (name, value) = engineEnum.Values[i];
            int currentPrefixLength = prefixLength;

            string[] parts = name.Split('_');

            if (parts.Length <= currentPrefixLength)
            {
                continue;
            }

            if (char.IsDigit(parts[currentPrefixLength][0]))
            {
                // The name of enum constants may begin with a numeric digit when strip from the enum prefix,
                // so we make the prefix for this constant one word shorter in those cases.
                for (--currentPrefixLength; currentPrefixLength > 0; currentPrefixLength--)
                {
                    if (!char.IsDigit(parts[currentPrefixLength][0]))
                    {
                        break;
                    }
                }
            }

            StringBuilder nameBuilder = new();
            for (int j = currentPrefixLength; j < parts.Length; j++)
            {
                if (j > currentPrefixLength)
                {
                    nameBuilder.Append('_');
                }
                nameBuilder.Append(parts[j]);
            }

            @enum.Values[i] = (SnakeToPascalCase(nameBuilder.ToString()), value);
        }
    }

    /// <summary>
    /// Remove the max constant from an enum. This modifies the enum in place.
    /// </summary>
    /// <remarks>
    /// Godot enums sometimes contain a max constant that represents the number
    /// of elements in an enum. These are unnecessary in C# and can break
    /// compatibility when their values change between Godot versions.
    /// </remarks>
    /// <param name="engineEnum">Godot enum information that is the source of truth.</param>
    /// <param name="enum">C# enum information that will be modified.</param>
    public static void RemoveMaxConstant(GodotEnumInfo engineEnum, EnumInfo @enum)
    {
        int maxEnumFieldIndex = 0;
        GodotEnumValueInfo? maxEnumField = null;

        // Look for the enum field that has the highest value and matches the naming pattern.
        for (int i = 0; i < engineEnum.Values.Length; i++)
        {
            GodotEnumValueInfo? enumField = engineEnum.Values[i];

            if (maxEnumField is null || enumField.Value > maxEnumField.Value)
            {
                if (enumField.Name.EndsWith("_MAX", StringComparison.Ordinal)
                 || enumField.Name.EndsWith("_ENUM_SIZE", StringComparison.Ordinal))
                {
                    maxEnumField = enumField;
                    maxEnumFieldIndex = i;
                }
            }
        }

        if (maxEnumField is not null)
        {
            @enum.Values.RemoveAt(maxEnumFieldIndex);
        }
    }
}

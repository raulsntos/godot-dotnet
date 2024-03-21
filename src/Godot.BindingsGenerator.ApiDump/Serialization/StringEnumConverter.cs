// This is a temporary converter created in order to support custom enum value names
// while this is unsupported upstream.
// See: https://github.com/dotnet/runtime/issues/74385

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Godot.BindingsGenerator.ApiDump.Serialization;

/// <summary>
/// Converter that deserializes enum values from strings using the name specified
/// by <see cref="JsonPropertyNameAttribute"/>.
/// </summary>
/// <typeparam name="TEnum">The type of the enum handled by the converter.</typeparam>
internal sealed class StringEnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<string, TEnum> _mappedValues = [];

    public StringEnumConverter()
    {
        foreach (var value in Enum.GetValues<TEnum>())
        {
            string enumMemberName = value.ToString();
            var enumMember = typeof(TEnum).GetField(enumMemberName)!;

            var attr = enumMember.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (attr is not null)
            {
                enumMemberName = attr.Name;
            }

            _mappedValues.Add(enumMemberName, value);
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string, found '{reader.TokenType}'.");
        }

        string? enumMemberName = reader.GetString();
        if (string.IsNullOrEmpty(enumMemberName) || !_mappedValues.TryGetValue(enumMemberName, out TEnum value))
        {
            throw new JsonException($"String '{enumMemberName}' does not correspond to any of the known values of enum '{typeof(TEnum)}'.");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        // We never serialize so we don't need to implement writing.
        throw new NotImplementedException();
    }
}

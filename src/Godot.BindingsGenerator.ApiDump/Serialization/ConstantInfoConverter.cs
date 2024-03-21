using System;
using System.Globalization;

namespace Godot.BindingsGenerator.ApiDump.Serialization;

/// <summary>
/// Custom JSON converter to handle <see cref="GodotConstantInfo"/>.
/// Godot global constants only contain a numeric value for <see cref="GodotConstantInfo.Value"/>
/// and don't specify <see cref="GodotConstantInfo.Type"/> because it's implied to be int.
/// Godot engine classes constants contain a string value for <see cref="GodotConstantInfo.Value"/>
/// and specify their type using <see cref="GodotConstantInfo.Type"/>.
/// We want to handle both kinds of constants using the same type, so this converter
/// is able to deserialize either of them to a <see cref="GodotConstantInfo"/>.
/// </summary>
internal sealed class ConstantInfoConverter : JsonConverter<GodotConstantInfo>
{
    public override GodotConstantInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected object, found '{reader.TokenType}'.");
        }

        string? constantName = null;
        string? valueString = null;
        string? typeName = null;
        bool valueIsInteger = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected object property, found '{reader.TokenType}'.");
            }

            string propertyName = reader.GetString() ?? string.Empty;
            switch (propertyName)
            {
                case "name":
                    reader.Read();
                    constantName = reader.GetString();
                    break;

                case "type":
                    reader.Read();
                    typeName = reader.GetString();
                    break;

                case "value":
                    reader.Read();
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.Number:
                            valueString = reader.GetInt32().ToString(CultureInfo.InvariantCulture);
                            valueIsInteger = true;
                            break;
                        case JsonTokenType.String:
                            valueString = reader.GetString();
                            break;
                        default:
                            throw new JsonException($"Unexpected constant value, found '{reader.TokenType}'.");
                    }
                    break;

                default:
                    throw new JsonException($"Unexpected property found: '{reader.GetString()}'");
            }
        }

        // When the value is an integer, the type must not be set.
        if (valueIsInteger)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                typeName = "int";
            }
            else
            {
                throw new JsonException("Unexpected format for constant, the value was read as an integer but the type was declared.");
            }
        }

        if (string.IsNullOrEmpty(constantName))
        {
            throw new JsonException($"JSON deserialization for type '{typeof(GodotConstantInfo)}' was missing required properties, including the following: name.");
        }
        if (string.IsNullOrEmpty(typeName))
        {
            throw new JsonException($"JSON deserialization for type '{typeof(GodotConstantInfo)}' was missing required properties, including the following: type.");
        }
        if (string.IsNullOrEmpty(valueString))
        {
            throw new JsonException($"JSON deserialization for type '{typeof(GodotConstantInfo)}' was missing required properties, including the following: value.");
        }

        return new GodotConstantInfo()
        {
            Name = constantName,
            Type = typeName,
            Value = valueString,
        };
    }

    public override void Write(Utf8JsonWriter writer, GodotConstantInfo value, JsonSerializerOptions options)
    {
        // We never serialize so we don't need to implement writing.
        throw new NotImplementedException();
    }
}

using System;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class GodotDictionaryDefaultValueParser : DefaultValueParser
{
    private static readonly TypeInfo[] _compatibleTypes =
    [
        KnownTypes.GodotDictionary,
        KnownTypes.GodotDictionaryGeneric,
        KnownTypes.NativeGodotDictionary,
    ];

    private readonly TypeInfo _type;

    public GodotDictionaryDefaultValueParser(TypeInfo type)
    {
        if (!_compatibleTypes.Contains(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' is not compatible with {nameof(GodotDictionaryDefaultValueParser)}.", nameof(type));
        }

        _type = type;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (engineDefaultValue == "{}")
        {
            // The marshaller will take care of marshalling null dictionaries
            // as empty dictionaries so we can avoid creating a new instance.
            return "default";
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

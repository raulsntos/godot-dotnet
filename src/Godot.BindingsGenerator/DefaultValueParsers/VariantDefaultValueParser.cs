using System;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class VariantDefaultValueParser : DefaultValueParser
{
    private static readonly TypeInfo[] _compatibleTypes =
    [
        KnownTypes.GodotVariant,
        KnownTypes.NativeGodotVariant,
    ];

    private readonly TypeInfo _type;

    public VariantDefaultValueParser(TypeInfo type)
    {
        if (!_compatibleTypes.Contains(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' is not compatible with {nameof(VariantDefaultValueParser)}.", nameof(type));
        }

        _type = type;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (engineDefaultValue == "null" || engineDefaultValue == "Variant()")
        {
            return "default";
        }

        // TODO: This should be fixed upstream.
        // HARDCODED: Fix some invalid default values in the bindings.
        // https://github.com/godotengine/godot/pull/84906
        {
            // `CodeEdit::add_code_completion_option` has bound `Variant::NIL` to a Variant parameter,
            // instead of `Variant()`.
            if (engineDefaultValue == "0")
            {
                return "default";
            }
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

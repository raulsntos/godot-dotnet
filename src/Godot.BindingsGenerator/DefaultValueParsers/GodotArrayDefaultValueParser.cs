using System;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class GodotArrayDefaultValueParser : DefaultValueParser
{
    private static readonly TypeInfo[] _compatibleTypes =
    [
        KnownTypes.GodotArray,
        KnownTypes.GodotArrayGeneric,
        KnownTypes.NativeGodotArray,
    ];

    private readonly TypeInfo _type;

    public GodotArrayDefaultValueParser(TypeInfo type)
    {
        if (!_compatibleTypes.Contains(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' is not compatible with {nameof(GodotArrayDefaultValueParser)}.", nameof(type));
        }

        _type = type;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (engineDefaultValue == "[]")
        {
            // The marshaller will take care of marshalling null arrays
            // as empty arrays so we can avoid creating a new instance.
            return "default";
        }

        if (TryParseDefaultExpressionAsConstructor(engineDefaultValue, out string? engineTypeName, out string[]? constructorArguments))
        {
            if (engineTypeName.StartsWith("Array[", StringComparison.Ordinal)
                && engineTypeName.EndsWith(']'))
            {
                // It's a generic array.
                if (constructorArguments.Length == 0 || constructorArguments is ["[]"])
                {
                    // The marshaller will take care of marshalling null arrays
                    // as empty arrays so we can avoid creating a new instance.
                    return "default";
                }
            }
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

using System;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class PackedArrayDefaultValueParser : DefaultValueParser
{
    private readonly TypeInfo _type;

    public PackedArrayDefaultValueParser(TypeInfo type)
    {
        _type = type;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (TryParseDefaultExpressionAsConstructor(engineDefaultValue, out string? engineTypeName, out string[]? constructorArguments))
        {
            if (TypeDB.IsTypePackedArray(engineTypeName))
            {
                if (constructorArguments.Length == 0)
                {
                    return _type.IsByRefLike ? "default" : "[]";
                }
            }
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

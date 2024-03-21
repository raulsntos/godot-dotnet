using System;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class NodePathDefaultValueParser : DefaultValueParser
{
    private static readonly TypeInfo[] _compatibleTypes =
    [
        KnownTypes.GodotNodePath, KnownTypes.NativeGodotNodePath,
    ];

    private readonly TypeInfo _type;

    public NodePathDefaultValueParser(TypeInfo type)
    {
        if (!_compatibleTypes.Contains(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' is not compatible with {nameof(NodePathDefaultValueParser)}.", nameof(type));
        }

        _type = type;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        var valueSpan = engineDefaultValue.AsSpan();
        if (valueSpan.StartsWith("NodePath(") && valueSpan[^1] == ')')
        {
            // Extract string literal from NodePath constructor.
            valueSpan = valueSpan["NodePath(".Length..^1];

            if (valueSpan.SequenceEqual("\"\""))
            {
                return _type.IsByRefLike
                    ? "default"
                    : $"{_type.FullNameWithGlobal}.Empty";
            }

            return _type.IsByRefLike
                 ? $"{_type.FullNameWithGlobal}.Create({valueSpan})"
                 : $"new {_type.FullNameWithGlobal}({valueSpan})";
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

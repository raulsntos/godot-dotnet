using System;
using System.Collections.Generic;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class TransformDefaultValueParser : DefaultValueParser
{
    private static readonly TypeInfo[] _compatibleTypes =
    [
        KnownTypes.GodotTransform2D, KnownTypes.GodotTransform3D,
    ];

    private static readonly Dictionary<TypeInfo, string> _identities = new()
    {
        [KnownTypes.GodotTransform2D] = "Transform2D(1, 0, 0, 1, 0, 0)",
        [KnownTypes.GodotTransform3D] = "Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0)",
    };

    private readonly TypeInfo _type;

    public TransformDefaultValueParser(TypeInfo type)
    {
        if (!_compatibleTypes.Contains(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' is not compatible with {nameof(TransformDefaultValueParser)}.", nameof(type));
        }

        _type = type;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (engineDefaultValue == _identities[_type])
        {
            return $"{_type.FullNameWithGlobal}.Identity";
        }

        if (TryParseDefaultExpressionAsConstructor(engineDefaultValue, out string? engineTypeName, out _))
        {
            if (engineTypeName == _type.Name)
            {
                throw new NotImplementedException();
            }
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class VectorDefaultValueParser : DefaultValueParser
{
    private readonly TypeInfo _type;
    private readonly TypeInfo _componentType;
    private readonly int _componentCount;
    private readonly string _engineTypeName;

    public VectorDefaultValueParser(TypeInfo type)
    {
        if (type == KnownTypes.GodotVector2)
        {
            _engineTypeName = "Vector2";
            _type = type;
            _componentType = KnownTypes.SystemSingle;
            _componentCount = 2;
        }
        else if (type == KnownTypes.GodotVector2I)
        {
            _engineTypeName = "Vector2i";
            _type = type;
            _componentType = KnownTypes.SystemInt32;
            _componentCount = 2;
        }
        else if (type == KnownTypes.GodotVector3)
        {
            _engineTypeName = "Vector3";
            _type = type;
            _componentType = KnownTypes.SystemSingle;
            _componentCount = 3;
        }
        else if (type == KnownTypes.GodotVector3I)
        {
            _engineTypeName = "Vector3i";
            _type = type;
            _componentType = KnownTypes.SystemInt32;
            _componentCount = 3;
        }
        else if (type == KnownTypes.GodotVector4)
        {
            _engineTypeName = "Vector4";
            _type = type;
            _componentType = KnownTypes.SystemSingle;
            _componentCount = 4;
        }
        else if (type == KnownTypes.GodotVector4I)
        {
            _engineTypeName = "Vector4i";
            _type = type;
            _componentType = KnownTypes.SystemInt32;
            _componentCount = 4;
        }
        else if (type == KnownTypes.GodotColor)
        {
            _engineTypeName = "Color";
            _type = type;
            _componentType = KnownTypes.SystemSingle;
            _componentCount = 4;
        }
        else if (type == KnownTypes.GodotRect2)
        {
            _engineTypeName = "Rect2";
            _type = type;
            _componentType = KnownTypes.SystemSingle;
            _componentCount = 4;
        }
        else if (type == KnownTypes.GodotRect2I)
        {
            _engineTypeName = "Rect2i";
            _type = type;
            _componentType = KnownTypes.SystemInt32;
            _componentCount = 4;
        }
        else
        {
            throw new ArgumentException($"Type '{type.FullName}' is not compatible with {nameof(VectorDefaultValueParser)}.", nameof(type));
        }
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (TryParseDefaultExpressionAsConstructor(engineDefaultValue, out string? constructorTypeName, out string[]? constructorArguments))
        {
            if (_engineTypeName == constructorTypeName && constructorArguments.Length == _componentCount)
            {
                if (constructorArguments.All(arg => arg == "0"))
                {
                    return "default";
                }

                List<string> componentArguments = [];

                for (int i = 0; i < _componentCount; i++)
                {
                    if (!TryGetDefaultValueExpression(_componentType, constructorArguments[i], out string? componentValue))
                    {
                        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
                    }

                    componentArguments.Add(componentValue);
                }
                if (componentArguments.Count == _componentCount)
                {
                    return $"new {_type.FullNameWithGlobal}({string.Join(", ", componentArguments)})";
                }
            }
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

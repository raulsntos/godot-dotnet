using System;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class StringDefaultValueParser : DefaultValueParser
{
    private static readonly TypeInfo[] _compatibleTypes =
    [
        KnownTypes.SystemString, KnownTypes.GodotStringName,
        KnownTypes.NativeGodotString, KnownTypes.NativeGodotStringName,
    ];

    private readonly TypeInfo _type;

    public StringDefaultValueParser(TypeInfo type)
    {
        if (!_compatibleTypes.Contains(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' is not compatible with {nameof(StringDefaultValueParser)}. Use a string-like type.", nameof(type));
        }

        _type = type;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (engineDefaultValue == "\"\"")
        {
            if (_type == KnownTypes.SystemString)
            {
                // string.Empty can't be used as a parameter because it's not constant.
                return "\"\"";
            }

            return _type.IsByRefLike ? "default" : $"{_type.FullNameWithGlobal}.Empty";
        }

        bool isStringName = _type == KnownTypes.GodotStringName || _type == KnownTypes.NativeGodotStringName;

        if (isStringName && engineDefaultValue == "&\"\"")
        {
            return _type.IsByRefLike ? "default" : $"{_type.FullNameWithGlobal}.Empty";
        }

        var valueSpan = engineDefaultValue.AsSpan();
        if (isStringName && valueSpan[0] == '&')
        {
            // Remove StringName prefix.
            valueSpan = valueSpan[1..];
        }

        if (valueSpan[0] == '"' && valueSpan[^1] == '"')
        {
            if (_type == KnownTypes.SystemString)
            {
                return engineDefaultValue;
            }
            else if (_type == KnownTypes.GodotStringName)
            {
                return $"{_type.FullNameWithGlobal}.CreateFromUtf8({valueSpan}u8)";
            }
            else if (_type == KnownTypes.NativeGodotString
                  || _type == KnownTypes.NativeGodotStringName)
            {
                return $"{_type.FullNameWithGlobal}.Create({valueSpan}u8)";
            }
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{_type.FullName}'.", nameof(engineDefaultValue));
    }
}

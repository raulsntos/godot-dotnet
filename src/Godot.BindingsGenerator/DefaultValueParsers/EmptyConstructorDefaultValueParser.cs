using System;

namespace Godot.BindingsGenerator;

/// <summary>
/// Basic parser that only handles parameterless constructors.
/// </summary>
internal sealed class EmptyConstructorDefaultValueParser : DefaultValueParser
{
    private readonly string _engineTypeName;

    public EmptyConstructorDefaultValueParser(string engineTypeName)
    {
        _engineTypeName = engineTypeName;
    }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (engineDefaultValue == $"{_engineTypeName}()")
        {
            return "default";
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{KnownTypes.GodotRid.FullName}'.", nameof(engineDefaultValue));
    }
}

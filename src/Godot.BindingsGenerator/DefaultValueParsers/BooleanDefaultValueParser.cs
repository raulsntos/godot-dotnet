using System;

namespace Godot.BindingsGenerator;

internal sealed class BooleanDefaultValueParser : DefaultValueParser
{
    public static BooleanDefaultValueParser Instance { get; } = new();

    private BooleanDefaultValueParser() { }

    protected override string ParseCore(string engineDefaultValue)
    {
        if (engineDefaultValue == "true" || engineDefaultValue == "false")
        {
            return engineDefaultValue;
        }

        throw new ArgumentException($"Value '{engineDefaultValue}' can't be used as a default value for '{KnownTypes.SystemBoolean.FullName}'.", nameof(engineDefaultValue));
    }
}

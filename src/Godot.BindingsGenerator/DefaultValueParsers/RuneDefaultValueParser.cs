using System.Globalization;

namespace Godot.BindingsGenerator;

internal sealed class RuneDefaultValueParser : DefaultValueParser
{
    public static RuneDefaultValueParser Instance { get; } = new();

    private RuneDefaultValueParser() { }

    protected override string ParseCore(string engineDefaultValue)
    {
        int value = int.Parse(engineDefaultValue, CultureInfo.InvariantCulture);
        return $"(global::System.Text.Rune)({value})";
    }
}

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines the value of a Godot enum's member.
/// </summary>
public class GodotEnumValueInfo
{
    /// <summary>
    /// Name of the enum's member.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Value of the enum's member.
    /// </summary>
    [JsonPropertyName("value")]
    public required long Value { get; set; }

    /// <summary>
    /// Deconstructs the current enum member information.
    /// </summary>
    /// <param name="name">Name of the enum member.</param>
    /// <param name="value">Value of the enum member.</param>
    public void Deconstruct(out string name, out long value)
    {
        name = Name;
        value = Value;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} = {Value}";
}

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot property for an engine class.
/// </summary>
public class GodotPropertyInfo
{
    /// <summary>
    /// Name of the property.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the type of the property.
    /// Some properties contain multiple names separated by comma, it's usually
    /// better to use the getter return type and the setter first argument type
    /// as the type of the property when generating the C# bindings.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Name of the method used to set this property.
    /// The method should be available in <see cref="GodotClassInfo.Methods"/>.
    /// </summary>
    [JsonPropertyName("setter")]
    public string? Setter { get; set; }

    /// <summary>
    /// Name of the method used to get this property.
    /// The method should be available in <see cref="GodotClassInfo.Methods"/>.
    /// </summary>
    [JsonPropertyName("getter")]
    public string? Getter { get; set; }

    /// <summary>
    /// Some properties use getter/setter methods that contain an extra index
    /// argument, this is the index value that must be used with in the method
    /// invocation.
    /// This allows properties to reuse methods that take an index parameter,
    /// the index parameter is always the first parameter.
    /// <example>
    /// For example, a property <c>Slot4</c> can reuse the methods <c>GetSlot</c>
    /// and <c>SetSlot</c> with the hardcoded index <c>4</c>:
    /// <code>
    /// class MyClass
    /// {
    ///     Variant Slot4
    ///     {
    ///         get => GetSlot(index: 4);
    ///         set => SetSlot(index: 4, value);
    ///     }
    ///
    ///     Variant GetSlot(int index);
    ///     SetSlot(int index, Variant value);
    /// }
    /// </code>
    /// </example>
    /// </summary>
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Type} {Name} {{ get: {Getter}, set: {Setter} }}";
}

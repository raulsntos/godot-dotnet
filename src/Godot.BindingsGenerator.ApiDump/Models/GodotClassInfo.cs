namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot class. These are usually referred to as the engine classes and
/// ultimately derive from the Godot's Object type.
/// </summary>
public class GodotClassInfo
{
    /// <summary>
    /// Name of the class.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Indicates if the class derives from the RefCounted type and uses reference counting.
    /// </summary>
    [JsonPropertyName("is_refcounted")]
    public bool IsRefCounted { get; set; }

    /// <summary>
    /// Indicates if the class can be instantiated by consumers.
    /// </summary>
    [JsonPropertyName("is_instantiable")]
    public bool IsInstantiable { get; set; }

    /// <summary>
    /// The name of the base type that this class derives from.
    /// </summary>
    [JsonPropertyName("inherits")]
    public string? Inherits { get; set; }

    /// <summary>
    /// Indicates the type of API and whether it's available in exported games.
    /// </summary>
    [JsonPropertyName("api_type")]
    public GodotApiType ApiType { get; set; }

    /// <summary>
    /// Properties defined by the class.
    /// </summary>
    [JsonPropertyName("properties")]
    public GodotPropertyInfo[] Properties { get; set; } = [];

    /// <summary>
    /// Constructors defined by the class.
    /// </summary>
    [JsonPropertyName("constants")]
    public GodotConstantInfo[] Constants { get; set; } = [];

    /// <summary>
    /// Enums defined by the class.
    /// </summary>
    [JsonPropertyName("enums")]
    public GodotEnumInfo[] Enums { get; set; } = [];

    /// <summary>
    /// Methods defined by the class.
    /// </summary>
    [JsonPropertyName("methods")]
    public GodotMethodInfo[] Methods { get; set; } = [];

    /// <summary>
    /// Signals defined by the class.
    /// </summary>
    [JsonPropertyName("signals")]
    public GodotSignalInfo[] Signals { get; set; } = [];

    /// <inheritdoc/>
    public override string ToString() => $"class {Name}";
}

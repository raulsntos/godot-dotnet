namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Defines a Godot built-in class. These classes are typically represented as structs in C#.
/// </summary>
public class GodotBuiltInClassInfo
{
    /// <summary>
    /// Name of the class.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// The name of the return type for the indexer if the class supports indexing,
    /// <see langword="null"/> otherwise.
    /// The index type is always <see langword="long"/>.
    /// </summary>
    [JsonPropertyName("indexing_return_type")]
    public string? IndexingReturnType { get; set; }

    /// <summary>
    /// If the class is keyed.
    /// Usually a dictionary-like class that can use Variant keys to access Variant items.
    /// </summary>
    [JsonPropertyName("is_keyed")]
    public bool IsKeyed { get; set; }

    /// <summary>
    /// Members defined by the class.
    /// </summary>
    [JsonPropertyName("members")]
    public GodotMemberInfo[] Members { get; set; } = [];

    /// <summary>
    /// Constants defined by the class.
    /// </summary>
    [JsonPropertyName("constants")]
    public GodotConstantInfo[] Constants { get; set; } = [];

    /// <summary>
    /// Enums defined by the class.
    /// </summary>
    [JsonPropertyName("enums")]
    public GodotEnumInfo[] Enums { get; set; } = [];

    /// <summary>
    /// Operators defined by the class.
    /// </summary>
    [JsonPropertyName("operators")]
    public GodotOperatorInfo[] Operators { get; set; } = [];

    /// <summary>
    /// Methods defined by the class.
    /// </summary>
    [JsonPropertyName("methods")]
    public GodotBuiltInMethodInfo[] Methods { get; set; } = [];

    /// <summary>
    /// Constructors defined by the class.
    /// </summary>
    [JsonPropertyName("constructors")]
    public GodotConstructorInfo[] Constructors { get; set; } = [];

    /// <summary>
    /// Whether the class defines a destructor that must be called in order to properly dispose it.
    /// </summary>
    [JsonPropertyName("has_destructor")]
    public bool HasDestructor { get; set; }

    /// <inheritdoc/>
    public override string ToString() => $"class {Name}";
}

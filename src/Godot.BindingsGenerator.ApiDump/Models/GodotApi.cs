using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Godot.BindingsGenerator.ApiDump.Serialization;

namespace Godot.BindingsGenerator.ApiDump;

/// <summary>
/// Contains the Godot API information dumped from a Godot build.
/// Usually deserialized from a 'extension_api.json' file.
/// </summary>
public class GodotApi
{
    /// <summary>
    /// Godot version information.
    /// </summary>
    [JsonPropertyName("header")]
    public required GodotApiHeader Header { get; set; }

    /// <summary>
    /// Size information for Godot's built-in classes.
    /// </summary>
    [JsonPropertyName("builtin_class_sizes")]
    public GodotClassSizes[] BuiltInClassSizes { get; set; } = [];

    /// <summary>
    /// Member offset information for Godot's built-in classes.
    /// </summary>
    [JsonPropertyName("builtin_class_member_offsets")]
    public GodotClassMemberOffsetsGroup[] BuiltInClassMemberOffsets { get; set; } = [];

    /// <summary>
    /// Constant information for Godot's global or top-level constants.
    /// </summary>
    [JsonPropertyName("global_constants")]
    public GodotConstantInfo[] GlobalConstants { get; set; } = [];

    /// <summary>
    /// Enum information for Godot's global or top-level enums.
    /// </summary>
    [JsonPropertyName("global_enums")]
    public GodotEnumInfo[] GlobalEnums { get; set; } = [];

    /// <summary>
    /// Collection of utility functions available in Godot.
    /// </summary>
    [JsonPropertyName("utility_functions")]
    public GodotUtilityFunctionInfo[] UtilityFunctions { get; set; } = [];

    /// <summary>
    /// Class information for Godot's built-in classes.
    /// </summary>
    [JsonPropertyName("builtin_classes")]
    public GodotBuiltInClassInfo[] BuiltInClasses { get; set; } = [];

    /// <summary>
    /// Class information for Godot's engine classes.
    /// </summary>
    [JsonPropertyName("classes")]
    public GodotClassInfo[] Classes { get; set; } = [];

    /// <summary>
    /// Collection of singletons available in Godot.
    /// </summary>
    [JsonPropertyName("singletons")]
    public GodotSingletonInfo[] Singletons { get; set; } = [];

    /// <summary>
    /// Struct information for Godot's native structures.
    /// </summary>
    [JsonPropertyName("native_structures")]
    public GodotNativeStructureInfo[] NativeStructures { get; set; } = [];

    /// <summary>
    /// Deserialize the extension API dump file as a <see cref="GodotApi"/> instance.
    /// </summary>
    /// <param name="stream">Stream that contains the API dump.</param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the deserialization.
    /// </param>
    /// <returns>The deserialized <see cref="GodotApi"/>.</returns>
    public static ValueTask<GodotApi?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync(stream, GodotApiJsonSerializerContext.Default.GodotApi, cancellationToken);
    }

    /// <summary>
    /// Deserialize the extension API dump file as a <see cref="GodotApi"/> instance.
    /// </summary>
    /// <param name="stream">Stream that contains the API dump.</param>
    /// <returns>The deserialized <see cref="GodotApi"/>.</returns>
    public static GodotApi? Deserialize(Stream stream)
    {
        return JsonSerializer.Deserialize(stream, GodotApiJsonSerializerContext.Default.GodotApi);
    }
}

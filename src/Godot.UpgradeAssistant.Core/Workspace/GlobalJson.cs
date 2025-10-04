using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Helper for reading and writing 'global.json' files.
/// </summary>
internal sealed class GlobalJson
{
    private static readonly JsonDocumentOptions _jsonDocumentOption = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public static JsonDocument ParseAsDocument(Stream stream)
    {
        return JsonDocument.Parse(stream, _jsonDocumentOption);
    }

    public static JsonObject? ParseAsNode(Stream stream)
    {
        return JsonNode.Parse(stream, nodeOptions: null, _jsonDocumentOption)?.AsObject();
    }

    public static Task SerializeAsync(Stream stream, JsonNode value, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.SerializeAsync(stream, value, _jsonSerializerOptions, cancellationToken);
    }
}

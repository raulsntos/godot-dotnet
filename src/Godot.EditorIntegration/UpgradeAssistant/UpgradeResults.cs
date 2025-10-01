using System.Text.Json.Serialization;

namespace Godot.EditorIntegration.UpgradeAssistant;

// Must match 'SummaryStats' in 'Godot.UpgradeAssistant.Core/Exporters/SummaryStats.cs'
// but we only need to include the properties we actually use, others will be silently ignored
// in deserialization.
internal sealed partial class UpgradeResults
{
    public int ProblemsReported { get; init; }
}

[JsonSerializable(typeof(UpgradeResults))]
internal sealed partial class UpgradeResultsJsonContext : JsonSerializerContext { }

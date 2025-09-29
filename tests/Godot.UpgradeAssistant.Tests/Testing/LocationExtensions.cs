using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Godot.UpgradeAssistant.Tests;

internal static class LocationExtensions
{
    public static FileLinePositionSpan GetFileLinePositionSpan(this AnalysisResult analysisResult)
    {
        var start = new LinePosition(analysisResult.Location.StartLine, analysisResult.Location.StartColumn);
        var end = new LinePosition(analysisResult.Location.EndLine, analysisResult.Location.EndColumn);
        return new FileLinePositionSpan(analysisResult.Location.FilePath ?? "", start, end);
    }
}

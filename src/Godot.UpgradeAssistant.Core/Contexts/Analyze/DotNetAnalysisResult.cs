using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="AnalysisResult"/> reported for a .NET workspace.
/// </summary>
public abstract class DotNetAnalysisResult : AnalysisResult
{
    /// <summary>
    /// Constructs a <see cref="DotNetDiagnosticAnalysisResult"/> from a .NET diagnostic.
    /// </summary>
    /// <param name="diagnostic">The .NET diagnostic to create the analysis result for.</param>
    /// <param name="document">The document that triggered the diagnostic.</param>
    public static DotNetDiagnosticAnalysisResult FromDiagnostic(Diagnostic diagnostic, Document document)
    {
        return new DotNetDiagnosticAnalysisResult(diagnostic, document);
    }
}

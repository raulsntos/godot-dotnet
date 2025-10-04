using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Serilog;

namespace Godot.UpgradeAssistant.Providers;

internal static class DotNetDocumentExtensions
{
    public static bool TryGetDocumentForResult(this Solution solution, DotNetDiagnosticAnalysisResult analysisResult, [NotNullWhen(true)] out Document? document)
    {
        return solution.TryGetDocumentWithFilePath(analysisResult.Location.FilePath, out document);
    }

    public static bool TryGetDocumentForDiagnostic(this Solution solution, Diagnostic diagnostic, [NotNullWhen(true)] out Document? document)
    {
        return solution.TryGetDocumentWithFilePath(diagnostic.Location.SourceTree?.FilePath, out document);
    }

    public static bool TryGetDocumentWithFilePath(this Solution solution, string? filePath, [NotNullWhen(true)] out Document? document)
    {
        var documents = solution.GetDocumentIdsWithFilePath(filePath);

        if (documents.Length == 0)
        {
            document = null;
            return false;
        }

        if (documents.Length != 1)
        {
            Log.Warning(SR.FormatLog_FoundMultipleDocumentsOnlyOneWillBeUsed(documents.Length, filePath));
        }

        document = solution.GetDocument(documents[0]);
        return document is not null;
    }
}

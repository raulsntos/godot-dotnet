using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DiagnosticProvider : FixAllContext.DiagnosticProvider
{
    private readonly IEnumerable<DotNetDiagnosticAnalysisResult> _analysisResults;

    internal DiagnosticProvider(IEnumerable<DotNetDiagnosticAnalysisResult> analysisResults)
    {
        _analysisResults = analysisResults;
    }

    public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
    {
        var results = _analysisResults
            .Where(d => d.Project.Id == project.Id)
            .Select(d => d.Diagnostic);

        return Task.FromResult(results);
    }

    public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
    {
        var results = _analysisResults
            .Where(d => d.Document.Id == document.Id)
            .Select(d => d.Diagnostic);

        return Task.FromResult(results);
    }

    public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
    {
        var results = _analysisResults
            .Where(d => d.Project.Id == project.Id)
            .Select(d => d.Diagnostic);

        return Task.FromResult(results);
    }
}

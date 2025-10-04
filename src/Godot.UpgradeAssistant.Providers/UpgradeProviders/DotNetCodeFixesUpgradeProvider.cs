using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Serilog;

namespace Godot.UpgradeAssistant.Providers;

internal sealed partial class DotNetCodeFixersUpgradeProvider : IUpgradeAllProvider
{
    // GUA1XXX
    [GeneratedRegex(@"^GUA1[0-9]{3}$")]
    private static partial Regex RuleIdRegex { get; }

    private static ImmutableArray<CodeFixProvider> _fixers;
    private static ImmutableArray<CodeFixProvider> Fixers => _fixers.IsDefault ? (_fixers = GetCodeFixers()) : _fixers;

    public bool CanHandle(string ruleId)
    {
        return RuleIdRegex.IsMatch(ruleId);
    }

    public async Task UpgradeAsync(UpgradeContext context, IEnumerable<AnalysisResult> analysisResults, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Upgrader only applies to .NET workspaces and there isn't one.
            return;
        }

        var solution = context.Workspace.DotNetWorkspace.Solution;
        var workspace = context.Workspace.DotNetWorkspace.Workspace;

        var diagnosticResults = GetDiagnosticResults(analysisResults).ToArray();
        if (diagnosticResults.Length == 0)
        {
            // No diagnostics to fix.
            return;
        }

        foreach (var fixer in Fixers)
        {
            foreach (var diagnosticResult in diagnosticResults)
            {
                if (!fixer.FixableDiagnosticIds.Contains(diagnosticResult.Diagnostic.Id))
                {
                    // Skip diagnostic for this fixer if it does not support it,
                    // another fixer may be able to fix it.
                    continue;
                }

                var fixContext = new CodeFixContext(
                    document: diagnosticResult.Document,
                    diagnostic: diagnosticResult.Diagnostic,
                    registerCodeFix: (action, diagnostics) =>
                    {
                        var upgrade = DotNetUpgradeAction.FromCodeAction(action);
                        context.RegisterUpgrade(upgrade, diagnosticResult);
                    }, cancellationToken);

                await fixer.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);
            }
        }
    }

    public UpgradeAction? MergeFixes(WorkspaceInfo workspace, IReadOnlyCollection<UpgradeFix> allFixes)
    {
        if (workspace.DotNetWorkspace is null)
        {
            // Upgrader only applies to .NET workspaces and there isn't one.
            return null;
        }

        if (allFixes.Count == 0)
        {
            // No fixes to merge.
            return null;
        }

        if (allFixes.Count == 1)
        {
            // If there's only one fix, we can reuse its upgrade action instead of creating one.
            return allFixes.First().UpgradeAction;
        }

        var solution = workspace.DotNetWorkspace.Solution;

        // Collect all the diagnostics fixed by all the upgrade actions that will be merged.
        var diagnosticResults = allFixes
            .SelectMany(fix => fix.AnalysisResults)
            .Distinct()
            .Cast<DotNetDiagnosticAnalysisResult>();

        return new DotNetSolutionUpgradeAction(
            id: nameof(DotNetCodeFixersUpgradeProvider),
            title: "Fix all",
            CreateChangedSolution);

        Task<Solution?> CreateChangedSolution(CancellationToken cancellationToken = default)
        {
            return ApplyAllFixes(solution, diagnosticResults, allFixes, cancellationToken);
        }
    }

    private static async Task<Solution?> ApplyAllFixes(Solution solution, IEnumerable<DotNetDiagnosticAnalysisResult> diagnosticResults, IReadOnlyCollection<UpgradeFix> allFixes, CancellationToken cancellationToken = default)
    {
        if (allFixes.Count == 0)
        {
            // No fixes to apply.
            return null;
        }

        var diagnosticProvider = new DiagnosticProvider(diagnosticResults);

        var sortedProjects = GetSortedProjects(solution, cancellationToken);

        // Mapping from document to the cumulative text changes created for that document.
        var docIdToTextMerger = new Dictionary<DocumentId, TextChangeMerger>();

        foreach (var project in sortedProjects)
        {
            // First, determine the diagnostics to fix for the current project.
            var diagnosticsByDocument = await GetDocumentDiagnosticsToFixAsync(project, diagnosticProvider, cancellationToken).ConfigureAwait(false);

            // Then, order the diagnostics so we process them in a consistent manner and get the same results given the
            // same input.
            var sortedDiagnostics = diagnosticsByDocument.SelectMany(kvp => kvp.Value)
                .Where(d => d.Location.IsInSource)
                .OrderBy(d => d.Location.SourceTree!.FilePath)
                .ThenBy(d => d.Location.SourceSpan.Start)
                .ToImmutableArray();

            // Finally, take all the changes made to each document and merge them together into docIdToTextMerger to
            // keep track of the total set of changes to any particular document.
            var changedDocuments = GetAllChangedDocumentsForDiagnosticAsync(solution, sortedDiagnostics, allFixes, cancellationToken);
            await AddDocumentChangesAsync(solution, changedDocuments, docIdToTextMerger, cancellationToken).ConfigureAwait(false);
        }

        if (docIdToTextMerger.Count == 0)
        {
            // None of the fixes had document changes to apply.
            return null;
        }

        // Merge all the changes so they can be applied at once, and apply the changes to the solution.
        return await MergeChangesAsync(solution, docIdToTextMerger, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixAsync(Project project, FixAllContext.DiagnosticProvider diagnosticProvider, CancellationToken cancellationToken)
    {
        var projectDiagnostics = await diagnosticProvider.GetAllDiagnosticsAsync(project, cancellationToken).ConfigureAwait(false);

        var allDiagnostics = projectDiagnostics.ToImmutableArray();

        if (allDiagnostics.IsEmpty)
        {
            return ImmutableDictionary<Document, ImmutableArray<Diagnostic>>.Empty;
        }

        var diagnosticsByDocument = new Dictionary<Document, ImmutableArray<Diagnostic>.Builder>();

        foreach (var diagnostic in allDiagnostics)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = project.GetDocument(diagnostic.Location.SourceTree);
            if (document is null)
            {
                // Diagnostics with no document can't be fixed, ignore them.
                continue;
            }

            if (!diagnosticsByDocument.TryGetValue(document, out var documentDiagnostics))
            {
                diagnosticsByDocument[document] = documentDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            }
            documentDiagnostics.Add(diagnostic);
        }

        return diagnosticsByDocument.ToImmutableDictionary(
            d => d.Key,
            d => d.Value.ToImmutable());
    }

    private static async IAsyncEnumerable<Document> GetAllChangedDocumentsForDiagnosticAsync(Solution solution, IEnumerable<Diagnostic> diagnostics, IReadOnlyCollection<UpgradeFix> allFixes, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var diagnostic in diagnostics)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var action = GetUpgradeActionForDiagnostic(diagnostic, allFixes);
            if (action is null)
            {
                // No fixes available for this diagnostic.
                continue;
            }

            var changedSolution = await action.GetChangedSolutionAsync(cancellationToken).ConfigureAwait(false);
            if (changedSolution is null)
            {
                // Upgrade action contained no changes, so we're done with this diagnostic.
                continue;
            }

            var changedDocuments = changedSolution
                .GetChanges(solution)
                .GetProjectChanges()
                .SelectMany(p => p.GetChangedDocuments())
                .Select(changedSolution.GetDocument);

            foreach (var document in changedDocuments)
            {
                Debug.Assert(document is not null, "Unable to get changed document for fixed diagnostic.");
                yield return document;
            }
        }
    }

    private static DotNetWorkspaceUpgradeAction? GetUpgradeActionForDiagnostic(Diagnostic diagnostic, IReadOnlyCollection<UpgradeFix> allFixes)
    {
        // Get all the fixes that fix the current diagnostic.
        List<UpgradeFix> availableFixes = [];
        foreach (var fix in allFixes)
        {
            foreach (var analysisResult in fix.AnalysisResults)
            {
                if (analysisResult is DotNetDiagnosticAnalysisResult diagnosticResult
                 && diagnosticResult.Diagnostic == diagnostic)
                {
                    availableFixes.Add(fix);
                    break;
                }
            }
        }

        if (availableFixes.Count == 0)
        {
            // No fixes available for this diagnostic.
            return null;
        }
        else if (availableFixes.Count > 1)
        {
            // More than one fix available for this diagnostic.
            Log.Warning(SR.FormatLog_MultipleFixesAvailableFirstWillBeUsed(availableFixes.Count, diagnostic.Id));
        }

        {
            var fix = availableFixes[0];

            if (fix.UpgradeAction is not DotNetWorkspaceUpgradeAction action)
            {
                throw new InvalidOperationException(SR.FormatInvalidOperation_UnsupportedFixOfType(fix.UpgradeAction.GetType()));
            }

            return action;
        }
    }

    private static async Task AddDocumentChangesAsync(Solution solution, IAsyncEnumerable<Document> changedDocuments, Dictionary<DocumentId, TextChangeMerger> docIdToTextMerger, CancellationToken cancellationToken = default)
    {
        // Group all the changed documents by their document ID.
        // This means multiple changes apply to the same original document.
        var groupedChangedDocuments = new Dictionary<DocumentId, ImmutableArray<Document>.Builder>();
        await foreach (var changedDocument in changedDocuments.ConfigureAwait(false))
        {
            if (!groupedChangedDocuments.TryGetValue(changedDocument.Id, out var allDocumentChanges))
            {
                groupedChangedDocuments[changedDocument.Id] = allDocumentChanges = ImmutableArray.CreateBuilder<Document>();
            }
            allDocumentChanges.Add(changedDocument);
        }

        // Merge all the changed documents that apply changes to the same original document.
        var tasks = new List<Task>(groupedChangedDocuments.Count);
        foreach (var (documentId, allDocumentChanges) in groupedChangedDocuments)
        {
            if (!docIdToTextMerger.TryGetValue(documentId, out var textMerger))
            {
                var originalDocument = solution.GetDocument(documentId);
                Debug.Assert(originalDocument is not null, "Unable to get original document for fixed diagnostic.");

                textMerger = new TextChangeMerger(originalDocument);
                docIdToTextMerger.Add(documentId, textMerger);
            }

            // Process all document groups in parallel. For each group, merge all the doc changes into an
            // aggregated set of changes in the TextChangeMerger type.
            tasks.Add(Task.Run(async () =>
            {
                await textMerger.TryMergeChangesAsync(allDocumentChanges.ToImmutable(), cancellationToken).ConfigureAwait(false);
            }, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static async Task<Solution> MergeChangesAsync(Solution solution, Dictionary<DocumentId, TextChangeMerger> docIdToTextMerger, CancellationToken cancellationToken = default)
    {
        var currentSolution = solution;

        foreach (var group in docIdToTextMerger.GroupBy(kvp => kvp.Key.ProjectId))
        {
            var docIdsAndMerger = group.Select(kvp => (kvp.Key, kvp.Value));
            foreach (var (documentId, textMerger) in docIdsAndMerger)
            {
                var newText = await textMerger.GetFinalMergedTextAsync(cancellationToken).ConfigureAwait(false);
                currentSolution = currentSolution.WithDocumentText(documentId, newText);
            }
        }

        return currentSolution;
    }

    private static ImmutableArray<CodeFixProvider> GetCodeFixers()
    {
        var assembly = typeof(DotNetCodeFixersUpgradeProvider).Assembly;

        var fixers = ImmutableArray.CreateBuilder<CodeFixProvider>();

        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var exportAttribute = type.GetCustomAttribute<ExportCodeFixProviderAttribute>();
            if (exportAttribute is null)
            {
                // Type is not an exported code fix provider.
                continue;
            }

            if (!exportAttribute.Languages.Contains(LanguageNames.CSharp))
            {
                // Code fixer does not support C#.
                continue;
            }

            if (type.IsAbstract)
            {
                // Code fixer must be a concrete type.
                continue;
            }

            var fixer = Activator.CreateInstance(type) as CodeFixProvider;
            if (fixer is null)
            {
                // Unable to create an instance of the code fix provider.
                continue;
            }

            fixers.Add(fixer);
        }

        return fixers.DrainToImmutable();
    }

    /// <summary>
    /// Get all the analysis results converted to <see cref="DotNetDiagnosticAnalysisResult"/>.
    /// </summary>
    /// <param name="analysisResults">Generic enumeration of analysis results.</param>
    /// <returns>The enumeration with the elements converted to <see cref="DotNetAnalysisResult"/>.</returns>
    private static IEnumerable<DotNetDiagnosticAnalysisResult> GetDiagnosticResults(IEnumerable<AnalysisResult> analysisResults)
    {
        foreach (var analysis in analysisResults)
        {
            if (analysis is not DotNetDiagnosticAnalysisResult diagnosticResult)
            {
                // Upgrader can only handle analysis results of type DotNetDiagnosticAnalysisResult.
                // This should be unreachable.
                Debug.Fail($"{nameof(DotNetCodeFixersUpgradeProvider)} received an analysis result of unsupported type '{analysis.GetType()}'.");
                continue;
            }

            yield return diagnosticResult;
        }
    }

    /// <summary>
    /// Get all projects in the solution sorted topologically using the dependency graph.
    /// </summary>
    /// <param name="solution">Solution to get the projects from.</param>
    /// <param name="cancellationToken">Optional token to cancel the asynchronous operation.</param>
    /// <returns>The sorted enumeration of the projects in the solution.</returns>
    private static IEnumerable<Project> GetSortedProjects(Solution solution, CancellationToken cancellationToken = default)
    {
        var dependencyGraph = solution.GetProjectDependencyGraph();
        foreach (var projectId in dependencyGraph.GetTopologicallySortedProjects(cancellationToken))
        {
            var project = solution.GetProject(projectId);
            if (project is null)
            {
                throw new InvalidOperationException(SR.FormatInvalidOperation_RequiredProjectIsNotAvailable(projectId));
            }

            yield return project;
        }
    }
}

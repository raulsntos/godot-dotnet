using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Serilog;

namespace Godot.UpgradeAssistant.Providers;

internal sealed class DotNetAnalyzersAnalysisProvider : IAnalysisProvider
{
    private static AnalyzerReference? _analyzerReference;

    private static AnalyzerReference AnalyzerReference => _analyzerReference ??= GetAnalyzerReference();

    public async Task AnalyzeAsync(AnalysisContext context, CancellationToken cancellationToken = default)
    {
        if (context.Workspace.DotNetWorkspace is null)
        {
            // Analyzer only applies to .NET workspaces and there isn't one.
            return;
        }

        var solution = context.Workspace.DotNetWorkspace.Solution;

        foreach (var project in solution.Projects)
        {
            var analyzers = AnalyzerReference.GetAnalyzers(project.Language);
            await AnalyzeProjectAsync(context, solution, project, analyzers, cancellationToken).ConfigureAwait(false);
        }

        static async Task AnalyzeProjectAsync(AnalysisContext context, Solution solution, Project project, ImmutableArray<DiagnosticAnalyzer> analyzers = default, CancellationToken cancellationToken = default)
        {
            project = project.AddAnalyzerReference(AnalyzerReference);

            if (context.IsGodotDotNetEnabled)
            {
                // Add 'IsGodotDotNetEnabled' property so analyzers can check
                // whether the Godot .NET bindings should be used.
                solution = solution.AddAnalyzerConfigDocument(
                    DocumentId.CreateNewId(project.Id),
                    name: ".globalconfig",
                    filePath: "/.globalconfig",
                    text: SourceText.From($"""
                        is_global = true
                        build_property.{PropertyNames.IsGodotDotNetEnabled} = true
                        build_property.{PropertyNames.TargetGodotVersion} = {context.TargetGodotVersion}
                        """));
                project = solution.GetProject(project.Id)!;
            }

            if (analyzers.IsDefaultOrEmpty)
            {
                Log.Warning(SR.Log_AnalyzersAvailableZero);
                return;
            }

            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            if (compilation is null)
            {
                Log.Error(SR.FormatLog_UnableToGetCompilationForProject(project.Name, project.FilePath));
                return;
            }

            Log.Verbose(SR.FormatLog_AnalyzersAvailableTotal(analyzers.Length));

            // Filter analyzers that require enabling the Godot .NET bindings.

            // Even though analyzers can use the 'IsGodotDotNetEnable' build_property,
            // this allows excluding some analyzers entirely which can be more convenient
            // for those that are only needed for the Godot .NET bindings.
            if (!context.IsGodotDotNetEnabled)
            {
                analyzers = analyzers
                    .Where(static analyzer =>
                        !analyzer.GetType().IsDefined(typeof(RequiresGodotDotNetAttribute), false))
                    .ToImmutableArray();

                // Check if there are still analyzers available after filtering them.
                if (analyzers.IsDefaultOrEmpty)
                {
                    Log.Warning(SR.Log_AnalyzersAvailableZero);
                    return;
                }
            }

            Log.Verbose(SR.FormatLog_AnalyzersAvailableEnabled(analyzers.Length));

            var diagnostics = await compilation
                .WithAnalyzers(analyzers, project.AnalyzerOptions)
                .GetAnalyzerDiagnosticsAsync(analyzers, cancellationToken)
                .ConfigureAwait(false);
            foreach (var diagnostic in diagnostics)
            {
                if (!solution.TryGetDocumentForDiagnostic(diagnostic, out var document))
                {
                    Log.Error(SR.FormatLog_FoundDotNetDiagnosticWithoutDocument(diagnostic));
                    continue;
                }

                context.ReportAnalysis(DotNetAnalysisResult.FromDiagnostic(diagnostic, document));
            }
        }
    }

    private static AnalyzerFileReference GetAnalyzerReference()
    {
        var assembly = typeof(DotNetAnalyzersAnalysisProvider).Assembly;

        var reference = new AnalyzerFileReference(assembly.Location, AssemblyLoader.Instance);
        reference.AnalyzerLoadFailed += (_, e) =>
        {
            Log.Error(e.Message);
        };

        return reference;
    }

    private sealed class AssemblyLoader : IAnalyzerAssemblyLoader
    {
        public static readonly AssemblyLoader Instance = new();

        private AssemblyLoader() { }

        public void AddDependencyLocation(string fullPath) { }

        public Assembly LoadFromPath(string fullPath)
        {
            return Assembly.LoadFrom(fullPath);
        }
    }
}

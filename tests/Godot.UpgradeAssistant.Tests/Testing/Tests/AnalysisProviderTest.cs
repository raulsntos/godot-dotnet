using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Godot.UpgradeAssistant.Tests;

internal class AnalysisProviderTest<TAnalysisProvider>
    where TAnalysisProvider : IAnalysisProvider, new()
{
    private const string DefaultPathPrefix = "/0/Test";
    private const string DefaultSolutionName = "TestSolution";
    private const string DefaultProjectName = "TestProject";

    protected const string DefaultProject = "<Project />";
    protected string DefaultSolution => TestSolutionExtension switch
    {
        "slnx" => "<Solution />",
        _ => "",
    };

    public string? TestSolution { get; set; }

    public string TestSolutionExtension { get; set; } = "sln";

    public string? TestProject { get; set; }

    public string? TestGlobalJson { get; set; }

    public SemVer TargetGodotVersion { get; set; }

    public bool IsGodotDotNetEnabled { get; set; }

    public List<DiagnosticResult> ExpectedDiagnostics { get; } = [];

    private readonly List<AnalysisResult> _results = [];

    public IReadOnlyList<AnalysisResult> AnalysisResults => _results;

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        List<(string FileName, SourceText Content)> sources = [];

        string solutionPath = $"{DefaultPathPrefix}/{DefaultSolutionName}.{TestSolutionExtension}";
        string projectPath = $"{DefaultPathPrefix}/{DefaultProjectName}.csproj";
        string globalJsonPath = $"{DefaultPathPrefix}/global.json";

        if (TestSolution is not null)
        {
            sources.Add((solutionPath, SourceText.From(TestSolution.ReplaceLineEndings(), Encoding.UTF8)));
        }

        if (TestProject is not null)
        {
            sources.Add((projectPath, SourceText.From(TestProject.ReplaceLineEndings(), Encoding.UTF8)));
        }

        if (TestGlobalJson is not null)
        {
            sources.Add((globalJsonPath, SourceText.From(TestGlobalJson.ReplaceLineEndings(), Encoding.UTF8)));
        }

        var markupLocations = ImmutableDictionary<string, FileLinePositionSpan>.Empty;
        var (processedDiagnostics, processedSources) = ProcessMarkupSources(sources, ExpectedDiagnostics, DefaultPathPrefix, ref markupLocations);

        for (int i = 0; i < processedDiagnostics.Length; i++)
        {
            processedDiagnostics[i] = WithAppliedMarkupLocations(ref processedDiagnostics[i], markupLocations);
        }

        TestFile processedTestSolution = new()
        {
            FileName = solutionPath,
            Content = DefaultSolution,
        };

        TestFile processedTestProject = new()
        {
            FileName = projectPath,
            Content = DefaultProject,
        };

        TestFile? processedGlobalJson = null;

        foreach (var (fileName, content) in processedSources)
        {
            if (fileName == solutionPath)
            {
                processedTestSolution = processedTestSolution with { Content = content.ToString() };
            }

            if (fileName == projectPath)
            {
                processedTestProject = processedTestProject with { Content = content.ToString() };
            }

            if (fileName == globalJsonPath)
            {
                processedGlobalJson = new TestFile()
                {
                    FileName = globalJsonPath,
                    Content = content.ToString(),
                };
            }
        }

        var dotnetWorkspace = new MemoryDotNetWorkspaceInfo(processedTestSolution, processedTestProject, processedGlobalJson);
        using var workspace = new MemoryWorkspaceInfo(default, dotnetWorkspace);

        return RunAsyncCore(workspace, processedDiagnostics, cancellationToken);
    }

    protected virtual async Task RunAsyncCore(MemoryWorkspaceInfo workspace, DiagnosticResult[] processedExpectedDiagnostics, CancellationToken cancellationToken = default)
    {
        await VerifyAnalysis(workspace, processedExpectedDiagnostics, cancellationToken);
    }

    protected async Task VerifyAnalysis(MemoryWorkspaceInfo workspace, DiagnosticResult[] processedExpectedDiagnostics, CancellationToken cancellationToken = default)
    {
        _results.Clear();

        var verifier = new DefaultVerifier();

        var analyzer = new TAnalysisProvider();

        var context = new AnalysisContext(ReportAnalysis)
        {
            Workspace = workspace,
            TargetGodotVersion = TargetGodotVersion,
            IsGodotDotNetEnabled = IsGodotDotNetEnabled,
        };

        await analyzer.AnalyzeAsync(context, cancellationToken);

        var diagnosticsOutput = _results.Count != 0 ? FormatAnalysisResults(_results) : "    NONE.";
        string message = $"""
            Mismatch between number of diagnostics returned, expected "{processedExpectedDiagnostics.Length}" actual "{_results.Count}"

            Diagnostics:
            {diagnosticsOutput}

            """;
        verifier.Equal(processedExpectedDiagnostics.Length, _results.Count, message);

        // Sort both lists to ensure consistent ordering for comparison.
        SortAnalysisResults(_results);
        SortExpectedDiagnostics(processedExpectedDiagnostics);

        for (int i = 0; i < _results.Count; i++)
        {
            var actual = _results[i];
            var expected = processedExpectedDiagnostics[i];

            if (!expected.HasLocation)
            {
                message = "Expected a diagnostic with no location:";
                verifier.Equal(default, actual.Location, message);
            }
            else
            {
                VerifyDiagnosticLocation(actual, expected, verifier);
            }

            message = FormatVerifierMessage(actual, expected, $"Expected diagnostic id to be \"{expected.Id}\" was \"{actual.Id}\"");
            verifier.Equal(expected.Id, actual.Id, message);

            if (expected.Message is not null)
            {
                message = FormatVerifierMessage(actual, expected, $"Expected diagnostic message to be \"{expected.Message}\" was \"{actual.GetMessage(null)}\"");
                verifier.Equal(expected.Message, actual.GetMessage(null), message);
            }
        }

        void ReportAnalysis(AnalysisResult analysis)
        {
            _results.Add(analysis);
        }

        static void VerifyDiagnosticLocation(AnalysisResult actual, DiagnosticResult expected, IVerifier verifier)
        {
            var actualSpan = actual.GetFileLinePositionSpan();
            var expectedLocation = expected.Spans[0];

            bool assert = NormalizePath(actualSpan.Path) == expectedLocation.Span.Path;

            string message = FormatVerifierMessage(actual, expected, $"Expected diagnostic to be in file \"{expectedLocation.Span.Path}\" was actually in file \"{actualSpan.Path}\"");
            verifier.True(assert, message);

            VerifyLinePosition(actual, expected, actualSpan.StartLinePosition, expectedLocation.Span.StartLinePosition, "start", verifier);
            if (!expected.Options.HasFlag((DiagnosticOptions)DiagnosticLocationOptions.IgnoreLength))
            {
                VerifyLinePosition(actual, expected, actualSpan.EndLinePosition, expectedLocation.Span.EndLinePosition, "end", verifier);
            }

            static void VerifyLinePosition(AnalysisResult actual, DiagnosticResult expected, LinePosition actualLinePosition, LinePosition expectedLinePosition, string positionText, IVerifier verifier)
            {
                string message = FormatVerifierMessage(actual, expected, $"Expected diagnostic to {positionText} on line \"{expectedLinePosition.Line + 1}\" was actually on line \"{actualLinePosition.Line + 1}\"");
                verifier.Equal(expectedLinePosition.Line, actualLinePosition.Line, message);

                message = FormatVerifierMessage(actual, expected, $"Expected diagnostic to {positionText} at column \"{expectedLinePosition.Character + 1}\" was actually at column \"{actualLinePosition.Character + 1}\"");
                verifier.Equal(expectedLinePosition.Character, actualLinePosition.Character, message);
            }

            static string NormalizePath(string path)
            {
                // ProjectRootElement always normalizes the path, so when analysis providers report a diagnostic
                // the location is always normalized. In Windows this means it will change the path separators and
                // add a drive letter, so we need to change it back so it matches the expected diagnostic location.
                if (OperatingSystem.IsWindows())
                {
                    int driveSeparator = path.IndexOf(':');
                    if (driveSeparator != -1)
                    {
                        path = path.Substring(driveSeparator + 1);
                    }
                    return path.Replace("\\", "/", StringComparison.Ordinal);
                }
                else
                {
                    return path;
                }
            }
        }

        static string FormatVerifierMessage(AnalysisResult actual, DiagnosticResult expected, string message)
        {
            return $"""
                {message}
                Expected diagnostic:
                    // {expected.Id}: {expected.Message} {GetLocation(expected)}
                Actual diagnostic:
                    // {actual.Id}: {actual.GetMessage(null)} at {actual.Location}
                """;

            static string GetLocation(DiagnosticResult diagnostic)
            {
                return diagnostic.HasLocation
                    ? $"at {diagnostic.Spans[0].Span}"
                    : "";
            }
        }

        static string FormatAnalysisResults(IEnumerable<AnalysisResult> analysisResults)
        {
            var sb = new StringBuilder();

            foreach (var analysisResult in analysisResults)
            {
                var location = analysisResult.GetFileLinePositionSpan();
                sb.AppendLine(CultureInfo.InvariantCulture, $"    // {analysisResult.Id}: {analysisResult.GetMessage(null)} at {location},");
            }

            return sb.ToString();
        }
    }

    private static void SortAnalysisResults(List<AnalysisResult> analysisResults)
    {
        analysisResults.Sort((x, y) => CompareLocation(x.Location, y.Location));

        static int CompareLocation(Location x, Location y)
        {
            int cmp = x.StartLine.CompareTo(y.StartLine);
            if (cmp != 0)
            {
                return cmp;
            }
            return x.EndLine.CompareTo(y.EndLine);
        }
    }

    private static void SortExpectedDiagnostics(DiagnosticResult[] expectedDiagnostics)
    {
        Array.Sort(expectedDiagnostics, ((x, y) =>
        {
            // The diagnostic may have no location, in which case the 'Spans' collection will be empty.
            if (x.Spans.Length == 0 && y.Spans.Length == 0)
            {
                return 0;
            }
            if (x.Spans.Length == 0)
            {
                return -1;
            }
            if (y.Spans.Length == 0)
            {
                return 1;
            }

            return CompareFileLinePositionSpan(x.Spans[0].Span, y.Spans[0].Span);
        }));

        static int CompareFileLinePositionSpan(FileLinePositionSpan x, FileLinePositionSpan y)
        {
            int cmp = x.StartLinePosition.CompareTo(y.StartLinePosition);
            if (cmp != 0)
            {
                return cmp;
            }
            return x.EndLinePosition.CompareTo(y.EndLinePosition);
        }
    }

    private static (DiagnosticResult[] ExpectedDiagnostics, (string Filename, SourceText Content)[] Sources) ProcessMarkupSources(IEnumerable<(string FileName, SourceText Content)> sources, IEnumerable<DiagnosticResult> explicitDiagnostics, string defaultPath, ref ImmutableDictionary<string, FileLinePositionSpan> markupLocations)
    {
        List<(string fileName, SourceText content)> sourceFiles = new();

        var diagnostics = explicitDiagnostics
            .Select(diagnostic => diagnostic.WithDefaultPath(defaultPath))
            .ToList();

        foreach (var (fileName, content) in sources)
        {
            TestFileMarkupParser.GetPositionsAndSpans(content.ToString(), out string output, out var positions, out var namedSpans);

            sourceFiles.Add((fileName, content.Replace(new TextSpan(0, content.Length), output)));

            if (positions.IsEmpty && namedSpans.IsEmpty)
            {
                // No markup notation in this input.
                continue;
            }

            var sourceText = SourceText.From(output, content.Encoding, content.ChecksumAlgorithm);

            foreach (var position in positions)
            {
                var diagnostic = CreateDiagnosticForPosition("", fileName, sourceText, position);
                diagnostics.Add(diagnostic);
            }

            foreach (var (name, spans) in namedSpans.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                if (name.StartsWith('#'))
                {
                    // This is an indexed location. Keep track of it for later processing.
                    if (markupLocations.ContainsKey(name) || spans.Length != 1)
                    {
                        throw new InvalidOperationException($"Input contains multiple markup locations with key '{name}'.");
                    }

                    var linePositionSpan = sourceText.Lines.GetLinePositionSpan(spans[0]);
                    markupLocations = markupLocations.Add(name, new FileLinePositionSpan(fileName, linePositionSpan));
                    continue;
                }

                foreach (var span in spans)
                {
                    var diagnostic = CreateDiagnosticForSpan(name, fileName, sourceText, span);
                    diagnostics.Add(diagnostic);
                }
            }
        }

        return (diagnostics.ToArray(), sourceFiles.ToArray());
    }

    private static DiagnosticResult CreateDiagnosticForPosition(string diagnosticId, string fileName, SourceText content, int position)
    {
        var diagnosticResult = CreateDiagnostic(diagnosticId);
        var linePosition = content.Lines.GetLinePosition(position);
        return diagnosticResult.WithLocation(fileName, linePosition);
    }

    private static DiagnosticResult CreateDiagnosticForSpan(string diagnosticId, string fileName, SourceText content, TextSpan span)
    {
        var diagnosticResult = CreateDiagnostic(diagnosticId);
        var linePositionSpan = content.Lines.GetLinePositionSpan(span);
        return diagnosticResult.WithSpan(new FileLinePositionSpan(fileName, linePositionSpan));
    }

    private static DiagnosticResult CreateDiagnostic(string diagnosticId)
    {
        if (string.IsNullOrEmpty(diagnosticId))
        {
            throw new InvalidOperationException("Markup syntax can't omit the diagnostic ID, position markup is not supported.");
        }

        return new DiagnosticResult(diagnosticId, DiagnosticSeverity.Error)
            .WithMessage(null)
            .WithOptions(DiagnosticOptions.IgnoreAdditionalLocations | DiagnosticOptions.IgnoreSeverity);
    }

    // NOTE: Unfortunately this method is private, so we need to use 'UnsafeAccessor' to call it.
    //       But we only need this for the {|#n:Text|} syntax, which we don't really use right now.
    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern DiagnosticResult WithAppliedMarkupLocations(ref DiagnosticResult diagnostic, ImmutableDictionary<string, FileLinePositionSpan> markupLocations);
}

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Exporter implementation that writes the summary of the results to an HTML file.
/// </summary>
public sealed class HtmlExporter : IExporter
{
    /// <inheritdoc/>
    public async Task ExportAsync(Summary summary, string outputPath, CancellationToken cancellationToken = default)
    {
        using var stream = File.Open(outputPath, FileMode.Create);
        using var writer = new StreamWriter(stream);

        await WriteAsync(writer, """
            <!DOCTYPE html>
            <html>
            """, cancellationToken).ConfigureAwait(false);

        await WriteHeadAsync(writer, cancellationToken).ConfigureAwait(false);

        await WriteAsync(writer, """

                <body>
            """, cancellationToken).ConfigureAwait(false);

        await WriteBodyAsync(writer, summary, cancellationToken).ConfigureAwait(false);

        await WriteAsync(writer, """

                </body>
            </html>

            """, cancellationToken).ConfigureAwait(false);
    }

    // TODO: Use constant StringSyntaxAttribute.Html when added to the BCL.
    // https://github.com/dotnet/runtime/issues/76138
    private static Task WriteAsync(TextWriter writer, [StringSyntax("Html")] string value, CancellationToken cancellationToken = default)
    {
        return writer.WriteAsync(value.AsMemory(), cancellationToken);
    }

    private static async Task WriteHeadAsync(TextWriter writer, CancellationToken cancellationToken = default)
    {
        await WriteAsync(writer, $"""

                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width,minimum-scale=1,initial-scale=1,user-scalable=yes">
                    <title>Godot .NET Upgrade Assistant summary - {DateTime.Now}</title>

            """, cancellationToken).ConfigureAwait(false);
        await WriteStyleAsync(writer, cancellationToken).ConfigureAwait(false);
        await WriteAsync(writer, """

                </head>
            """, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteStyleAsync(TextWriter writer, CancellationToken cancellationToken = default)
    {
        await WriteAsync(writer, "<style>", cancellationToken).ConfigureAwait(false);

        using var cssStream = typeof(HtmlExporter).Assembly.GetManifestResourceStream("Godot.UpgradeAssistant.Core.Exporters.style.css");
        Debug.Assert(cssStream is not null);

        byte[] buffer = new byte[2048];

        int bytesRead;
        while ((bytesRead = cssStream.Read(buffer)) > 0)
        {
            string value = Encoding.UTF8.GetString(buffer.AsSpan(0, bytesRead));
            await WriteAsync(writer, value, cancellationToken).ConfigureAwait(false);
        }

        await WriteAsync(writer, "</style>", cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteBodyAsync(TextWriter writer, Summary summary, CancellationToken cancellationToken = default)
    {
        await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);

        await WriteAsync(writer, """

                    <div class="container">
            """, cancellationToken).ConfigureAwait(false);

        await WriteGeneralSummaryAsync(writer, summary, cancellationToken).ConfigureAwait(false);

        await WriteSummaryResultAsync(writer, summary, cancellationToken).ConfigureAwait(false);

        if (summary.Problems.Count > 0)
        {
            await WriteProblemsAsync(writer, summary, cancellationToken).ConfigureAwait(false);
        }

        await WriteAsync(writer, """

                    </div>
            """, cancellationToken).ConfigureAwait(false);

        await WriteFooterAsync(writer, cancellationToken).ConfigureAwait(false);
    }

    private static Task WriteHeaderAsync(TextWriter writer, CancellationToken cancellationToken = default)
    {
        return WriteAsync(writer, """

                    <header class="head">
                        <div class="container">
                            <h1>Godot .NET Upgrade Assistant summary</h1>
                        </div>
                    </header>
            """, cancellationToken);
    }

    private static Task WriteFooterAsync(TextWriter writer, CancellationToken cancellationToken = default)
    {
        // TODO: Update the repo URL when moving to the godotengine org.
        return WriteAsync(writer, """

                    <footer>
                        <div class="container">
                            <div>
                                <p>
                                    &copy; 2025 Godot .NET Upgrade Assistant.<br/>
                                    <a href="https://github.com/raulsntos/godot-dotnet">Tool source code on GitHub</a>
                                </p>
                            </div>
                            <ul>
                                <li><strong>Godot Engine Resources</strong></li>
                                <li><a href="https://godotengine.org">Website</a></li>
                                <li><a href="https://docs.godotengine.org">Documentation</a></li>
                                <li><a href="https://github.com/godotengine">Code Repository</a></li>
                            </ul>
                        </div>
                    </footer>
            """, cancellationToken);
    }

    private static Task WriteGeneralSummaryAsync(TextWriter writer, Summary summary, CancellationToken cancellationToken = default)
    {
        return WriteAsync(writer, $"""

                        <table>
                            <tr>
                                <th>Summary creation date</th>
                                <td class="numeric">{summary.TimeStamp}</td>
                            </tr>
                            <tr>
                                <th>Problems reported</th>
                                <td class="numeric">{summary.Problems.Count}</td>
                            </tr>
                            <tr>
                                <th>Problems unresolved</th>
                                <td class="numeric">{summary.Problems
                                    .Count(problem => !problem.HasFixApplied)}</td>
                            </tr>
                            <tr>
                                <th>Problems with fixes available</th>
                                <td class="numeric">{summary.Problems
                                    .Count(problem => problem.HasFixAvailable)}</td>
                            </tr>
                            <tr>
                                <th>Problems with fixes applied</th>
                                <td class="numeric">{summary.Problems
                                    .Count(problem => problem.HasFixApplied)}</td>
                            </tr>
                        </table>
            """, cancellationToken);
    }

    private static async Task WriteSummaryResultAsync(TextWriter writer, Summary summary, CancellationToken cancellationToken)
    {
        if (summary.Problems.Count == 0)
        {
            await WriteAsync(writer, $"""

                            <div class="summary-result success">No problems found. Your project is ready for Godot {summary.TargetGodotVersion}! 🎉</div>
                """, cancellationToken).ConfigureAwait(false);
        }
        else if (!summary.Problems.Any(problem => !problem.HasFixApplied))
        {
            await WriteAsync(writer, $"""

                            <div class="summary-result success">All problems fixed. Your project has been upgraded to Godot {summary.TargetGodotVersion}! 🎉</div>
                """, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await WriteAsync(writer, """

                            <div class="summary-result error">⚠ Found problems that need to be fixed manually.</div>
                """, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WriteProblemsAsync(TextWriter writer, Summary summary, CancellationToken cancellationToken = default)
    {
        await WriteAsync(writer, """

                        <h1>Problems found</h1>
                        <div class="problems">
                            <ul>
            """, cancellationToken).ConfigureAwait(false);

        foreach (var problem in summary.Problems)
        {
            await WriteProblemAsync(writer, problem, cancellationToken).ConfigureAwait(false);
        }

        await WriteAsync(writer, """

                            </ul>
                        </div>
            """, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteProblemAsync(TextWriter writer, ProblemSummaryData problem, CancellationToken cancellationToken)
    {
        string status = problem switch
        {
            _ when problem.HasFixApplied => "fixed",
            _ when problem.HasFixAvailable => "fixable",
            _ => "unfixable",
        };

        await WriteAsync(writer, $"""

                                <li class="problem {status} card">
                                    <div class="card-content">
                                        <h3>[{problem.AnalysisResult.Id}] {problem.AnalysisResult.Title}</h3>
                                        <table>
            """, cancellationToken).ConfigureAwait(false);

        if (problem.AnalysisResult.Description is not null)
        {
            await WriteAsync(writer, $"""

                                                <tr>
                                                    <th>Description</th>
                                                    <td>{problem.AnalysisResult.Description}</td>
                                                </tr>
                """, cancellationToken).ConfigureAwait(false);
        }

        if (problem.AnalysisResult.MessageFormat is not null)
        {
            await WriteAsync(writer, $"""

                                                <tr>
                                                    <th>Message</th>
                                                    <td>{problem.AnalysisResult.GetMessage(writer.FormatProvider)}</td>
                                                </tr>
                """, cancellationToken).ConfigureAwait(false);
        }

        if (problem.AnalysisResult.Location.IsValid)
        {
            await WriteAsync(writer, $"""

                                                <tr>
                                                    <th>Location</th>
                                                    <td><small>{problem.AnalysisResult.Location}</small></td>
                                                </tr>
                """, cancellationToken).ConfigureAwait(false);
        }

        string outcome = problem switch
        {
            _ when problem.HasFixApplied => "Fix has been applied.",
            _ when problem.HasFixAvailable => "Problem has available fixes but none have been applied.",
            _ => "Problem has no available fixes and must be fixed manually.",
        };

        await WriteAsync(writer, $"""

                                            <tr class="outcome">
                                                <th>Outcome</th>
                                                <td>{outcome}</td>
                                            </tr>
            """, cancellationToken).ConfigureAwait(false);
        if (problem.AnalysisResult.HelpUri is not null)
        {
            await WriteAsync(writer, $"""

                                                <tr>
                                                    <th>Help</th>
                                                    <td><a href="{problem.AnalysisResult.HelpUri}">{problem.AnalysisResult.HelpUri}</a></td>
                                                </tr>
                """, cancellationToken).ConfigureAwait(false);
        }
        if (problem.AnalysisResult.HelpUri is not null)
        {
            await WriteAsync(writer, $"""

                                                <tr>
                                                    <th>Location</th>
                                                    <td>{problem.AnalysisResult.Location}</td>
                                                </tr>
                """, cancellationToken).ConfigureAwait(false);
        }

        await WriteAsync(writer, """

                                        </table>
                                    </div>
            """, cancellationToken).ConfigureAwait(false);

        await WriteFixesAsync(writer, problem, cancellationToken).ConfigureAwait(false);

        await WriteAsync(writer, """

                                </li>
            """, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteFixesAsync(TextWriter writer, ProblemSummaryData problem, CancellationToken cancellationToken = default)
    {
        await WriteAsync(writer, """

                                    <div class="footer">
                                        <div class="card-content">
                                            <h4>Fixes</h4>
            """, cancellationToken).ConfigureAwait(false);

        if (!problem.HasFixAvailable)
        {
            await WriteAsync(writer, """

                                                <div class="text-muted">No fixes available, problem must be fixed manually.</div>
                """, cancellationToken).ConfigureAwait(false);
            return;
        }

        await WriteAsync(writer, """

                                            <div class="fixes">
                                                <ul>
            """, cancellationToken).ConfigureAwait(false);

        if (problem.HasFixApplied)
        {
            await WriteFixAsync(writer, problem.UpgradeActionApplied, isApplied: true, cancellationToken).ConfigureAwait(false);
        }

        foreach (var upgrade in problem.UpgradeActions
            .Where(upgrade => upgrade != problem.UpgradeActionApplied))
        {
            await WriteFixAsync(writer, upgrade, isApplied: false, cancellationToken).ConfigureAwait(false);
        }

        await WriteAsync(writer, """

                                                </ul>
                                            </div>
                                        </div>
                                    </div>
            """, cancellationToken).ConfigureAwait(false);
    }

    private static Task WriteFixAsync(TextWriter writer, UpgradeAction upgrade, bool isApplied, CancellationToken cancellationToken = default)
    {
        return WriteAsync(writer, $"""

                                                    <li class="fix {(isApplied ? "applied" : "")}">
                                                        <strong>{upgrade.Title}</strong>
                                                    </li>
            """, cancellationToken);
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="AnalysisResult"/> constructed from a .NET diagnostic.
/// </summary>
public sealed class DotNetDiagnosticAnalysisResult : DotNetAnalysisResult
{
    /// <summary>
    /// Diagnostic from the .NET analyzer that generated this analysis result.
    /// </summary>
    public Diagnostic Diagnostic { get; }

    /// <summary>
    /// The .NET project that produced the diagnostic.
    /// </summary>
    public Project Project => Document.Project;

    /// <summary>
    /// The .NET document that produced the diagnostic.
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// Constructs a <see cref="DotNetDiagnosticAnalysisResult"/>.
    /// </summary>
    /// <param name="diagnostic">The .NET diagnostic to create the analysis result for.</param>
    /// <param name="document">The document that triggered the diagnostic.</param>
    [SetsRequiredMembers]
    public DotNetDiagnosticAnalysisResult(Diagnostic diagnostic, Document document)
    {
        Id = diagnostic.Id;
        Title = diagnostic.Descriptor.Title;
        Location = GetLocation(diagnostic);

        Description = diagnostic.Descriptor.Description;
        MessageFormat = diagnostic.Descriptor.MessageFormat;

        Diagnostic = diagnostic;
        Document = document;
    }

    private static Location GetLocation(Diagnostic diagnostic)
    {
        var lineSpan = diagnostic.Location.GetLineSpan();

        return new Location
        (
            startLine: lineSpan.StartLinePosition.Line,
            endLine: lineSpan.EndLinePosition.Line,
            startColumn: lineSpan.StartLinePosition.Character,
            endColumn: lineSpan.EndLinePosition.Character,
            filePath: lineSpan.Path
        );
    }

    /// <inheritdoc/>
    protected override string? GetMessageCore(IFormatProvider? formatProvider)
    {
        return Diagnostic.GetMessage(formatProvider);
    }
}

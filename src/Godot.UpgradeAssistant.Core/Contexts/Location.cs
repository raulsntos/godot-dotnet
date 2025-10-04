using System.Diagnostics.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Represents a span of text in source code.
/// </summary>
public readonly struct Location
{
    /// <summary>
    /// Zero-based integer for the starting source line.
    /// </summary>
    public int StartLine { get; }

    /// <summary>
    /// Zero-based integer for the ending source line.
    /// </summary>
    public int EndLine { get; }

    /// <summary>
    /// Zero-based integer for the starting source column.
    /// </summary>
    public int StartColumn { get; }

    /// <summary>
    /// Zero-based integer for the ending source column.
    /// </summary>
    public int EndColumn { get; }

    /// <summary>
    /// The path to the file that contains the source.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Indicates whether this location points to a valid source.
    /// </summary>
    [MemberNotNullWhen(true, nameof(FilePath))]
    public bool IsValid => FilePath is not null;

    /// <summary>
    /// Construct a <see cref="Location"/> from <see cref="Microsoft.Build.Construction.ElementLocation"/>.
    /// </summary>
    /// <param name="location">The MSBuild location.</param>
    public static implicit operator Location(Microsoft.Build.Construction.ElementLocation location) => new
    (
        startLine: location.Line - 1,
        endLine: location.Line - 1,
        startColumn: location.Column - 1,
        endColumn: location.Column - 1,
        filePath: location.File
    );

    /// <summary>
    /// Constructs a <see cref="Location"/>.
    /// </summary>
    /// <param name="startLine">Zero-based integer for the starting source line.</param>
    /// <param name="endLine">Zero-based integer for the ending source line.</param>
    /// <param name="startColumn">Zero-based integer for the starting source column.</param>
    /// <param name="endColumn">Zero-based integer for the ending source column.</param>
    /// <param name="filePath">The path to the file that contains the source.</param>
    public Location(int startLine, int endLine, int startColumn, int endColumn, string filePath)
    {
        StartLine = startLine;
        EndLine = endLine;
        StartColumn = startColumn;
        EndColumn = endColumn;
        FilePath = filePath;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return IsValid ? $"{FilePath}({StartLine + 1},{StartColumn + 1})" : "";
    }
}

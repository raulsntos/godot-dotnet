using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes a problem encountered in the analysis step that needs to be fixed.
/// </summary>
public abstract partial class AnalysisResult : IFormattable
{
    /// <summary>
    /// Id of the analysis rule that triggered this result.
    /// It will be used for upgrades to determine which results they can handle.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Short title describing the analysis result.
    /// </summary>
    public required LocalizableString Title { get; init; }

    /// <summary>
    /// Optional long description for the analysis result.
    /// </summary>
    public LocalizableString? Description { get; init; }

    /// <summary>
    /// A format message string, which can be passed as the first argument to
    /// <see cref="string.Format(string, object[])"/> when creating the message
    /// with this analysis result.
    /// </summary>
    public LocalizableString? MessageFormat { get; init; }

    /// <summary>
    /// Optional hyperlink that provides more detailed information regarding
    /// the analysis result.
    /// </summary>
    public Uri? HelpUri { get; init; }

    /// <summary>
    /// Location of the analysis result.
    /// </summary>
    public required Location Location { get; init; }

    /// <summary>
    /// Get the analysis result message formatted with the arguments that were provided
    /// when it was constructed.
    /// </summary>
    /// <param name="formatProvider">
    /// The provider to use to format the value.
    /// -or-
    /// A null reference to obtain the numeric format information from the current locale
    /// setting of the operating system.
    /// </param>
    /// <returns>The formatted message for this analysis result.</returns>
    public string? GetMessage(IFormatProvider? formatProvider = null)
    {
        return GetMessageCore(formatProvider);
    }

    /// <summary>
    /// Implementation of <see cref="GetMessage(IFormatProvider?)"/> that retrieves the
    /// message formatted with the corresponding arguments.
    /// </summary>
    /// <param name="formatProvider">
    /// The provider to use to format the value.
    /// -or-
    /// A null reference to obtain the numeric format information from the current locale
    /// setting of the operating system.
    /// </param>
    /// <returns>The formatted message for this analysis result.</returns>
    protected abstract string? GetMessageCore(IFormatProvider? formatProvider);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        string title = Title.ToString(formatProvider);
        var sb = new StringBuilder($"{Id}: {title}");
        if (Description is not null)
        {
            string description = Description.ToString(formatProvider);
            sb.Append(formatProvider, $" [{description}]");
        }
        if (Location.IsValid)
        {
            sb.Append(formatProvider, $" [{Location}]");
        }
        return sb.ToString();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToString(null, null);
    }
}

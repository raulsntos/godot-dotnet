using System;
using Microsoft.CodeAnalysis;

namespace Godot.UpgradeAssistant;

partial class AnalysisResult
{
    /// <summary>
    /// Constructs an <see cref="AnalysisResult"/>.
    /// </summary>
    /// <param name="id">Id of the analysis rule that triggered this result.</param>
    /// <param name="title">Short title describing the analysis result.</param>
    /// <param name="location">Location of the analysis result.</param>
    /// <param name="description">Long description for the analysis result.</param>
    /// <param name="helpUri">Hyperlink that provides more detailed information.</param>
    /// <param name="messageFormat">A format message string.</param>
    /// <param name="messageArgs">The arguments provided for <paramref name="messageFormat"/>.</param>
    /// <returns>The constructed analysis result.</returns>
    public static AnalysisResult Create(string id, LocalizableString title, Location location, LocalizableString? description = null, Uri? helpUri = null, LocalizableString? messageFormat = null, params object?[]? messageArgs)
    {
        return new SimpleAnalysisResult()
        {
            Id = id,
            Title = title,
            Location = location,

            Description = description,
            HelpUri = helpUri,

            MessageFormat = messageFormat,
            MessageArgs = messageArgs,
        };
    }
}

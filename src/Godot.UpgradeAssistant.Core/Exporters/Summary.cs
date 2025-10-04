using System;
using System.Collections.Generic;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes the result of the assistant execution.
/// </summary>
public readonly struct Summary
{
    /// <summary>
    /// Date and time when the assistant was executed.
    /// </summary>
    public DateTime TimeStamp { get; }

    /// <summary>
    /// The target version of Godot that the assistant tried to upgrade to.
    /// </summary>
    public SemVer TargetGodotVersion { get; }

    /// <summary>
    /// Collection of all the analysis summary data generated.
    /// </summary>
    public IReadOnlyCollection<ProblemSummaryData> Problems { get; }

    /// <summary>
    /// Constructs a <see cref="Summary"/>.
    /// </summary>
    /// <param name="timeStamp">
    /// Date and time when the assistant was executed.
    /// </param>
    /// <param name="targetGodotVersion">
    /// The target version of Godot that the assistant tried to upgrade to.
    /// </param>
    /// <param name="problems">
    /// Collection of all the analysis summary data generated.
    /// </param>
    public Summary(DateTime timeStamp, SemVer targetGodotVersion, IReadOnlyCollection<ProblemSummaryData> problems)
    {
        TimeStamp = timeStamp;
        TargetGodotVersion = targetGodotVersion;
        Problems = problems;
    }
}

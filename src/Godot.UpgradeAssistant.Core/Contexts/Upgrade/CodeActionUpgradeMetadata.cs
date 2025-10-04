using Microsoft.CodeAnalysis.CodeActions;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Contains information that can be included in a <see cref="CodeAction"/>
/// to specify the values in the corresponding <see cref="UpgradeAction"/>
/// that don't have an equivalent in <see cref="CodeAction"/>.
/// </summary>
public sealed class CodeActionUpgradeMetadata
{
    /// <see cref="UpgradeAction.FixesDiagnostic"/>
    public bool FixesDiagnostic { get; init; } = true;
}

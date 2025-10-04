using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes an operation that can fix a corresponding analysis result.
/// </summary>
public abstract class UpgradeAction
{
    private static readonly AsyncLocal<IUpgradeProvider?> _asyncLocalUpgradeProvider = new();

    /// <summary>
    /// Current <see cref="IUpgradeProvider"/> to set as <see cref="UpgradeProvider"/>
    /// when creating a new <see cref="UpgradeAction"/> instance.
    /// </summary>
    internal static IUpgradeProvider? CurrentUpgradeProvider
    {
        get => _asyncLocalUpgradeProvider.Value;
        set => _asyncLocalUpgradeProvider.Value = value;
    }

    /// <summary>
    /// Id of the upgrade rule that triggered this action.
    /// It is shared for all equivalent upgrade actions so they can be
    /// filtered or executed in batch.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Short title describing the upgrade action.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Optional long description for the upgrade action.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Indicates whether the upgrade action fixes the reported diagnostic
    /// after applying the changes. It is used for changes like adding comments
    /// or partial fixes that still need manual work.
    /// </summary>
    public bool FixesDiagnostic { get; init; } = true;

    /// <summary>
    /// The <see cref="IUpgradeProvider"/> that created the upgrade action.
    /// </summary>
    public IUpgradeProvider? UpgradeProvider { get; }

    /// <summary>
    /// Constructs an <see cref="UpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    public UpgradeAction(string id, string title)
    {
        Id = id;
        Title = title;
        UpgradeProvider = CurrentUpgradeProvider;
    }

    /// <summary>
    /// Apply the changes to the workspace.
    /// </summary>
    /// <param name="workspace">Workspace to apply the changes to.</param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>Task that completes when the changes have been applied.</returns>
    public abstract Task ApplyChanges(WorkspaceInfo workspace, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder($"{Id}: {Title}");
        if (!string.IsNullOrEmpty(Description))
        {
            sb.Append(CultureInfo.InvariantCulture, $" [{Description}]");
        }
        return sb.ToString();
    }
}

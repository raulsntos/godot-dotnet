
using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

/// <summary>
/// An <see cref="UpgradeAction"/> constructed for a .NET workspace
/// that applies fixes to a <c>global.json</c> file.
/// </summary>
internal sealed class GlobalJsonUpgradeAction : DotNetUpgradeAction
{
    private readonly Func<JsonNode, CancellationToken, Task<JsonNode>> _applyChanges;

    /// <summary>
    /// Construct a <see cref="GlobalJsonUpgradeAction"/>.
    /// </summary>
    /// <param name="id">Id of the upgrade rule that triggered this action.</param>
    /// <param name="title">Short title describing the upgrade action.</param>
    /// <param name="applyChanges">
    /// Function that applies the fix and returns the changed JSON root node.
    /// </param>
    public GlobalJsonUpgradeAction(string id, string title, Func<JsonNode, CancellationToken, Task<JsonNode>> applyChanges) : base(id, title)
    {
        _applyChanges = applyChanges;
    }

    public override async Task ApplyChanges(WorkspaceInfo workspace, CancellationToken cancellationToken = default)
    {
        Debug.Assert(workspace.DotNetWorkspace is not null);

        JsonNode? globalJsonNode;

        {
            using var globalJsonStream = workspace.DotNetWorkspace.OpenGlobalJsonStream();
            Debug.Assert(globalJsonStream is not null);

            globalJsonNode = GlobalJson.ParseAsNode(globalJsonStream);
            Debug.Assert(globalJsonNode is not null);

            globalJsonNode = await _applyChanges(globalJsonNode, cancellationToken).ConfigureAwait(false);
        }

        {
            using var globalJsonStream = workspace.DotNetWorkspace.OpenGlobalJsonStream();
            Debug.Assert(globalJsonStream is not null);

            await GlobalJson.SerializeAsync(globalJsonStream, globalJsonNode, cancellationToken).ConfigureAwait(false);
        }
    }
}

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Godot.UpgradeAssistant;

partial class WorkspaceInfo
{
    /// <summary>
    /// Reload the <see cref="WorkspaceInfo"/> using the same <c>project.godot</c> file
    /// of the currently opened Godot project.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the step.
    /// </param>
    /// <returns>Task that completes when the workspace is ready.</returns>
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        if (DotNetWorkspace is null)
        {
            // There is no .NET workspace to reload.
            return;
        }

        Debug.Assert(DotNetWorkspace is MSBuildDotNetWorkspaceInfo, "Only MSBuild workspaces can be reloaded.");

        // We need to retrieve the paths to the solution and project files before
        // disposing the .NET workspace, but we need to dispose the workspace before
        // re-opening it.
        string dotnetSolutionFilePath = DotNetWorkspace.Solution.FilePath!;
        string dotnetProjectFilePath = DotNetWorkspace.Project.FilePath!;

        DotNetWorkspace.Dispose();

        var dotnetWorkspace = await OpenDotNetWorkspaceAsync(GodotProject.ProjectFilePath, dotnetSolutionFilePath, dotnetProjectFilePath, cancellationToken).ConfigureAwait(false);
        Debug.Assert(dotnetWorkspace is not null);

        DotNetWorkspace = dotnetWorkspace;
    }
}

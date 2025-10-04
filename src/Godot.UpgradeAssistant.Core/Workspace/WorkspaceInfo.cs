using System;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes a workspace for a Godot .NET project.
/// </summary>
public sealed partial class WorkspaceInfo : IDisposable
{
    /// <summary>
    /// Parsed information from the <c>project.godot</c> file.
    /// </summary>
    public GodotProject GodotProject { get; }

    /// <summary>
    /// The loaded .NET workspace if this Godot project contained one;
    /// otherwise, the workspace will be <see langword="null"/>.
    /// </summary>
    public DotNetWorkspaceInfo? DotNetWorkspace { get; private set; }

    /// <summary>
    /// Construct a <see cref="WorkspaceInfo"/> from a Godot project.
    /// </summary>
    /// <param name="project">The godot project.</param>
    public WorkspaceInfo(GodotProject project)
    {
        GodotProject = project;
    }

    /// <summary>
    /// Construct a <see cref="WorkspaceInfo"/> from a Godot project and a .NET workspace.
    /// </summary>
    /// <param name="project">The godot project.</param>
    /// <param name="dotnetWorkspace">The .NET workspace.</param>
    public WorkspaceInfo(GodotProject project, DotNetWorkspaceInfo dotnetWorkspace) : this(project)
    {
        DotNetWorkspace = dotnetWorkspace;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        DotNetWorkspace?.Dispose();
    }
}

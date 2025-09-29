using System;

namespace Godot.UpgradeAssistant.Tests;

/// <summary>
/// Convenient wrapper over <see cref="WorkspaceInfo"/> for in-memory tests
/// that provides access to the underlying <see cref="MemoryDotNetWorkspaceInfo"/>
/// without having to cast it manually.
/// </summary>
internal sealed class MemoryWorkspaceInfo : IDisposable
{
    private WorkspaceInfo _workspaceInfo;

    public GodotProject GodotProject =>
        _workspaceInfo.GodotProject;

    public MemoryDotNetWorkspaceInfo DotNetWorkspace =>
        (MemoryDotNetWorkspaceInfo)_workspaceInfo.DotNetWorkspace!;

    public MemoryWorkspaceInfo(GodotProject godotProject, MemoryDotNetWorkspaceInfo dotnetWorkspace)
    {
        _workspaceInfo = new WorkspaceInfo(godotProject, dotnetWorkspace);
    }

    public static implicit operator WorkspaceInfo(MemoryWorkspaceInfo info) => info._workspaceInfo;

    public void Dispose()
    {
        _workspaceInfo.Dispose();
    }
}

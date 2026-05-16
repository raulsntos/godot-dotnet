using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Godot.EditorIntegration.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Godot.EditorIntegration.Workspace;

internal sealed partial class DotNetWorkspace : IDisposable
{
    /// <summary>
    /// Path to the <c>.csproj</c> file that defines the workspace.
    /// The workspace will be reloaded automatically when this file or any of its files change on disk.
    /// </summary>
    private string _projectPath;

    public string ProjectPath => _projectPath;

    // These fields together represent the state of the workspace and should be in sync.
    // We can update the solution and compilation when individual documents change, to avoid
    // a full reload which can take a few seconds, but we still need to reload the entire
    // workspace when the .csproj file changes or documents are added/removed.
    private MSBuildWorkspace? _workspace;
    private Solution? _solution;
    private ProjectId? _projectId;
    private Compilation? _compilation;

    public DotNetWorkspaceState State
    {
        get => field;
        private set
        {
            field = value;
            EditorInternal.StatusIndicatorNotifyStateChanged();
        }
    }

    private readonly ManualResetEventSlim _initialLoadCompleted = new(initialState: false);
    private CancellationTokenSource _reloadCts = new();

    private FileSystemWatcher? _watcher;

    private bool _disposed;

    public bool IsAvailable
    {
        get
        {
            if (_disposed)
            {
                return false;
            }

            return _workspace is not null
                && _solution is not null
                && _projectId is not null
                && _compilation is not null;
        }
    }

    public static DotNetWorkspace Create(string csprojPath)
    {
        return new DotNetWorkspace(csprojPath);
    }

    private DotNetWorkspace(string projectPath)
    {
        UpdateProjectPath(projectPath);
        _initialLoadCompleted.Wait();
    }

    [MemberNotNull(nameof(_projectPath))]
    public void UpdateProjectPath(string projectPath)
    {
        if (_projectPath == projectPath)
        {
            return;
        }

        _projectPath = projectPath;

        SetupFileSystemWatcher();
        ScheduleReload();
    }

    public Document? GetDocumentForSyntax(SyntaxNode syntaxNode)
    {
        return _solution?.GetDocument(syntaxNode.SyntaxTree);
    }

    public Document? GetDocumentForFilePath(string filePath)
    {
        var solution = _solution;
        if (solution is null)
        {
            // The workspace is not available.
            return null;
        }

        var documentId = solution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();
        if (documentId is null)
        {
            // The workspace may be outdated and not know about this file yet, but this is
            // unlikely to be a problem in practice because the file system watcher should
            // reload the workspace when that happens.
            return null;
        }

        // Update the document to ensure the content is in sync with the file on disk,
        // in case it was changed before the workspace was reloaded.
        solution = solution.WithDocumentTextLoader(
            documentId,
            new FileTextLoader(filePath, defaultEncoding: null),
            PreservationMode.PreserveValue);

        return solution.GetDocument(documentId);
    }

    public IEnumerable<ITypeSymbol> FindTypeSymbols(string typeName)
    {
        var compilation = _compilation;
        if (compilation is null)
        {
            return [];
        }

        if (_typeCache.TryGetValue(typeName, out var cachedTypes))
        {
            return cachedTypes;
        }

        // The type is not in the cache, if it exists it should be added to the cache eventually
        // when the background caching completes.
        return [];
    }

    public bool TryApplyChanges(Solution newSolution)
    {
        if (_workspace is null || _solution is null || _projectId is null)
        {
            return false;
        }

        bool applied = _workspace.TryApplyChanges(newSolution);
        if (applied)
        {
            // Determine which documents changed so we can update the type cache incrementally.
            var changedDocumentIds = newSolution
                .GetChanges(_solution)
                .GetProjectChanges()
                .SelectMany(static p => p.GetChangedDocuments())
                .ToList();

            // Keep our snapshot in sync with what was just written to disk.
            _solution = _workspace.CurrentSolution;

            // Update the type cache incrementally for each changed document,
            // without needing to re-derive the entire compilation.
            foreach (var documentId in changedDocumentIds)
            {
                // No need to wait for the caching to complete.
                _ = CacheTypeSymbolsForDocumentAsync(documentId, _reloadCts.Token);
            }
        }

        return applied;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _watcher?.Dispose();
        _reloadCts.Cancel();
        _workspace?.Dispose();
        _initialLoadCompleted.Dispose();
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Godot.EditorIntegration.Workspace;

partial class DotNetWorkspace
{
    private async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        MSBuildWorkspace? newWorkspace = null;
        try
        {
            newWorkspace = MSBuildWorkspace.Create();
            var project = await newWorkspace.OpenProjectAsync(_projectPath, cancellationToken: cancellationToken).ConfigureAwait(false);
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation is null)
            {
                // Leave existing state unchanged.
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Swap in the new workspace atomically.
            var oldWorkspace = _workspace;
            _compilation = compilation;
            _solution = newWorkspace.CurrentSolution;
            _projectId = project.Id;

            // Swap the current workspace with the new one after we've fully initialized it.
            (_workspace, newWorkspace) = (newWorkspace, null);

            ClearTypeCache();
            oldWorkspace?.Dispose();

            await CacheTypeSymbolsAsync(cancellationToken);
        }
        finally
        {
            // After we swap the workspace, this is null so it won't be disposed.
            // But if something goes wrong before the swap, this will ensure we don't leave
            // a partially initialized workspace around.
            newWorkspace?.Dispose();
        }
    }

    private void SetupFileSystemWatcher()
    {
        _watcher?.Dispose();
        _watcher = null;

        string? projectDirectory = Path.GetDirectoryName(_projectPath);
        if (projectDirectory is null)
        {
            return;
        }

        if (!Directory.Exists(projectDirectory))
        {
            return;
        }

        _watcher = new FileSystemWatcher(projectDirectory)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };
        _watcher.Changed += OnFileEvent;
        _watcher.Created += OnFileEvent;
        _watcher.Deleted += OnFileEvent;
        _watcher.Renamed += OnFileRenamed;
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        var solution = _solution;
        if (solution is null)
        {
            // The workspace is not available. Wait until it's initialized which will do a full load.
            return;
        }

        if (e.FullPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            // The C# project itself changed, we need to do a full reload to pick up the new set of documents.
            ScheduleReload();
            return;
        }

        if (!e.FullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            // Any non-.cs file could be consumed by a source generator.
            // Trigger a full reload so that any generated symbols stay accurate.
            ScheduleReload();
            return;
        }

        if (e.ChangeType == WatcherChangeTypes.Created)
        {
            var projectId = _projectId;
            if (projectId is null)
            {
                // Workspace not ready, the full load will take care of it.
                return;
            }

            // Add the new document to the solution snapshot so the type cache can be updated
            // without a full MSBuild re-evaluation. The document will be properly included
            // in the next full reload.
            var documentId = DocumentId.CreateNewId(projectId);
            _solution = solution.AddDocument(DocumentInfo.Create(
                documentId,
                name: Path.GetFileNameWithoutExtension(e.FullPath),
                loader: new FileTextLoader(e.FullPath, defaultEncoding: null),
                filePath: e.FullPath));

            _ = CacheTypeSymbolsForDocumentAsync(documentId, _reloadCts.Token);
        }
        else if (e.ChangeType == WatcherChangeTypes.Deleted)
        {
            var documentIds = solution.GetDocumentIdsWithFilePath(e.FullPath);
            if (documentIds.Length == 0)
            {
                // File wasn't tracked, nothing to do.
                return;
            }

            var documentId = documentIds[0];
            _solution = solution.RemoveDocument(documentId);

            _ = CacheTypeSymbolsForDocumentAsync(documentId, _reloadCts.Token);
        }
        else if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            var documentId = solution.GetDocumentIdsWithFilePath(e.FullPath).FirstOrDefault();
            if (documentId is null)
            {
                // File wasn't tracked, nothing to do.
                // The full reload will pick it up eventually if it becomes relevant.
                return;
            }

            _solution = solution.WithDocumentTextLoader(
                documentId,
                new FileTextLoader(e.FullPath, defaultEncoding: null),
                PreservationMode.PreserveValue);

            _ = CacheTypeSymbolsForDocumentAsync(documentId, _reloadCts.Token);
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // Any renamed file could affect the workspace so we schedule a full reload.
        ScheduleReload();
    }

    private void ScheduleReload()
    {
        // Cancel any pending reload and replace it with a new one after a short debounce.
        // This means a burst of file-system events triggers only one reload.
        var newCts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _reloadCts, newCts);
        oldCts.CancelAsync();

        bool isInitialLoad = State == DotNetWorkspaceState.NotOpened;
        State = DotNetWorkspaceState.Opening;

        if (!File.Exists(_projectPath))
        {
            State = DotNetWorkspaceState.ProjectNotFound;
            _initialLoadCompleted.Set();
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                // Debounce to avoid too many reloads in quick succession, unless it's the first load.
                if (!isInitialLoad)
                {
                    await Task.Delay(500, newCts.Token).ConfigureAwait(false);
                }

                // Check that we weren't canceled while waiting.
                newCts.Token.ThrowIfCancellationRequested();

                // Full workspace reload.
                await ReloadAsync(newCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                // Reload failed. We'll keep the existing workspace unchanged.
                // The next file-system event will trigger another attempt.
                // TODO(@raulsntos): Since these reloads are scheduled in the background, this can get noisy, we may not want to actually print an error every time it happens because we may prefer a more graceful degradation where the workspace just fails silently and hopefully it will be reloaded successfully before the user attempts to call any of the methods that need it.
                GD.PrintErr($"Failed to reload C# workspace: {exception}");
            }
            finally
            {
                // Only update the state if this is still the active reload.
                // If we were superseded by a newer reload, that one will do it.
                if (_reloadCts == newCts)
                {
                    // TODO(@raulsntos): Whether the reload succeeded or failed, we have to update the status indicator and status panel to indicate the current state of the workspace. We should probably also expose the exception message in the status panel when it fails, to make it easier to diagnose issues.
                    State = IsAvailable ? DotNetWorkspaceState.Opened : DotNetWorkspaceState.FailedToOpen;
                    _initialLoadCompleted.Set();
                }
            }
        });
    }
}

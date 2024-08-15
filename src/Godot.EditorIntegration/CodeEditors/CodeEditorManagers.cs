using System;

namespace Godot.EditorIntegration.CodeEditors;

/// <summary>
/// Provides integration with external code editors using the available
/// <see cref="CodeEditorManager"/> types.
/// </summary>
internal sealed class CodeEditorManagers : IDisposable
{
    private CodeEditorManager? _currentEditorManager;

    /// <summary>
    /// Get an <see cref="CodeEditorManager"/> instance that corresponds to the given <paramref name="editorId"/>.
    /// If the current manager instance matches the editor type, it will return that instance; otherwise, it will
    /// dispose the current manager instance and create a new one of the expected type.
    /// </summary>
    /// <param name="editorId">The identifier for the requested code editor.</param>
    /// <returns>An editor manager instance for the requested code editor.</returns>
    private CodeEditorManager GetEditorManager(CodeEditorId editorId)
    {
        return editorId switch
        {
            CodeEditorId.VisualStudio when OperatingSystem.IsWindows() =>
                GetEditorManagerOfType<VisualStudioManager>(),
            CodeEditorId.VisualStudio when !OperatingSystem.IsWindows() =>
                throw new PlatformNotSupportedException(SR.PlatformNotSupported_VisualStudio),

            CodeEditorId.VSCode =>
                GetEditorManagerOfType<VisualStudioCodeManager>(),

            CodeEditorId.Rider =>
                GetEditorManagerOfType<RiderManager>(),

            CodeEditorId.CustomEditor =>
                GetEditorManagerOfType<CustomEditorManager>(),

            _ => throw new ArgumentException(SR.FormatArgument_CodeEditorManagerNotFound(editorId), nameof(editorId)),
        };

        CodeEditorManager GetEditorManagerOfType<TEditorManager>() where TEditorManager : CodeEditorManager, new()
        {
            if (_currentEditorManager is not TEditorManager)
            {
                if (_currentEditorManager is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _currentEditorManager = new TEditorManager();
            }

            return _currentEditorManager;
        }
    }

    /// <summary>
    /// Open the file at <paramref name="filePath"/> in the code editor identified
    /// by <paramref name="editorId"/> with the cursor on the given line and column.
    /// </summary>
    /// <param name="editorId">Code editor identifier.</param>
    /// <param name="filePath">Absolute path of the file to open in the editor.</param>
    /// <param name="line">Line at which to place the cursor. Index is 1-based.</param>
    /// <param name="column">Column at which to place the cursor. Index is 1-based.</param>
    /// <returns>
    /// The error that occurred when trying to launch the editor,
    /// or <see cref="Error.Ok"/> if the editor launched successfully.
    /// </returns>
    public Error OpenInEditor(CodeEditorId editorId, string filePath, int line, int column)
    {
        if (editorId == CodeEditorId.None)
        {
            // Not an error. Tells the caller to fallback to the
            // global external editor settings or the built-in editor.
            return Error.Unavailable;
        }

        try
        {
            var manager = GetEditorManager(editorId);
            return manager.Launch(filePath, line, column);
        }
        catch (Exception e)
        {
            GD.PushError(SR.FormatCodeEditorErrorOnLaunch(editorId, e.Message));
            return Error.Failed;
        }
    }

    /// <summary>
    /// Open the file at <paramref name="filePath"/> in the current code editor
    /// with the cursor on the given line and column. The current code editor
    /// is determined by the <see cref="EditorSettingNames.ExternalEditor"/> setting.
    /// </summary>
    /// <param name="filePath">Absolute path of the file to open in the editor.</param>
    /// <param name="line">Line at which to place the cursor. Index is 1-based.</param>
    /// <param name="column">Column at which to place the cursor. Index is 1-based.</param>
    /// <returns>
    /// The error that occurred when trying to launch the editor,
    /// or <see cref="Error.Ok"/> if the editor launched successfully.
    /// </returns>
    public Error OpenInCurrentEditor(string filePath, int line, int column)
    {
        var editorSettings = EditorInterface.Singleton.GetEditorSettings();
        var codeEditorId = editorSettings.GetSetting(EditorSettingNames.ExternalEditor).As<CodeEditorId>();
        return OpenInEditor(codeEditorId, filePath, line, column);
    }

    public void Dispose()
    {
        if (_currentEditorManager is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

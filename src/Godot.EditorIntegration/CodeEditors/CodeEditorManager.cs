using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot.EditorIntegration.Utils;

namespace Godot.EditorIntegration.CodeEditors;

/// <summary>
/// Implements support for launching an external code editor.
/// </summary>
internal abstract class CodeEditorManager
{
    /// <summary>
    /// Open the file at <paramref name="filePath"/> in the code editor,
    /// with the cursor on the given line and column numbers.
    /// </summary>
    /// <param name="filePath">Absolute path of the file to open in the editor.</param>
    /// <param name="line">Line at which to place the cursor. Index is 1-based.</param>
    /// <param name="column">Column at which to place the cursor. Index is 1-based.</param>
    /// <returns>
    /// The error that occurred when trying to launch the editor,
    /// or <see cref="Error.Ok"/> if the editor launched successfully.
    /// </returns>
    public Error Launch(string filePath, int line, int column)
    {
        return LaunchCore(filePath, line, column);
    }

    protected abstract Error LaunchCore(string filePath, int line, int column);

    protected static Process StartProcess(string command, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo(command, arguments)
        {
            CreateNoWindow = true,
        };

        if (OS.Singleton.IsStdOutVerbose())
        {
            Console.WriteLine(startInfo.GetCommandLineDisplay());
        }

        var process = new Process() { StartInfo = startInfo };
        process.Start();

        if (OperatingSystem.IsWindows() && process.Id > 0)
        {
            // Allows application to focus itself.
            User32Dll.AllowSetForegroundWindow(process.Id);
        }

        return process;
    }
}

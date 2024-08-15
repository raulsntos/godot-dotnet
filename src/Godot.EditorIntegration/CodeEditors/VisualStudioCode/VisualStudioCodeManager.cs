using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot.EditorIntegration.Internals;
using Godot.EditorIntegration.Utils;

namespace Godot.EditorIntegration.CodeEditors;

internal sealed class VisualStudioCodeManager : CodeEditorManager
{
    // The package path is '/Applications/Visual Studio Code.app'
    private const string VSCodeBundleId = "com.microsoft.VSCode";

    private static string? _vsCodePath;

    private static readonly string[] _vsCodeNames =
    [
        "code", "code-oss",
        "vscode", "vscode-oss",
        "visual-studio-code", "visual-studio-code-oss"
    ];

    protected override Error LaunchCore(string filePath, int line, int column)
    {
        string command;

        var args = new List<string>();

        if (OperatingSystem.IsMacOS() && EditorInternal.IsMacOSAppBundleInstalled(VSCodeBundleId))
        {
            command = "/usr/bin/open";

            args.Add("-b");
            args.Add(VSCodeBundleId);

            // The reusing of existing windows made by the 'open' command might not choose a window that is
            // editing our folder. It's better to ask for a new window and let VSCode do the window management.
            args.Add("-n");

            // The open process must wait until the application finishes (which is instant in VSCode's case)
            args.Add("--wait-apps");

            args.Add("--args");
        }
        else
        {
            if (string.IsNullOrEmpty(_vsCodePath) || !File.Exists(_vsCodePath))
            {
                // Try to search it again if it wasn't found last time or if it was removed from its location.
                _vsCodePath = _vsCodeNames
                    .Select(ProcessUtils.PathWhich)
                    .FirstOrDefault(path => path is not null);
            }

            if (string.IsNullOrEmpty(_vsCodePath))
            {
                GD.PushError(SR.FormatCodeEditorErrorNotFound("VSCode"));
                return Error.FileNotFound;
            }

            command = _vsCodePath;
        }

        args.Add(Path.GetDirectoryName(EditorPath.ProjectSlnPath)!);

        if (line >= 0)
        {
            args.Add("-g");
            args.Add($"{filePath}:{line}:{column}");
        }
        else
        {
            args.Add(filePath);
        }

        StartProcess(command, args);
        return Error.Ok;
    }
}

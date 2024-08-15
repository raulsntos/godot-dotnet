using System;
using System.Collections.Generic;
using System.IO;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.CodeEditors;

internal sealed class VisualStudioManager : CodeEditorManager
{
    protected override Error LaunchCore(string filePath, int line, int column)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_VisualStudio);
        }

        string command = "dotnet";

        List<string> args =
        [
            Path.Join(EditorPath.EditorAssembliesPath, "Godot.EditorIntegration.OpenVisualStudio.dll"),
            EditorPath.ProjectSlnPath,
            line >= 0 ? $"{filePath};{line};{column}" : filePath,
        ];

        StartProcess(command, args);
        return Error.Ok;
    }
}

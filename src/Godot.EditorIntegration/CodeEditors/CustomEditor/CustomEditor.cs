using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Text;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.CodeEditors;

internal sealed class CustomEditorManager : CodeEditorManager
{
    private string? _cachedExecArgs;

    private readonly List<CompositeFormat> _compositeFormats = [];

    // Do not change the order of these names.
    // They must match the order in the string.Format invocation.
    private readonly string[] _formatterNames =
    [
        "line",
        "col",
        "file",
        "project",
    ];

    protected override Error LaunchCore(string filePath, int line, int column)
    {
        var editorSettings = EditorInterface.Singleton.GetEditorSettings();

        string projectDirPath = Path.GetDirectoryName(EditorPath.ProjectSlnPath)!;

        string execCommand = editorSettings.GetSetting(EditorSettingNames.CustomExecPath).As<string>();
        string execArgs = editorSettings.GetSetting(EditorSettingNames.CustomExecPathArgs).As<string>();

        if (string.IsNullOrWhiteSpace(execArgs))
        {
            // If no arguments were provided, at least pass in the file path.
            StartProcess(execCommand, [filePath]);
            return Error.Ok;
        }

        if (_cachedExecArgs != execArgs)
        {
            // The cached exec args are outdated, so the composite formats need to be re-created.
            ParseCommandLineArguments(execArgs);
        }

        StartProcess(execCommand, FormatCommandLineArguments(projectDirPath, filePath, line, column));

        return Error.Ok;
    }

    private void ParseCommandLineArguments(string execArgs)
    {
        _compositeFormats.Clear();

        foreach (string execArg in CliParser.SplitCommandLine(execArgs))
        {
            string arg = execArg;

            // Replace the parameters with indexes so it can be used with string.Format
            // to prevent multiple replacements (in case one of the values contains another
            // parameter).
            for (int i = 0; i < _formatterNames.Length; i++)
            {
                arg = arg.Replace("{" + _formatterNames[i] + "}", "{" + i + "}", StringComparison.OrdinalIgnoreCase);
            }

            _compositeFormats.Add(CompositeFormat.Parse(arg));
        }

        _cachedExecArgs = execArgs;
    }

    private IEnumerable<string> FormatCommandLineArguments(string project, string filePath, int line, int column)
    {
        foreach (var format in _compositeFormats)
        {
            yield return string.Format(CultureInfo.InvariantCulture, format,
                // Do not change the order of these arguments.
                // They must match the order in _formatterNames.
                line,
                column,
                filePath,
                project);
        }
    }
}

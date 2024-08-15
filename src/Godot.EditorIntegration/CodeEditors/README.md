# Godot.EditorIntegration (code editors)

This directory contains support for multiple external code editors or IDEs, so that Godot can use them to open and edit the .NET project.

We only intend to support the main C# code editors:

- [Visual Studio](./VisualStudio)
- [JetBrains Rider](./JetBrainsRider)
- [VSCode](./VisualStudioCode)

> [!NOTE]
> We can't support every code editor that exists out there, so we choose to support the main code editors used for C#. Other code editors can be used in Godot with the [CustomEditor](./CustomEditor) support that allows the user to specify manually how to open any code editor.

## Implementation

To add support for a code editor, we implement a `CodeEditorManager` class and add its instantiation to `CodeEditorManagers.GetExternalEditorManager`.

Implementing `CodeEditorManager` only requires overriding the `LaunchCore` method, which should open the corresponding code editor. Usually opening a code editor can be done by executing a command with the right arguments (i.e.: the path to the .NET solution, and optionally the path to a .cs file and the line and column).

```csharp
protected override Error LaunchCore(string filePath, int line, int column)
{
	// Use the `code` command to open VSCode, passing the path to the project directory,
	// and the `-g` argument specifying the path to the .cs file and the line and column.
	string dirPath = Path.GetDirectoryName(EditorPath.ProjectSlnPath);
	StartProcess("code", [dirPath, "-g", $"{filePath}:{line}:{column}"]);
	return Error.Ok;
}
```

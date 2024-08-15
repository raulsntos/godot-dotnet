namespace Godot.EditorIntegration.CodeEditors;

internal enum CodeEditorId : long
{
    None = 0,
    VisualStudio = 1, // Windows-only.
    VSCode = 4,
    Rider = 5,
    CustomEditor = 6,
}

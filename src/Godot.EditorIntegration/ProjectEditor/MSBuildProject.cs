using Microsoft.Build.Construction;

namespace Godot.EditorIntegration.ProjectEditor;

internal sealed class MSBuildProject
{
    public ProjectRootElement Root { get; }

    public bool HasUnsavedChanges { get; set; }

    public void Save() => Root.Save();

    public void Save(string path) => Root.Save(path);

    public MSBuildProject(ProjectRootElement root)
    {
        Root = root;
    }

    public static MSBuildProject? Open(string path)
    {
        var root = ProjectRootElement.Open(path);
        return root is not null ? new MSBuildProject(root) : null;
    }

    public static MSBuildProject Create(string path)
    {
        var root = ProjectRootElement.Create(path);
        return new MSBuildProject(root);
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

namespace Godot.EditorIntegration.ProjectEditor;

internal sealed class DotNetSolution
{
    private string _directoryPath;

    private readonly Dictionary<string, ProjectInfo> _projects = [];

    public string Name { get; }

    public required string DirectoryPath
    {
        get => _directoryPath;
        [MemberNotNull(nameof(_directoryPath))]
        set => _directoryPath = Path.IsPathFullyQualified(value) ? value : Path.GetFullPath(value);
    }

    public DotNetSolution(string name)
    {
        Name = name;
    }

    public sealed class ProjectInfo
    {
        public required string Guid { get; set; }
        public required string PathRelativeToSolution { get; set; }
        public List<string> Configs { get; set; } = [];
    }

    public void AddNewProject(string name, ProjectInfo projectInfo)
    {
        _projects[name] = projectInfo;
    }

    public bool HasProject(string name)
    {
        return _projects.ContainsKey(name);
    }

    public ProjectInfo GetProjectInfo(string name)
    {
        return _projects[name];
    }

    public bool RemoveProject(string name)
    {
        return _projects.Remove(name);
    }

    public void Save()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            throw new FileNotFoundException(SR.FileNotFound_SolutionDirectory);
        }

        var projectsDecl = new StringBuilder();
        var slnPlatformsCfg = new StringBuilder();
        var projPlatformsCfg = new StringBuilder();

        bool isFirstProject = true;
        foreach (var (projectName, projectInfo) in _projects)
        {
            if (!isFirstProject)
            {
                projectsDecl.AppendLine();
            }

            string projectPath = projectInfo.PathRelativeToSolution.Replace("/", "\\");
            projectsDecl.Append(CultureInfo.InvariantCulture, $$"""
                Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{{projectName}}", "{{projectPath}}", "{{{projectInfo.Guid}}}"
                EndProject
                """);

            for (int i = 0; i < projectInfo.Configs.Count; i++)
            {
                string config = projectInfo.Configs[i];

                if (i != 0 || !isFirstProject)
                {
                    slnPlatformsCfg.AppendLine();
                    projPlatformsCfg.AppendLine();
                }

                slnPlatformsCfg.Append(CultureInfo.InvariantCulture, $"""
                    		{config}|Any CPU = {config}|Any CPU
                    """);
                projPlatformsCfg.Append(CultureInfo.InvariantCulture, $$"""
                    		{{{projectInfo.Guid}}}.{{config}}|Any CPU.ActiveCfg = {{config}}|Any CPU
                    		{{{projectInfo.Guid}}}.{{config}}|Any CPU.Build.0 = {{config}}|Any CPU
                    """);
            }

            isFirstProject = false;
        }

        string solutionPath = Path.Join(DirectoryPath, $"{Name}.sln");
        string content = $"""
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            {projectsDecl}
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            {slnPlatformsCfg}
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            {projPlatformsCfg}
            	EndGlobalSection
            EndGlobal

            """;

        File.WriteAllText(solutionPath, content, Encoding.UTF8); // UTF-8 with BOM
    }
}

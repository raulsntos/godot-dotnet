using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Implements <see cref="DotNetWorkspaceInfo"/> for a real .NET workspace in the filesystem.
/// </summary>
public sealed class MSBuildDotNetWorkspaceInfo : DotNetWorkspaceInfo, IDisposable
{
    /// <summary>
    /// The MSBuild workspace for the Godot project.
    /// </summary>
    public override Workspace Workspace { get; }

    /// <summary>
    /// The loaded project that represents the Godot project in the solution.
    /// </summary>
    public override Project Project { get; }

    /// <summary>
    /// The loaded project root element that represents the Godot project in the solution.
    /// </summary>
    public ProjectRootElement ProjectRootElement { get; }

    /// <summary>
    /// Path to the <c>global.json</c> file located from the Godot working directory.
    /// </summary>
    private readonly string? _globalJsonPath;

    /// <summary>
    /// Constructs a <see cref="MSBuildDotNetWorkspaceInfo"/> from the specified workspace information.
    /// </summary>
    /// <param name="workspace">The MSBuild workspace.</param>
    /// <param name="project">The MSBuild project.</param>
    /// <param name="projectRootElement">The MSBuild project root element.</param>
    /// <param name="globalJsonPath">The path to the <c>global.json</c> file.</param>
    public MSBuildDotNetWorkspaceInfo(Workspace workspace, Project project, ProjectRootElement projectRootElement, string? globalJsonPath)
    {
        Workspace = workspace;
        Project = project;
        ProjectRootElement = projectRootElement;
        _globalJsonPath = globalJsonPath;
    }

    /// <inheritdoc/>
    public override ProjectRootElement OpenProjectRootElement()
    {
        return ProjectRootElement;
    }

    /// <inheritdoc/>
    public override void SaveProjectRootElement(ProjectRootElement projectRootElement)
    {
        // Save the project root element to the file it was loaded from.
        projectRootElement.Save();
    }

    /// <inheritdoc/>
    public override Task<SolutionModel> OpenSolutionModelAsync(out ISolutionSerializer solutionSerializer, CancellationToken cancellationToken = default)
    {
        string? solutionMoniker = Solution.FilePath;
        if (string.IsNullOrEmpty(solutionMoniker))
        {
            throw new InvalidOperationException(SR.InvalidOperation_SolutionNotFound);
        }

        var serializer = SolutionSerializers.GetSerializerByMoniker(solutionMoniker);
        if (serializer is null)
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_SolutionSerializerNotAvailableForMoniker(solutionMoniker));
        }

        solutionSerializer = serializer;
        return serializer.OpenAsync(solutionMoniker, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task SaveSolutionModelAsync(SolutionModel solutionModel, ISolutionSerializer solutionSerializer, CancellationToken cancellationToken = default)
    {
        string? solutionMoniker = Solution.FilePath;
        if (string.IsNullOrEmpty(solutionMoniker))
        {
            throw new InvalidOperationException(SR.InvalidOperation_SolutionNotFound);
        }

        return solutionSerializer.SaveAsync(solutionMoniker, solutionModel, cancellationToken);
    }

    /// <inheritdoc/>
    public override bool TryOpenGlobalJsonStream([NotNullWhen(true)] out Stream? stream)
    {
        if (!File.Exists(_globalJsonPath))
        {
            stream = null;
            return false;
        }

        stream = new FileStream(_globalJsonPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        return true;
    }
}

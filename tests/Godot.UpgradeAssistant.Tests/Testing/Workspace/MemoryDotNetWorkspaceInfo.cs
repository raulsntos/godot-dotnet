using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using Microsoft.VisualStudio.SolutionPersistence.Serializer.SlnV12;
using Microsoft.VisualStudio.SolutionPersistence.Serializer.Xml;

namespace Godot.UpgradeAssistant.Tests;

/// <summary>
/// Implements <see cref="DotNetWorkspaceInfo"/> for an in-memory adhoc .NET workspace for testing purposes.
/// </summary>
internal sealed class MemoryDotNetWorkspaceInfo : DotNetWorkspaceInfo
{
    public override Workspace Workspace { get; }

    public TestFile ProjectFile { get; private set; }

    public override Microsoft.CodeAnalysis.Project Project { get; }

    public TestFile SolutionFile { get; private set; }

    private readonly ISolutionSerializer _solutionSerializer;

    public TestFile? GlobalJsonFile { get; private set; }

    public MemoryDotNetWorkspaceInfo(TestFile solutionFile, TestFile projectFile, TestFile? globalJsonFile = null)
    {
        var workspace = new AdhocWorkspace();
        Workspace = workspace;

        ProjectFile = projectFile;
        SolutionFile = solutionFile;
        GlobalJsonFile = globalJsonFile;

        string projectName = Path.GetFileNameWithoutExtension(projectFile.FileName);

        var solution = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), filePath: solutionFile.FileName, projects: [
            ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), projectName, projectName, LanguageNames.CSharp, filePath: projectFile.FileName),
        ]);
        workspace.AddSolution(solution);

        Project = Solution.Projects.First();

        _solutionSerializer = SolutionSerializers.GetSerializerByMoniker(solutionFile.FileName)
            ?? throw new InvalidOperationException($"No solution serializer found for file '{solutionFile.FileName}'.");
    }

    /// <inheritdoc/>
    public override ProjectRootElement OpenProjectRootElement()
    {
        using var xmlReader = XmlReader.Create(new StringReader(ProjectFile.Content));
        var projectRootElement = ProjectRootElement.Create(xmlReader, ProjectCollection.GlobalProjectCollection, preserveFormatting: true);
        projectRootElement.FullPath = ProjectFile.FileName;
        return projectRootElement;
    }

    /// <inheritdoc/>
    public override void SaveProjectRootElement(ProjectRootElement projectRootElement)
    {
        using var writer = new Utf8StringWriter();
        projectRootElement.Save(writer);
        ProjectFile = ProjectFile with
        {
            Content = writer.ToString(),
        };
    }

    /// <inheritdoc/>
    public override Task<SolutionModel> OpenSolutionModelAsync(out ISolutionSerializer solutionSerializer, CancellationToken cancellationToken = default)
    {
        solutionSerializer = _solutionSerializer;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SolutionFile.Content));
        return solutionSerializer switch
        {
            ISolutionSingleFileSerializer<SlnV12SerializerSettings> serializer =>
                serializer.OpenAsync(stream, cancellationToken),

            ISolutionSingleFileSerializer<SlnxSerializerSettings> serializer =>
                serializer.OpenAsync(stream, cancellationToken),

            _ =>
                throw new InvalidOperationException($"Unrecognized solution serializer '{solutionSerializer.GetType()}'."),
        };
    }

    public override async Task SaveSolutionModelAsync(SolutionModel solutionModel, ISolutionSerializer solutionSerializer, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        switch (solutionSerializer)
        {
            case ISolutionSingleFileSerializer<SlnV12SerializerSettings> serializer:
                await serializer.SaveAsync(stream, solutionModel, cancellationToken).ConfigureAwait(false);
                break;

            case ISolutionSingleFileSerializer<SlnxSerializerSettings> serializer:
                await serializer.SaveAsync(stream, solutionModel, cancellationToken).ConfigureAwait(false);
                break;

            default:
                throw new InvalidOperationException($"Unrecognized solution serializer '{solutionSerializer.GetType()}'.");
        }

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        SolutionFile = SolutionFile with
        {
            Content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false),
        };
    }

    /// <inheritdoc/>
    public override bool TryOpenGlobalJsonStream([NotNullWhen(true)] out Stream? stream)
    {
        if (GlobalJsonFile is null)
        {
            stream = null;
            return false;
        }

        byte[] content = Encoding.UTF8.GetBytes(GlobalJsonFile.Value.Content);
        stream = new GlobalJsonStream(content, content =>
        {
            GlobalJsonFile = GlobalJsonFile.Value with
            {
                Content = Encoding.UTF8.GetString(content),
            };
        });
        return true;
    }

    // IMPORTANT: We need to use UTF-8 encoding to prevent the ProjectRootElement.Save method
    // from writing the XML declaration (<?xml version="1.0" encoding="utf-16"?>).
    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding { get; } = Encoding.UTF8;
    }

    // IMPORTANT: We need to return a stream that copies the data written to it
    // to the underlying TestFile instance so they are kept in sync as changes
    // are made to the 'global.json' file it represents.
    private sealed class GlobalJsonStream : MemoryStream
    {
        private readonly Action<byte[]> _writeAction;

        public GlobalJsonStream(byte[] buffer, Action<byte[]> writeAction) : base(buffer)
        {
            _writeAction = writeAction;
        }

        public override void Flush()
        {
            base.Flush();
            _writeAction(ToArray());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writeAction(ToArray());
            }

            base.Dispose(disposing);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators.Tests;

internal static class CSharpSourceGeneratorVerifier<TSourceGenerator>
where TSourceGenerator : new()
{
    public sealed class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;

            SolutionTransforms.Add((Solution solution, ProjectId projectId) =>
            {
                Project project = solution.GetProject(projectId)!
                    .AddMetadataReference(MetadataReference.CreateFromFile(Constants.GodotBindingsAssembly.Location));

                return project.Solution;
            });
        }
    }

    private static (string FileName, string ContentFileName) MapFileNames(string source)
    {
        return (source, source);
    }

    public static Task Verify(IEnumerable<string> sources, IEnumerable<string> generatedSources)
    {
        return MakeVerifier(sources, generatedSources).RunAsync();
    }

    public static Task Verify(IEnumerable<string> sources, IEnumerable<(string FileName, string ContentFileName)> generatedSources)
    {
        return MakeVerifier(sources.Select(MapFileNames), generatedSources).RunAsync();
    }

    public static Task Verify(IEnumerable<(string FileName, string ContentFileName)> sources, IEnumerable<(string FileName, string ContentFileName)> generatedSources)
    {
        return MakeVerifier(sources, generatedSources).RunAsync();
    }

    public static Test MakeVerifier(IEnumerable<string> sources, IEnumerable<string> generatedSources)
    {
        return MakeVerifier(sources.Select(MapFileNames), generatedSources.Select(MapFileNames));
    }

    public static Test MakeVerifier(IEnumerable<(string FileName, string ContentFileName)> sources, IEnumerable<(string FileName, string ContentFileName)> generatedSources)
    {
        var verifier = new Test();

        verifier.TestState.Sources.AddRange(sources.Select(source => (
            source.FileName,
            SourceText.From(File.ReadAllText(Path.Combine(Constants.SourceFolderPath, source.ContentFileName)), Encoding.UTF8)
        )));

        verifier.TestState.GeneratedSources.AddRange(generatedSources.Select(generatedSource => (
            FullGeneratedSourceName(generatedSource.FileName),
            SourceText.From(File.ReadAllText(Path.Combine(Constants.GeneratedSourceFolderPath, generatedSource.ContentFileName)), Encoding.UTF8)
        )));

        return verifier;
    }

    private static string FullGeneratedSourceName(string name)
    {
        var generatorType = typeof(TSourceGenerator);
        return Path.Combine(generatorType.Namespace!, generatorType.FullName!, name);
    }
}

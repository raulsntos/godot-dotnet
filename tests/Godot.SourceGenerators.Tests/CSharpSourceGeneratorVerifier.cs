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

    public static Task Verify(ICollection<string> sources, ICollection<string> generatedSources)
    {
        return MakeVerifier(sources, generatedSources).RunAsync();
    }

    public static Test MakeVerifier(ICollection<string> sources, ICollection<string> generatedSources)
    {
        var verifier = new Test();

        verifier.TestState.Sources.AddRange(sources.Select(source => (
            source,
            SourceText.From(File.ReadAllText(Path.Combine(Constants.SourceFolderPath, source)), Encoding.UTF8)
        )));

        verifier.TestState.GeneratedSources.AddRange(generatedSources.Select(generatedSource => (
            FullGeneratedSourceName(generatedSource),
            SourceText.From(File.ReadAllText(Path.Combine(Constants.GeneratedSourceFolderPath, generatedSource)), Encoding.UTF8)
        )));

        return verifier;
    }

    private static string FullGeneratedSourceName(string name)
    {
        var generatorType = typeof(TSourceGenerator);
        return Path.Combine(generatorType.Namespace!, generatorType.FullName!, name);
    }
}

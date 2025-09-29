using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Godot.UpgradeAssistant.Tests;

internal static class CSharpAnalyzerVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    public sealed class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    {
        public Test(MetadataReference[] references)
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90;

            SolutionTransforms.Add((Solution solution, ProjectId projectId) =>
            {
                Project project = solution.GetProject(projectId)!
                    .AddMetadataReferences(references);

                return project.Solution;
            });
        }
    }

    public static Task Verify(string sources, DiagnosticResult[] expected, MetadataReference[] references)
    {
        return MakeVerifier([sources], expected, references).RunAsync();
    }

    public static Test MakeVerifier(ICollection<string> sources, DiagnosticResult[] expected, MetadataReference[] references)
    {
        var verifier = new Test(references);

        verifier.TestState.Sources.AddRange(sources.Select(source =>
        {
            return (source, SourceText.From(File.ReadAllText(Path.Combine(Constants.SourceFolderPath, source))));
        }));

        verifier.ExpectedDiagnostics.AddRange(expected);
        return verifier;
    }
}

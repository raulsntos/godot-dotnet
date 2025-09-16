using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Godot.Analyzers.Tests;

internal static class CSharpAnalyzerVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    public sealed class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90;

            SolutionTransforms.Add((Solution solution, ProjectId projectId) =>
            {
                Project project = solution.GetProject(projectId)!
                    .AddMetadataReference(MetadataReference.CreateFromFile(Constants.GodotBindingsAssembly.Location));

                return project.Solution;
            });
        }
    }

    public static Task Verify(string sources, params DiagnosticResult[] expected)
    {
        return MakeVerifier([sources], expected).RunAsync();
    }

    public static Test MakeVerifier(ICollection<string> sources, params DiagnosticResult[] expected)
    {
        var verifier = new Test();

        verifier.TestState.Sources.AddRange(sources.Select(source =>
        {
            return (source, SourceText.From(File.ReadAllText(Path.Combine(Constants.SourceFolderPath, source))));
        }));

        verifier.ExpectedDiagnostics.AddRange(expected);
        return verifier;
    }
}

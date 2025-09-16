using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Godot.Analyzers.Tests;

internal static class CSharpCodeFixVerifier<TCodeFix, TAnalyzer>
    where TCodeFix : CodeFixProvider, new()
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public sealed class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
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

    public static Task Verify(string sources, string fixedSources)
    {
        return MakeVerifier(sources, fixedSources).RunAsync();
    }

    public static Test MakeVerifier(string source, string results)
    {
        var verifier = new Test();

        verifier.TestCode = File.ReadAllText(Path.Combine(Constants.SourceFolderPath, source));
        verifier.FixedCode = File.ReadAllText(Path.Combine(Constants.GeneratedSourceFolderPath, results));

        return verifier;
    }
}

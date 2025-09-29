using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Godot.UpgradeAssistant.Tests;

internal static class CSharpCodeFixVerifier<TCodeFix, TAnalyzer>
    where TCodeFix : CodeFixProvider, new()
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public sealed class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test(MetadataReference[] beforeReferences, MetadataReference[] afterReferences)
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90;

            SolutionTransforms.Add((solution, projectId) =>
            {
                var project = solution.GetProject(projectId);
                project = project!
                    .WithParseOptions(((Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)project.ParseOptions!)
                        .WithLanguageVersion((Microsoft.CodeAnalysis.CSharp.LanguageVersion)1300));
                return project.Solution;
            });

            TestState.AdditionalReferences.AddRange(beforeReferences);
            FixedState.AdditionalReferences.AddRange(afterReferences);
        }
    }

    public static Task Verify(string sources, string fixedSources, MetadataReference[] beforeReferences, MetadataReference[] afterReferences)
    {
        return MakeVerifier(sources, fixedSources, beforeReferences, afterReferences).RunAsync();
    }

    public static Test MakeVerifier(string source, string results, MetadataReference[] beforeReferences, MetadataReference[] afterReferences)
    {
        var verifier = new Test(beforeReferences, afterReferences);

        verifier.TestCode = File.ReadAllText(Path.Combine(Constants.SourceFolderPath, source));
        verifier.FixedCode = File.ReadAllText(Path.Combine(Constants.GeneratedSourceFolderPath, results));

        return verifier;
    }
}

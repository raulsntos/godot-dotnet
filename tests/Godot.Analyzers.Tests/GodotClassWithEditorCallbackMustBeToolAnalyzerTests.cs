using System.Threading.Tasks;

namespace Godot.Analyzers.Tests;

using Verifier = CSharpCodeFixVerifier<AddToolToGodotClassCodeFix, GodotClassWithEditorCallbackMustBeToolAnalyzer>;

public class GodotClassWithEditorCallbackMustBeToolAnalyzerTests
{
    [Fact]
    public async Task GodotClassWithEditorCallbacks()
    {
        await Verifier.Verify("GODOT0004_GodotClassWithEditorCallbacks.cs", "GODOT0004_GodotClassWithEditorCallbacks.fixed.cs");
    }
}

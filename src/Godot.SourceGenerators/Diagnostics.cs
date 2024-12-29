using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class Diagnostics
{
    public const string ERR_PREFIX = "GODOT_";
    public const string ERR_CATEGORY = "Godot.SourceGenerators";

    public static Diagnostic ClassDoesNotHaveCorrectBaseType(
        Location location,
        GodotClassSpec spec
    )
    {
        return Diagnostic.Create(
            new DiagnosticDescriptor(
                $"{ERR_PREFIX}000",
                "Class Does Not Have Correct Base Type",
                messageFormat: "The class `{0}` does not derive from Godot.GodotObject.",
                category: ERR_CATEGORY,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true
            ),
            location,
            spec.FullyQualifiedSymbolName
        );
    }
}

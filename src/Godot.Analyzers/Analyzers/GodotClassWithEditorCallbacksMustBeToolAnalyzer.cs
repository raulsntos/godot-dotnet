using System.Collections.Generic;
using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class GodotClassWithEditorCallbackMustBeToolAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0004_GodotClassWithEditorCallbacksShouldBeTool,
    ]);

    private static readonly HashSet<string> _editorCallbackMethodNames =
    [
        $"{KnownTypeNames.GodotObject}._GetPropertyList",
        $"{KnownTypeNames.GodotObject}._PropertyCanRevert",
        $"{KnownTypeNames.GodotObject}._PropertyGetRevert",
        $"{KnownTypeNames.GodotObject}._ValidateProperty",
        $"{KnownTypeNames.GodotNode}._GetConfigurationWarnings",
        $"{KnownTypeNames.GodotNode}._GetAccessibilityConfigurationWarnings",
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        var containingType = methodSymbol.ContainingType;
        if (containingType is null)
        {
            return;
        }

        if (!containingType.TryGetAttribute(KnownTypeNames.GodotClassAttribute, out var godotClassAttribute))
        {
            // If the class is not registered, the user is likely registering the class manually.
            // So we can't know whether they are registering the class as a tool or not, but it's
            // preferable to avoid annoying false positives, so we bail out.
            return;
        }

        if (IsGodotClassMarkedAsTool(godotClassAttribute))
        {
            // The class is already marked as tool, so there's no issue.
            return;
        }

        if (!IsEditorCallback(methodSymbol))
        {
            // The method is not one of the known editor callbacks, so there's likely no issue.
            // It's possible there are some editor callbacks we don't currently handle, but
            // we don't have to be exhaustive.
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Descriptors.GODOT0004_GodotClassWithEditorCallbacksShouldBeTool,
            location: methodSymbol.Locations[0],
            // Message Format parameters.
            methodSymbol.Name
        ));
    }

    private static bool IsEditorCallback(IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.IsOverride)
        {
            // All engine callbacks are virtual methods that are overridden in user classes.
            return false;
        }

        var overriddenMethod = methodSymbol.OverriddenMethod;
        if (overriddenMethod is null)
        {
            // This should be unreachable since we already checked that the method is an override.
            return false;
        }

        if (overriddenMethod.ContainingAssembly.Name != "Godot.Bindings")
        {
            // All engine callbacks are defined in the Godot.Bindings assembly.
            return false;
        }

        // Check for known editor callback methods.
        string fullMethodName = $"{overriddenMethod.ContainingType.FullQualifiedNameOmitGlobal()}.{methodSymbol.Name}";
        if (_editorCallbackMethodNames.Contains(fullMethodName))
        {
            return true;
        }

        return false;
    }

    private static bool IsGodotClassMarkedAsTool(AttributeData godotClassAttribute)
    {
        foreach (var (key, constant) in godotClassAttribute.NamedArguments)
        {
            if (key == "Tool" && constant.Value as bool? == true)
            {
                return true;
            }
        }

        return false;
    }
}

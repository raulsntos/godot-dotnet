using System.Collections.Immutable;
using System.Linq;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class AttributeOutsideGodotClassAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0001_GodotAttributeHasNoEffectOutsideGodotClass,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.Attribute);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var semanticModel = context.SemanticModel;

        SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(context.Node, context.CancellationToken);
        var attributeConstructorSymbol = symbolInfo.Symbol as IMethodSymbol;
        if (attributeConstructorSymbol is null)
        {
            return;
        }

        var attributeSymbol = attributeConstructorSymbol.ContainingType;
        if (attributeSymbol is null)
        {
            return;
        }

        if (!IsTargetAttribute(attributeSymbol))
        {
            return;
        }

        TypeDeclarationSyntax? containingTypeSyntaxNode = context.Node.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();

        var containingTypeSymbol = semanticModel.GetDeclaredSymbol(containingTypeSyntaxNode, context.CancellationToken);
        if (containingTypeSymbol is null)
        {
            // If there is no containing type symbol, the attribute may be applied to a top-level type
            // like an enum or a delegate.
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0001_GodotAttributeHasNoEffectOutsideGodotClass,
                context.Node.GetLocation(),
                // Message Format parameters.
                attributeSymbol.ToDisplayString()
            ));
            return;
        }

        if (!containingTypeSymbol.HasAttribute(KnownTypeNames.GodotClassAttribute))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0001_GodotAttributeHasNoEffectOutsideGodotClass,
                context.Node.GetLocation(),
                // Message Format parameters.
                attributeSymbol.ToDisplayString()
            ));
        }
    }

    private static bool IsTargetAttribute(INamedTypeSymbol attributeSymbol)
    {
        // These are all the attributes that have no effect outside a class with [GodotClass].
        return attributeSymbol.FullQualifiedNameOmitGlobal() switch
        {
            KnownTypeNames.BindConstructorAttribute or
            KnownTypeNames.BindConstantAttribute or
            KnownTypeNames.BindEnumAttribute or
            KnownTypeNames.BindPropertyAttribute or
            KnownTypeNames.BindMethodAttribute or
            KnownTypeNames.SignalAttribute or
            KnownTypeNames.PropertyGroupAttribute or
            KnownTypeNames.PropertySubgroupAttribute
                => true,

            _
                => false,
        };
    }
}

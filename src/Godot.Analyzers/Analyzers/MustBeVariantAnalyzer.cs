using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class MustBeVariantAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0801_GenericTypeArgumentMustBeVariant,
        Descriptors.GODOT0802_GenericTypeParameterMustBeVariantAnnotated,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.TypeArgumentList);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        // Ignore syntax inside comments.
        if (IsInsideDocumentation(context.Node))
        {
            return;
        }

        var typeArgListSyntax = (TypeArgumentListSyntax)context.Node;

        // Method invocation or variable declaration that contained the type arguments
        var parentSyntax = context.Node.Parent;
        if (parentSyntax is null)
        {
            return;
        }

        var sm = context.SemanticModel;

        for (int i = 0; i < typeArgListSyntax.Arguments.Count; i++)
        {
            var typeSyntax = typeArgListSyntax.Arguments[i];

            // Ignore omitted type arguments, e.g.: List<>, Dictionary<,>, etc.
            if (typeSyntax is OmittedTypeArgumentSyntax)
            {
                continue;
            }

            var typeSymbol = sm.GetSymbolInfo(typeSyntax).Symbol as ITypeSymbol;
            if (typeSymbol is null)
            {
                continue;
            }

            var parentSymbolInfo = sm.GetSymbolInfo(parentSyntax);
            var parentSymbol = parentSymbolInfo.Symbol;
            if (parentSymbol is null)
            {
                if (parentSymbolInfo.CandidateReason == CandidateReason.LateBound)
                {
                    // Invocations on dynamic are late bound so we can't retrieve the symbol.
                    continue;
                }

                if (parentSymbol is null)
                {
                    return;
                }
            }

            if (!ShouldCheckTypeArgument(context, parentSyntax, parentSymbol, typeSyntax, typeSymbol, i))
            {
                return;
            }

            if (typeSymbol is ITypeParameterSymbol typeParamSymbol)
            {
                if (!typeParamSymbol.HasAttribute(KnownTypeNames.MustBeVariantAttribute))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0802_GenericTypeParameterMustBeVariantAnnotated,
                        typeSyntax.GetLocation(),
                        // Message Format parameters.
                        typeSymbol.ToDisplayString()
                    ));
                }
                continue;
            }

            if (!IsSupportedType(context.Compilation, typeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.GODOT0801_GenericTypeArgumentMustBeVariant,
                    typeSyntax.GetLocation(),
                    // Message Format parameters.
                    typeSymbol.ToDisplayString()
                ));
            }
        }
    }

    private static bool IsSupportedType(Compilation compilation, ITypeSymbol type)
    {
        return Marshalling.TryGetMarshallingInformation(compilation, type, out _);
    }

    /// <summary>
    /// Check if the syntax node is inside a documentation syntax.
    /// </summary>
    /// <param name="syntax">Syntax node to check.</param>
    /// <returns><see langword="true"/> if the syntax node is inside a documentation syntax.</returns>
    private static bool IsInsideDocumentation(SyntaxNode? syntax)
    {
        while (syntax is not null)
        {
            if (syntax is DocumentationCommentTriviaSyntax)
            {
                return true;
            }

            syntax = syntax.Parent;
        }

        return false;
    }

    /// <summary>
    /// Check if the given type argument is being used in a type parameter that contains
    /// the <c>MustBeVariantAttribute</c>; otherwise, we ignore the attribute.
    /// </summary>
    /// <param name="context">Context for a syntax node action.</param>
    /// <param name="parentSyntax">The parent node syntax that contains the type node syntax.</param>
    /// <param name="parentSymbol">The symbol retrieved for the parent node syntax.</param>
    /// <param name="typeArgumentSyntax">The type node syntax of the argument type to check.</param>
    /// <param name="typeArgumentSymbol">The symbol retrieved for the type node syntax.</param>
    /// <param name="typeArgumentIndex"></param>
    /// <returns><see langword="true"/> if the type must be variant and must be analyzed.</returns>
    private static bool ShouldCheckTypeArgument(
        SyntaxNodeAnalysisContext context,
        SyntaxNode parentSyntax,
        ISymbol parentSymbol,
        TypeSyntax typeArgumentSyntax,
        ITypeSymbol typeArgumentSymbol,
        int typeArgumentIndex)
    {
        ITypeParameterSymbol? typeParamSymbol = parentSymbol switch
        {
            IMethodSymbol methodSymbol when parentSyntax.Ancestors().Any(s => s is AttributeSyntax) && methodSymbol.ContainingType.TypeParameters.Length > 0
                => methodSymbol.ContainingType.TypeParameters[typeArgumentIndex],

            IMethodSymbol { TypeParameters.Length: > 0 } methodSymbol
                => methodSymbol.TypeParameters[typeArgumentIndex],

            INamedTypeSymbol { TypeParameters.Length: > 0 } typeSymbol
                => typeSymbol.TypeParameters[typeArgumentIndex],

            _
                => null
        };

        if (typeParamSymbol is not null)
        {
            return typeParamSymbol.HasAttribute(KnownTypeNames.MustBeVariantAttribute);
        }

        return false;
    }
}

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.UpgradeAssistant.Providers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal sealed class DeltaParameterTypeCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1010_DeltaParameterType;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [Rule.Id];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the method declaration identified by the diagnostic.
        var declaration = root?
            .FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .First();

        if (declaration is null)
        {
            // Can't apply the code fix without a declaration.
            return;
        }

        bool deltaIsFloat32 = false;
        if (diagnostic.Properties.TryGetValue(PropertyNames.DeltaIsFloat32, out string? deltaIsFloat32Value)
         && (deltaIsFloat32Value?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            deltaIsFloat32 = true;
        }

        var codeAction = CodeAction.Create(
            title: SR.GUA1010_DeltaParameterType_CodeFix,
            equivalenceKey: nameof(DeltaParameterTypeCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, declaration, deltaIsFloat32, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFix(Document document, MethodDeclarationSyntax methodDeclarationSyntax, bool deltaIsFloat32, CancellationToken cancellationToken = default)
    {
        var methodParameters = methodDeclarationSyntax.ParameterList.Parameters;
        if (methodParameters.Count != 1)
        {
            // If we couldn't get the delta parameter, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var deltaParameter = methodParameters[0];

        var deltaTypeKeyword = deltaIsFloat32
            ? SyntaxKind.FloatKeyword
            : SyntaxKind.DoubleKeyword;
        var deltaType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(deltaTypeKeyword));

        var newDeltaParameter = deltaParameter.WithType(deltaType);

        var newParameterList = SyntaxFactory.ParameterList(
            SyntaxFactory.SeparatedList([newDeltaParameter]));

        var newSyntaxNode = methodDeclarationSyntax
            .WithParameterList(newParameterList);

        if (!deltaIsFloat32)
        {
            // When changing the delta parameter type from 'float' to 'double',
            // we may be producing compiler errors if the parameter was being used
            // in expressions expecting a 'float' value.
            // To avoid this, we find all the references to the delta parameter
            // and wrap it in a cast to 'float'.

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (methodDeclarationSyntax.Body is not null)
            {
                var newBody = FindAndReplaceDeltaReferences(semanticModel, deltaParameter, methodDeclarationSyntax.Body, cancellationToken);
                newSyntaxNode = newSyntaxNode.WithBody(newBody);
            }
            else if (methodDeclarationSyntax.ExpressionBody is not null)
            {
                var newBody = FindAndReplaceDeltaReferences(semanticModel, deltaParameter, methodDeclarationSyntax.ExpressionBody, cancellationToken);
                newSyntaxNode = newSyntaxNode.WithExpressionBody(newBody);
            }
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var newRoot = root.ReplaceNode(methodDeclarationSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);
    }

    private static TSyntaxNode FindAndReplaceDeltaReferences<TSyntaxNode>(SemanticModel? semanticModel, ParameterSyntax deltaParameterSyntax, TSyntaxNode root, CancellationToken cancellationToken = default) where TSyntaxNode : SyntaxNode
    {
        var deltaParameterSymbol = semanticModel.GetDeclaredSymbol(deltaParameterSyntax, cancellationToken);

        // Find all the identifier syntax nodes that are bound to the delta parameter symbol.
        var deltaReferences = root
            .DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Where(identifierSyntax =>
            {
                return SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(identifierSyntax).Symbol, deltaParameterSymbol);
            });

        // Wrap syntax nodes in cast expressions.
        var floatType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));
        return root.ReplaceNodes(deltaReferences, (original, changed) =>
        {
            return SyntaxFactory.CastExpression(floatType, changed);
        });
    }
}

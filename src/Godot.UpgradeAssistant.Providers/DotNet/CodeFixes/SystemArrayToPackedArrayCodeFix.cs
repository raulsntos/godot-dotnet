using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Godot.UpgradeAssistant.Providers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal sealed class SystemArrayToPackedArrayCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1011_SystemArrayToPackedArray;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [Rule.Id];

    public override FixAllProvider? GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (!diagnostic.Properties.TryGetValue(nameof(SyntaxKind), out string? syntaxKind))
        {
            // Unknown syntax.
            return;
        }

        // Find the argument syntax identified by the diagnostic.
        SyntaxNode? syntaxNode = syntaxKind switch
        {
            nameof(MethodDeclarationSyntax) =>
                FindSyntax<MethodDeclarationSyntax>(root, diagnosticSpan),

            nameof(ParameterSyntax) =>
                FindSyntax<ParameterSyntax>(root, diagnosticSpan),

            nameof(ArgumentSyntax) =>
                FindSyntax<ArgumentSyntax>(root, diagnosticSpan),

            nameof(InvocationExpressionSyntax) =>
                FindSyntax<InvocationExpressionSyntax>(root, diagnosticSpan),

            _ => null,
        };

        if (syntaxNode is null)
        {
            // Can't apply the code fix without a syntax.
            return;
        }

        var codeAction = CodeAction.Create(
            title: SR.GUA1011_SystemArrayToPackedArray_CodeFix,
            equivalenceKey: nameof(SystemArrayToPackedArrayCodeFix),
            createChangedDocument: cancellationToken => ApplyFix(context.Document, syntaxNode, cancellationToken));

        context.RegisterCodeFix(codeAction, diagnostic);

        static TSyntax? FindSyntax<TSyntax>(SyntaxNode? syntaxNode, TextSpan diagnosticSpan) where TSyntax : SyntaxNode
        {
            return syntaxNode?
                .FindToken(diagnosticSpan.Start).Parent?
                .AncestorsAndSelf()
                .OfType<TSyntax>()
                .First();
        }
    }

    private static Task<Document> ApplyFix(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken = default)
    {
        return syntaxNode switch
        {
            MethodDeclarationSyntax methodDeclarationSyntax =>
                ApplyFixToReturnParameter(document, methodDeclarationSyntax, cancellationToken),

            ParameterSyntax parameterSyntax =>
                ApplyFixToParameter(document, parameterSyntax, cancellationToken),

            ArgumentSyntax argumentSyntax =>
                ApplyFixToArgument(document, argumentSyntax, cancellationToken),

            InvocationExpressionSyntax invocationExpressionSyntax =>
                ApplyFixToInvocationReturn(document, invocationExpressionSyntax, cancellationToken),

            // Unknown syntax kind, return document unchanged.
            _ => Task.FromResult(document),
        };
    }

    private static async Task<Document> ApplyFixToInvocationReturn(Document document, InvocationExpressionSyntax invocationExpressionSyntax, CancellationToken cancellationToken = default)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        // Add ".ToArray()" to the returned packed array.
        var newSyntaxNode = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                invocationExpressionSyntax,
                SyntaxFactory.IdentifierName("ToArray")));

        var newRoot = root.ReplaceNode(invocationExpressionSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ApplyFixToArgument(Document document, ArgumentSyntax argumentSyntax, CancellationToken cancellationToken = default)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel?.GetOperation(argumentSyntax, cancellationToken) is not IArgumentOperation argumentOperation)
        {
            // If we couldn't get the argument operation, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        ITypeSymbol? elementTypeSymbol = GetArrayOrSpanElementTypeSymbol(argumentOperation.Parameter?.Type);
        if (elementTypeSymbol is null)
        {
            // If we couldn't get the element type symbol, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        NameSyntax packedArrayNameSyntax = GetPackedArrayNameSyntax(root, semanticModel, argumentOperation.Parameter!.Type, elementTypeSymbol, cancellationToken);
        var creationExpression = SyntaxFactory.ObjectCreationExpression(packedArrayNameSyntax);

        if (argumentSyntax.Expression is ArrayCreationExpressionSyntax arrayCreationExpression)
        {
            var collectionInitializer = SyntaxFactory.InitializerExpression(
                SyntaxKind.CollectionInitializerExpression,
                arrayCreationExpression.Initializer?.Expressions ?? []);

            creationExpression = creationExpression.WithInitializer(collectionInitializer);
        }
        else if (argumentSyntax.Expression is StackAllocArrayCreationExpressionSyntax stackallocCreationExpression)
        {
            var collectionInitializer = SyntaxFactory.InitializerExpression(
                SyntaxKind.CollectionInitializerExpression,
                stackallocCreationExpression.Initializer?.Expressions ?? []);

            creationExpression = creationExpression.WithInitializer(collectionInitializer);
        }
        else
        {
            var creationArgumentSyntax = argumentSyntax;

            // If the argument contains an enumerable or span invoking ".ToArray", we can remove the invocation,
            // since the packed array constructors can take an enumerable or span directly.
            if (creationArgumentSyntax.Expression is InvocationExpressionSyntax argumentInvocationExpression
             && argumentInvocationExpression.Expression is MemberAccessExpressionSyntax argumentMemberExpression)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(argumentInvocationExpression, cancellationToken).Symbol as IMethodSymbol;
                if (methodSymbol?.Name == "ToArray")
                {
                    // The method is "ToArray" but we still have to check that the containing type
                    // is a valid type to use with the packed array constructor.
                    // It must be IEnumerable<T>, ReadOnlySpan<T>, or any type that can be implicitly converted
                    // to either of them.
                    var containingType = methodSymbol.ContainingType;
                    if (containingType.EqualsGenericType("System.Span<T>")
                     || containingType.EqualsGenericType("System.ReadOnlySpan<T>")
                     || containingType.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.EqualsGenericType("System.Collections.Generic.IEnumerable<T>")))
                    {
                        creationArgumentSyntax = argumentSyntax.WithExpression(argumentMemberExpression.Expression);
                    }
                }
            }

            var argumentList = SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList([creationArgumentSyntax]));

            creationExpression = creationExpression.WithArgumentList(argumentList);
        }

        var newSyntaxNode = argumentSyntax.WithExpression(creationExpression);

        var newRoot = root.ReplaceNode(argumentSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ApplyFixToParameter(Document document, ParameterSyntax parameterSyntax, CancellationToken cancellationToken = default)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel?.GetDeclaredSymbol(parameterSyntax, cancellationToken) is not IParameterSymbol parameterSymbol)
        {
            // If we couldn't get the parameter symbol, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        ITypeSymbol? elementTypeSymbol = GetArrayOrSpanElementTypeSymbol(parameterSymbol.Type);
        if (elementTypeSymbol is null)
        {
            // If we couldn't get the element type symbol, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        NameSyntax packedArrayNameSyntax = GetPackedArrayNameSyntax(root, semanticModel, parameterSymbol.Type, elementTypeSymbol, cancellationToken);

        var newSyntaxNode = parameterSyntax.WithType(packedArrayNameSyntax);

        var newRoot = root.ReplaceNode(parameterSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ApplyFixToReturnParameter(Document document, MethodDeclarationSyntax methodDeclarationSyntax, CancellationToken cancellationToken = default)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            // If we couldn't get the syntax root, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel?.GetDeclaredSymbol(methodDeclarationSyntax, cancellationToken) is not IMethodSymbol methodSymbol)
        {
            // If we couldn't get the method symbol, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        ITypeSymbol? elementTypeSymbol = GetArrayOrSpanElementTypeSymbol(methodSymbol.ReturnType);
        if (elementTypeSymbol is null)
        {
            // If we couldn't get the element type symbol, return the document unchanged.
            // This should be unreachable.
            return document;
        }

        NameSyntax packedArrayNameSyntax = GetPackedArrayNameSyntax(root, semanticModel, methodSymbol.ReturnType, elementTypeSymbol, cancellationToken);

        var newSyntaxNode = methodDeclarationSyntax.WithReturnType(packedArrayNameSyntax);

        var newRoot = root.ReplaceNode(methodDeclarationSyntax, newSyntaxNode);
        return document.WithSyntaxRoot(newRoot);
    }

    private static ITypeSymbol? GetArrayOrSpanElementTypeSymbol(ITypeSymbol? arrayOrSpanTypeSymbol)
    {
        if (arrayOrSpanTypeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            return arrayTypeSymbol.ElementType;
        }
        else if (arrayOrSpanTypeSymbol.EqualsGenericType("System.ReadOnlySpan<T>"))
        {
            INamedTypeSymbol? spanTypeSymbol = arrayOrSpanTypeSymbol as INamedTypeSymbol;
            Debug.Assert(spanTypeSymbol is not null);
            Debug.Assert(spanTypeSymbol.TypeArguments.Length == 1);
            return spanTypeSymbol.TypeArguments[0];
        }

        return null;
    }

    private static NameSyntax GetPackedArrayNameSyntax(SyntaxNode root, SemanticModel semanticModel, ITypeSymbol arrayLikeTypeSymbol, ITypeSymbol elementTypeSymbol, CancellationToken cancellationToken = default)
    {
        if (!Marshalling.TryGetPackedArrayType(arrayLikeTypeSymbol, elementTypeSymbol, out _, out string? packedArrayTypeFullName))
        {
            throw new UnreachableException("Every array or span type used in GodotSharp API should have a corresponding packed array type.");
        }

        string packedArrayTypeName = packedArrayTypeFullName;
        if (IsGodotCollectionsInScope(root, semanticModel, cancellationToken))
        {
            packedArrayTypeName = packedArrayTypeFullName.Substring(packedArrayTypeFullName.LastIndexOf('.') + 1);
        }

        return SyntaxUtils.CreateQualifiedName(packedArrayTypeName);
    }

    private static bool IsGodotCollectionsInScope(SyntaxNode root, SemanticModel semanticModel, CancellationToken cancellationToken = default)
    {
        if (root is not CompilationUnitSyntax compilationUnitSyntax)
        {
            // Can't check if Godot.Collections is in scope without a compilation unit syntax,
            // so we'll just assume it isn't and need to fully-qualify it.
            return false;
        }

        return compilationUnitSyntax.Usings.Any(usingSyntax =>
        {
            var namespaceSymbol = semanticModel.GetSymbolInfo(usingSyntax, cancellationToken).Symbol;
            if (namespaceSymbol is INamespaceSymbol
             && namespaceSymbol.FullQualifiedNameOmitGlobal() == "Godot.Collections")
            {
                return true;
            }

            return false;
        });
    }
}

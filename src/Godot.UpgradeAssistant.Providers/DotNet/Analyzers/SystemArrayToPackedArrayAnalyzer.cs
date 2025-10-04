using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Godot.UpgradeAssistant.Providers;

[RequiresGodotDotNet]
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SystemArrayToPackedArrayAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule =>
        Descriptors.GUA1011_SystemArrayToPackedArray;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Rule];

    public override void Initialize(DiagnosticAnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclarationNode, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameterNode, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(AnalyzeArgumentNode, SyntaxKind.Argument);
        context.RegisterSyntaxNodeAction(AnalyzeInvocationNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeMethodDeclarationNode(SyntaxNodeAnalysisContext context)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is not IMethodSymbol methodSymbol)
        {
            // Unable to get the method symbol.
            return;
        }

        if (methodSymbol.ReturnsVoid)
        {
            // We only care about methods with return types.
            return;
        }

        if (!methodSymbol.IsOverride)
        {
            // We only care about virtual method overrides.
            return;
        }

        var originalMethod = methodSymbol.GetOverrideOriginalSymbol();
        if (!MethodIsGodotSharpApi(originalMethod))
        {
            return;
        }

        if (!TypeIsChangedToPackedArray(methodSymbol.ReturnType))
        {
            return;
        }

        var properties = new Dictionary<string, string?>()
        {
            [nameof(SyntaxKind)] = nameof(MethodDeclarationSyntax),
        };

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Rule,
            location: methodDeclarationSyntax.GetLocation(),
            properties: properties.ToImmutableDictionary(),
            // Message Format parameters.
            "return",
            methodSymbol.ToDisplayString()));
    }

    private void AnalyzeParameterNode(SyntaxNodeAnalysisContext context)
    {
        var parameterSyntax = (ParameterSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetDeclaredSymbol(parameterSyntax) is not IParameterSymbol parameterSymbol)
        {
            // Unable to get the parameter symbol.
            return;
        }

        if (parameterSymbol.ContainingSymbol is not IMethodSymbol methodSymbol)
        {
            // Unable to get the containing method symbol.
            return;
        }

        if (!methodSymbol.IsOverride)
        {
            // We only care about virtual method overrides.
            return;
        }

        var originalMethod = methodSymbol.GetOverrideOriginalSymbol();
        if (!MethodIsGodotSharpApi(originalMethod))
        {
            return;
        }

        if (!ParameterTypeIsChangedToPackedArray(parameterSymbol))
        {
            return;
        }

        var properties = new Dictionary<string, string?>()
        {
            [nameof(SyntaxKind)] = nameof(ParameterSyntax),
        };

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Rule,
            location: parameterSyntax.GetLocation(),
            properties: properties.ToImmutableDictionary(),
            // Message Format parameters.
            parameterSymbol.Name,
            methodSymbol.ToDisplayString()));
    }

    private void AnalyzeArgumentNode(SyntaxNodeAnalysisContext context)
    {
        var argumentSyntax = (ArgumentSyntax)context.Node;

        InvocationExpressionSyntax? invocationExpression = argumentSyntax
            .Ancestors()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault();

        if (invocationExpression is null)
        {
            // Unable to get the method invocation that contains the argument.
            return;
        }

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetOperation(argumentSyntax) is not IArgumentOperation argumentOperation)
        {
            // Unable to get the argument operation.
            return;
        }

        var parameterSymbol = argumentOperation.Parameter;
        if (!ParameterTypeIsChangedToPackedArray(parameterSymbol))
        {
            return;
        }

        if (argumentSyntax.Expression is CollectionExpressionSyntax)
        {
            // We don't need to transform collection expressions,
            // it should be compatible with packed arrays.
            return;
        }

        if (argumentSyntax.Expression is InvocationExpressionSyntax argumentInvocationExpression)
        {
            if (semanticModel.GetSymbolInfo(argumentInvocationExpression).Symbol is IMethodSymbol argumentMethodSymbol
             && argumentMethodSymbol.DeclaredInGodotSharp())
            {
                // The argument expression invocates a method that is a Godot API,
                // so we can assume the returned type will be a packed array and
                // this argument doesn't need to be transformed.
                return;
            }
        }

        if (argumentSyntax.Expression is IdentifierNameSyntax argumentNameSyntax)
        {
            if (semanticModel.GetSymbolInfo(argumentNameSyntax).Symbol is IParameterSymbol argumentParameterSymbol
             && argumentParameterSymbol.ContainingSymbol is IMethodSymbol containingMethodSymbol
             && MethodIsGodotSharpApi(containingMethodSymbol))
            {
                // The argument expression references a parameter that is contained
                // in a Godot API, so we can assume the parameter's type will be a
                // packed array and this argument doesn't need to be transformed.
                return;
            }
        }

        var properties = new Dictionary<string, string?>()
        {
            [nameof(SyntaxKind)] = nameof(ArgumentSyntax),
        };

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Rule,
            location: argumentSyntax.GetLocation(),
            properties: properties.ToImmutableDictionary(),
            // Message Format parameters.
            parameterSymbol.Name,
            parameterSymbol.ContainingSymbol.ToDisplayString()));
    }

    private void AnalyzeInvocationNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;

        var semanticModel = context.SemanticModel;
        if (semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is not IMethodSymbol methodSymbol)
        {
            // Unable to get the method symbol.
            return;
        }

        if (!MethodIsGodotSharpApi(methodSymbol))
        {
            return;
        }

        if (!TypeIsChangedToPackedArray(methodSymbol.ReturnType))
        {
            return;
        }

        if (invocationExpressionSyntax.Parent is ArgumentSyntax argumentSyntax)
        {
            if (semanticModel.GetOperation(argumentSyntax) is not IArgumentOperation argumentOperation)
            {
                // Unable to get the argument operation.
                return;
            }

            var parameterSymbol = argumentOperation.Parameter;
            if (ParameterTypeIsChangedToPackedArray(parameterSymbol))
            {
                // The invocation expression is an argument of another Godot API,
                // so we can assume the parameter type will match the return type
                // of the invoked method.
                return;
            }
        }

        if (invocationExpressionSyntax.Parent is SpreadElementSyntax)
        {
            // We don't need to transform spreads in collection expressions,
            // it should be compatible with any target type.
            return;
        }

        if (invocationExpressionSyntax.Parent is ReturnStatementSyntax)
        {
            if (IsContainedInGodotSharpMethod(semanticModel, invocationExpressionSyntax))
            {
                // If the invocation expression is part of a return statement,
                // and it's contained in the method body of a GodotSharp API,
                // then we can assume that the return type is changed to packed
                // array and won't need to make any changes to the expression.
                return;
            }

            static bool IsContainedInGodotSharpMethod(SemanticModel semanticModel, InvocationExpressionSyntax invocationExpressionSyntax)
            {
                var containingMethodDeclaration = invocationExpressionSyntax
                    .Ancestors()
                    .OfType<MethodDeclarationSyntax>()
                    .First();

                if (containingMethodDeclaration is null)
                {
                    return false;
                }

                if (semanticModel.GetDeclaredSymbol(containingMethodDeclaration) is not IMethodSymbol containingMethodSymbol)
                {
                    return false;
                }

                return MethodIsGodotSharpApi(containingMethodSymbol);
            }
        }

        if (invocationExpressionSyntax.Parent is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == "ToArray" &&
            memberAccess.Parent is InvocationExpressionSyntax)
        {
            // If the invocation expression is already wrapped in a ".ToArray()" call,
            // we don't need to fix it. It should already be compatible with packed arrays.
            return;
        }

        var properties = new Dictionary<string, string?>()
        {
            [nameof(SyntaxKind)] = nameof(InvocationExpressionSyntax),
        };

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor: Rule,
            location: invocationExpressionSyntax.GetLocation(),
            properties: properties.ToImmutableDictionary(),
            // Message Format parameters.
            "return",
            methodSymbol.ToDisplayString()));
    }

    private static bool TypeIsChangedToPackedArray(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        // Only parameters and returns that used to be C# arrays or ReadOnlySpan<T> are changed to packed arrays.
        return typeSymbol is IArrayTypeSymbol
            || typeSymbol.EqualsGenericType("System.ReadOnlySpan<T>");
    }

    private static bool ParameterTypeIsChangedToPackedArray([NotNullWhen(true)] IParameterSymbol? parameterSymbol)
    {
        if (parameterSymbol is null)
        {
            // Unable to get the parameter symbol.
            return false;
        }

        if (parameterSymbol.IsParams)
        {
            // Parameters with 'params' don't change the type to packed array.
            return false;
        }

        if (!TypeIsChangedToPackedArray(parameterSymbol.Type))
        {
            return false;
        }

        if (parameterSymbol.ContainingSymbol is not IMethodSymbol methodSymbol)
        {
            // Unable to get the method symbol.
            return false;
        }

        if (!MethodIsGodotSharpApi(methodSymbol))
        {
            return false;
        }

        return true;
    }

    private static bool MethodIsGodotSharpApi(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsOverride)
        {
            return methodSymbol.OverridesFromGodotSharp();
        }
        else
        {
            return methodSymbol.DeclaredInGodotSharp();
        }
    }
}

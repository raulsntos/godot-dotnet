using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.UpgradeAssistant.Providers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class ApiMapAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor NotImplementedRule =>
        Descriptors.GUA1002_NotImplementedSymbol;

    private static DiagnosticDescriptor RemovedRule =>
        Descriptors.GUA1003_RemovedSymbol;

    private static DiagnosticDescriptor ReplacedRule =>
        Descriptors.GUA1004_ReplacedSymbol;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            NotImplementedRule,
            RemovedRule,
            ReplacedRule,
        ];

    public override void Initialize(DiagnosticAnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // IdentifierName syntax nodes for types, namespaces, and using directives.
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IdentifierName);

        // GenericName syntax nodes for types and namespaces.
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.GenericName);

        // MemberAccess syntax nodes for properties, fields, methods, and events.
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PointerMemberAccessExpression);

        // MethodDeclaration syntax nodes for virtual method overrides.
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);

        // Attribute syntax nodes for attribute types.
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var syntaxNode = context.Node;
        if (context.Node is AttributeSyntax attributeSyntax)
        {
            // For attributes we want the name expression syntax.
            syntaxNode = attributeSyntax.Name;
        }

        Debug.Assert(syntaxNode is not null);

        if (syntaxNode is IdentifierNameSyntax { IsVar: true })
        {
            // The identifier is the 'var' keyword, ignore.
            return;
        }

        var semanticModel = context.SemanticModel;

        var symbol = syntaxNode switch
        {
            ExpressionSyntax =>
                semanticModel.GetSymbolInfo(syntaxNode, context.CancellationToken).Symbol,
            MethodDeclarationSyntax =>
                semanticModel.GetDeclaredSymbol(syntaxNode, context.CancellationToken),
            _ => null,
        };
        if (symbol is null)
        {
            // Could not determine the symbol, ignore.
            return;
        }

        if (symbol is IMethodSymbol { IsOverride: true } methodSymbol)
        {
            // For method overrides we want the original virtual method that is being overridden.
            symbol = methodSymbol.GetOverrideOriginalSymbol();
        }

        var targetGodotVersion = context.Options.GetTargetGodotVersion();
        bool isGodotDotNetEnabled = context.Options.IsGodotDotNetEnabled();

        var properties = new Dictionary<string, string?>
        {
            [PropertyNames.TargetGodotVersion] = targetGodotVersion.ToString(),
            [PropertyNames.IsGodotDotNetEnabled] = isGodotDotNetEnabled ? "true" : "false",
        };

        var mapping = ApiMapUtils.GetApiEntryForSymbolAsync(symbol, targetGodotVersion, isGodotDotNetEnabled, context.CancellationToken).AsTask().Result;
        if (mapping is null)
        {
            // Mapping not found.
            return;
        }

        // Check that the syntax node found matches what the mapping kind expects.
        // This is important to avoid reporting multiple diagnostics in syntax like member access
        // that also contains identifier name, and could report multiple diagnostics for the same
        // mapping.
        switch (mapping.Kind)
        {
            case ApiMapKind.Field:
            case ApiMapKind.Property:
            case ApiMapKind.Method:
            case ApiMapKind.Event:
            {
                if (syntaxNode is not MemberAccessExpressionSyntax and not IdentifierNameSyntax and not MethodDeclarationSyntax)
                {
                    // Syntax node must be member access, identifier name, or method declaration.
                    return;
                }
                if (syntaxNode is IdentifierNameSyntax { Parent: MemberAccessExpressionSyntax })
                {
                    // If the syntax node is an identifier name that is inside a member access,
                    // ignore it so we don't analyze the same syntax multiple times.
                    return;
                }
                if (syntaxNode is MethodDeclarationSyntax && !symbol.IsVirtual)
                {
                    // If the syntax node is a method declaration, the symbol must be virtual.
                    return;
                }
                break;
            }

            case ApiMapKind.Namespace:
            case ApiMapKind.Type:
            {
                if (syntaxNode is not IdentifierNameSyntax and not GenericNameSyntax)
                {
                    // Syntax node must be identifier name or generic name.
                    return;
                }
                break;
            }
        }

        switch (mapping.State)
        {
            case ApiMapState.NotImplemented:
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor: NotImplementedRule,
                    location: context.Node.GetLocation(),
                    properties: properties.ToImmutableDictionary(),
                    // Message Format parameters.
                    symbol.ToDisplayString()));
                break;
            }

            case ApiMapState.Removed:
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor: RemovedRule,
                    location: context.Node.GetLocation(),
                    properties: properties.ToImmutableDictionary(),
                    // Message Format parameters.
                    symbol.ToDisplayString()));
                break;
            }

            case ApiMapState.Replaced:
            {
                if (string.IsNullOrWhiteSpace(mapping.Value))
                {
                    throw new InvalidOperationException(SR.FormatInvalidOperation_ApiMapEntryValueIsInvalid(mapping.Key));
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor: ReplacedRule,
                    location: context.Node.GetLocation(),
                    properties: properties.ToImmutableDictionary(),
                    // Message Format parameters.
                    symbol.ToDisplayString(),
                    mapping.Value));
                break;
            }

            default:
            {
                throw new InvalidOperationException(SR.FormatInvalidOperation_ApiMapEntryStateIsInvalid(mapping.State));
            }
        }
    }
}

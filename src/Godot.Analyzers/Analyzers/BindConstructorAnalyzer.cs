using System;
using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BindConstructorAnalyzer : DiagnosticAnalyzer
{
    [Flags]
    private enum BuilderMethodRequirements
    {
        None = 0,
        Static = 1 << 0,
        NonGeneric = 1 << 1,
        Parameterless = 1 << 2,
        Accessible = 1 << 3,
        CompatibleReturnType = 1 << 4,
        All = Static | NonGeneric | Parameterless | Accessible | CompatibleReturnType,
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0201_BuilderMethodNotFound,
        Descriptors.GODOT0201_BuilderMethodInaccessible,
        Descriptors.GODOT0201_BuilderMethodMustBeStatic,
        Descriptors.GODOT0201_BuilderMethodMustHaveNoGenericTypeParameters,
        Descriptors.GODOT0201_BuilderMethodMustHaveNoParameters,
        Descriptors.GODOT0201_BuilderMethodMustReturnCompatibleType,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (!context.Symbol.HasAttribute(KnownTypeNames.GodotClassAttribute))
        {
            // [BindConstructor] is only relevant on types also annotated with [GodotClass].
            return;
        }

        INamedTypeSymbol classSymbol = (INamedTypeSymbol)context.Symbol;

        if (!classSymbol.TryGetAttribute(KnownTypeNames.BindConstructorAttribute, out var attribute))
        {
            return;
        }

        INamedTypeSymbol? builderTypeSymbol;
        string? methodName;

        var ctorArgs = attribute.ConstructorArguments;

        switch (ctorArgs.Length)
        {
            case 2:
                builderTypeSymbol = ctorArgs[0].Value as INamedTypeSymbol;
                methodName = ctorArgs[1].Value as string;
                break;

            default:
                throw new InvalidOperationException($"BindConstructor attribute constructor has {ctorArgs.Length}, expected 2.");
        }

        if (builderTypeSymbol is null || string.IsNullOrEmpty(methodName))
        {
            // Attribute constructor requires these to be specified, so this should be unreachable.
            return;
        }

        string methodFullName = $"{builderTypeSymbol.ToDisplayString()}.{methodName}";
        var location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation();

        if (builderTypeSymbol.IsUnboundGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodMustHaveNoGenericTypeParameters,
                location,
                // Message Format parameters.
                methodFullName
            ));

            return;
        }

        // Find all the methods on the builder type with the specified name (there may be overloads).
        var methodSymbols = builderTypeSymbol.GetMembers(methodName!)
            .OfType<IMethodSymbol>()
            .ToImmutableArray();

        if (methodSymbols.Length == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodNotFound,
                location,
                // Message Format parameters.
                methodFullName
            ));

            return;
        }

        BuilderMethodRequirements allRequirements = BuilderMethodRequirements.None;

        foreach (var candidateMethodSymbol in methodSymbols)
        {
            BuilderMethodRequirements methodRequirements = BuilderMethodRequirements.None;

            if (candidateMethodSymbol.IsStatic)
            {
                methodRequirements |= BuilderMethodRequirements.Static;
            }

            if (!candidateMethodSymbol.IsGenericMethod)
            {
                methodRequirements |= BuilderMethodRequirements.NonGeneric;
            }

            if (candidateMethodSymbol.Parameters.Length == 0)
            {
                methodRequirements |= BuilderMethodRequirements.Parameterless;
            }

            if (context.Compilation.IsSymbolAccessibleWithin(candidateMethodSymbol, classSymbol))
            {
                methodRequirements |= BuilderMethodRequirements.Accessible;
            }

            if (IsReturnTypeValid(context.Compilation, candidateMethodSymbol.ReturnType, classSymbol))
            {
                methodRequirements |= BuilderMethodRequirements.CompatibleReturnType;
            }

            if (methodRequirements == BuilderMethodRequirements.All)
            {
                // Found an overload that satisfies all requirements.
                return;
            }

            allRequirements |= methodRequirements;
        }

        if (methodSymbols.Length > 1)
        {
            // We could not find an overload that satisfies all requirements.
            // But, since there are multiple overloads, we don't know which of
            // the overloads the user intended to use.
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodNotFound,
                location,
                // Message Format parameters.
                methodFullName
            ));

            return;
        }

        if (!allRequirements.HasFlag(BuilderMethodRequirements.Accessible))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodInaccessible,
                location,
                // Message Format parameters.
                methodFullName,
                classSymbol.ToDisplayString()
            ));
        }

        if (!allRequirements.HasFlag(BuilderMethodRequirements.Static))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodMustBeStatic,
                location,
                // Message Format parameters.
                methodFullName
            ));
        }

        if (!allRequirements.HasFlag(BuilderMethodRequirements.NonGeneric))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodMustHaveNoGenericTypeParameters,
                location,
                // Message Format parameters.
                methodFullName
            ));
        }

        if (!allRequirements.HasFlag(BuilderMethodRequirements.Parameterless))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodMustHaveNoParameters,
                location,
                // Message Format parameters.
                methodFullName
            ));
        }

        if (!allRequirements.HasFlag(BuilderMethodRequirements.CompatibleReturnType))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.GODOT0201_BuilderMethodMustReturnCompatibleType,
                location,
                // Message Format parameters.
                methodFullName,
                classSymbol.ToDisplayString()
            ));
        }
    }

    private static bool IsReturnTypeValid(Compilation compilation, ITypeSymbol? returnTypeSymbol, ITypeSymbol targetTypeSymbol)
    {
        if (returnTypeSymbol is null)
        {
            // Method must not return void.
            return false;
        }

        var conversion = compilation.ClassifyConversion(returnTypeSymbol, targetTypeSymbol);
        return conversion.IsImplicit;
    }
}

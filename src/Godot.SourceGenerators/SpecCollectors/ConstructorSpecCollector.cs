using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Godot.SourceGenerators;

internal static class ConstructorSpecCollector
{
    public static GodotConstructorSpec? Collect(Compilation compilation, INamedTypeSymbol classSymbol, CancellationToken cancellationToken = default)
    {
        if (!classSymbol.TryGetAttribute(KnownTypeNames.BindConstructorAttribute, out var attribute))
        {
            // Classes must have the attribute to register a custom constructor.
            return null;
        }

        INamedTypeSymbol? builderType;
        string? methodName;

        var ctorArgs = attribute.ConstructorArguments;

        switch (ctorArgs.Length)
        {
            case 2:
                builderType = ctorArgs[0].Value as INamedTypeSymbol;
                methodName = ctorArgs[1].Value as string;
                break;

            default:
                throw new InvalidOperationException($"BindConstructor attribute constructor has {ctorArgs.Length}, expected 2.");
        }

        if (builderType is null || string.IsNullOrEmpty(methodName))
        {
            // Attribute constructor requires these to be specified, so this should be unreachable.
            return null;
        }

        if (!TryGetBuilderMethod(compilation, classSymbol, builderType, methodName!, out var methodSymbol))
        {
            // Could not find a suitable method on the builder type.
            return null;
        }

        return new GodotConstructorSpec()
        {
            FullyQualifiedBuilderTypeName = builderType.FullQualifiedNameWithGlobal(),
            MethodSymbolName = methodSymbol.Name,
        };
    }

    private static bool TryGetBuilderMethod(Compilation compilation, INamedTypeSymbol targetTypeSymbol, INamedTypeSymbol builderType, string methodName, [NotNullWhen(true)] out IMethodSymbol? methodSymbol)
    {
        if (builderType.IsUnboundGenericType)
        {
            // Builder type must not be an unbound generic type.
            methodSymbol = null;
            return false;
        }

        // Find all the methods on the builder type with the specified name (there may be overloads).
        var methodSymbols = builderType.GetMembers(methodName);

        foreach (var symbol in methodSymbols)
        {
            if (symbol is not IMethodSymbol { IsStatic: true } candidateMethodSymbol)
            {
                continue;
            }

            if (candidateMethodSymbol.IsGenericMethod)
            {
                continue;
            }

            if (candidateMethodSymbol.Parameters.Length != 0)
            {
                continue;
            }

            if (!compilation.IsSymbolAccessibleWithin(candidateMethodSymbol, targetTypeSymbol))
            {
                continue;
            }

            if (!IsReturnTypeValid(compilation, targetTypeSymbol, candidateMethodSymbol.ReturnType))
            {
                continue;
            }

            methodSymbol = candidateMethodSymbol;
            return true;
        }

        methodSymbol = null;
        return false;
    }

    private static bool IsReturnTypeValid(Compilation compilation, ITypeSymbol targetTypeSymbol, ITypeSymbol? returnTypeSymbol)
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

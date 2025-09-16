using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class MethodSpecCollector
{
    public static GodotMethodSpec? Collect(Compilation compilation, IMethodSymbol methodSymbol, CancellationToken cancellationToken = default)
    {
        if (!methodSymbol.TryGetAttribute(KnownTypeNames.BindMethodAttribute, out var attribute))
        {
            // Methods must have the attribute to be registered.
            return null;
        }

        List<GodotPropertySpec> parameters = [];
        GodotPropertySpec? returnParameter = null;
        string? nameOverride = null;
        bool? isVirtual = null;

        foreach (var (key, constant) in attribute.NamedArguments)
        {
            switch (key)
            {
                case "Name":
                    nameOverride = constant.Value as string;
                    break;

                case "Virtual":
                    isVirtual = constant.Value as bool?;
                    break;
            }
        }

        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            GodotPropertySpec? parameterSpec = PropertySpecCollector.Collect(compilation, parameterSymbol, cancellationToken);
            if (parameterSpec is null)
            {
                return null;
            }
            parameters.Add(parameterSpec.Value);
        }

        if (!methodSymbol.ReturnsVoid)
        {
            returnParameter = PropertySpecCollector.Collect(compilation, methodSymbol.ReturnType, cancellationToken);
            if (returnParameter is null)
            {
                return null;
            }
        }

        // If this is an explicit interface implementation, avoid including the interface name in the symbol name.
        string symbolName = methodSymbol.Name;
        string? explicitInterfaceSymbolName = null;
        if (methodSymbol.ExplicitInterfaceImplementations.Length > 0)
        {
            symbolName = methodSymbol.ExplicitInterfaceImplementations[0].Name;
            explicitInterfaceSymbolName = methodSymbol.ExplicitInterfaceImplementations[0].ContainingType.FullQualifiedNameWithGlobal();
        }

        return new GodotMethodSpec()
        {
            SymbolName = symbolName,
            IsStatic = methodSymbol.IsStatic,
            IsVirtual = isVirtual ?? false,
            Parameters = [.. parameters],
            ReturnParameter = returnParameter,
            NameOverride = nameOverride,
            ExplicitInterfaceFullyQualifiedTypeName = explicitInterfaceSymbolName,
        };
    }
}

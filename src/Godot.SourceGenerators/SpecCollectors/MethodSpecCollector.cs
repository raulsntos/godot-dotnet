using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
            GodotPropertySpec parameterSpec = PropertySpecCollector.Collect(compilation, parameterSymbol, cancellationToken);
            parameters.Add(parameterSpec);
        }

        if (!methodSymbol.ReturnsVoid)
        {
            returnParameter = PropertySpecCollector.Collect(compilation, methodSymbol.ReturnType, cancellationToken);
            Debug.Assert(returnParameter is not null);
        }

        return new GodotMethodSpec()
        {
            SymbolName = methodSymbol.Name,
            IsStatic = methodSymbol.IsStatic,
            IsVirtual = isVirtual ?? false,
            Parameters = [.. parameters],
            ReturnParameter = returnParameter,
            NameOverride = nameOverride,
        };
    }
}

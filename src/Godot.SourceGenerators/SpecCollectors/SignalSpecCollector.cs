using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class SignalSpecCollector
{
    public static GodotSignalSpec? Collect(Compilation compilation, INamedTypeSymbol delegateTypeSymbol, CancellationToken cancellationToken = default)
    {
        if (delegateTypeSymbol.TypeKind != TypeKind.Delegate)
        {
            // Type symbol must be a delegate.
            return null;
        }

        if (!delegateTypeSymbol.TryGetAttribute(KnownTypeNames.SignalAttribute, out var attribute))
        {
            // Signals must have the attribute to be registered.
            return null;
        }

        List<GodotPropertySpec> parameters = [];
        string? nameOverride = null;

        foreach (var (key, constant) in attribute.NamedArguments)
        {
            switch (key)
            {
                case "Name":
                    nameOverride = constant.Value as string;
                    break;
            }
        }

        var delegateInvokeMethod = delegateTypeSymbol.DelegateInvokeMethod;
        if (delegateInvokeMethod is null)
        {
            return null;
        }

        foreach (var parameterSymbol in delegateInvokeMethod.Parameters)
        {
            GodotPropertySpec parameterSpec = PropertySpecCollector.Collect(compilation, parameterSymbol, cancellationToken);
            parameters.Add(parameterSpec);
        }

        return new GodotSignalSpec()
        {
            SymbolName = delegateTypeSymbol.Name,
            Parameters = [.. parameters],
            NameOverride = nameOverride,
        };
    }
}

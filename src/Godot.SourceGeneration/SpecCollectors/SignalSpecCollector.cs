using System;
using System.Collections.Generic;
using System.Threading;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGeneration;

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

        if (string.IsNullOrEmpty(nameOverride) && !delegateTypeSymbol.Name.EndsWith("EventHandler", StringComparison.OrdinalIgnoreCase))
        {
            // Signal delegates must end with 'EventHandler' suffix.
            return null;
        }

        foreach (var parameterSymbol in delegateInvokeMethod.Parameters)
        {
            GodotPropertySpec? parameterSpec = PropertySpecCollector.Collect(compilation, parameterSymbol, cancellationToken);
            if (parameterSpec is null)
            {
                return null;
            }
            parameters.Add(parameterSpec.Value);
        }

        return new GodotSignalSpec()
        {
            SymbolName = delegateTypeSymbol.Name,
            Parameters = [.. parameters],
            NameOverride = nameOverride,
        };
    }
}

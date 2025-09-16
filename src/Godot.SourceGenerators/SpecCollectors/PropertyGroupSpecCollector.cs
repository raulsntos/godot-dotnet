using System;
using System.Threading;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class PropertyGroupSpecCollector
{
    public static GodotPropertyGroupSpec? Collect(Compilation compilation, IPropertySymbol propertySymbol, CancellationToken cancellationToken = default)
    {
        if (!propertySymbol.TryGetAttribute(KnownTypeNames.PropertyGroupAttribute, out var attribute))
        {
            // Properties must have the attribute to define a group.
            return null;
        }

        return CollectCore(compilation, propertySymbol.Name, propertySymbol.Type, attribute, cancellationToken);
    }

    public static GodotPropertyGroupSpec? Collect(Compilation compilation, IFieldSymbol fieldSymbol, CancellationToken cancellationToken = default)
    {
        if (!fieldSymbol.TryGetAttribute(KnownTypeNames.PropertyGroupAttribute, out var attribute))
        {
            // Fields must have the attribute to define a group.
            return null;
        }

        return CollectCore(compilation, fieldSymbol.Name, fieldSymbol.Type, attribute, cancellationToken);
    }

    private static GodotPropertyGroupSpec CollectCore(Compilation compilation, string symbolName, ITypeSymbol typeSymbol, AttributeData attribute, CancellationToken cancellationToken = default)
    {
        string? name;
        string? prefix = null;

        var ctorArgs = attribute.ConstructorArguments;

        switch (ctorArgs.Length)
        {
            case 1:
                name = ctorArgs[0].Value as string;
                break;

            case 2:
                prefix = ctorArgs[1].Value as string;
                goto case 1;

            default:
                throw new InvalidOperationException($"Group attribute constructor has {ctorArgs.Length}, expected 1 or 2.");
        }

        return new GodotPropertyGroupSpec()
        {
            Name = name!,
            Prefix = prefix,
        };
    }
}

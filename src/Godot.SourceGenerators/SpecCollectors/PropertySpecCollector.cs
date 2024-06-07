using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.SourceGenerators;

internal static class PropertySpecCollector
{
    public static GodotPropertySpec? Collect(Compilation compilation, IPropertySymbol propertySymbol, CancellationToken cancellationToken = default)
    {
        if (!propertySymbol.TryGetAttribute(KnownTypeNames.BindPropertyAttribute, out var attribute))
        {
            // Properties must have the attribute to be registered.
            return null;
        }

        return CollectCore(compilation, propertySymbol.Name, propertySymbol.Type, attribute, cancellationToken) with
        {
            GroupDefinition = PropertyGroupSpecCollector.Collect(compilation, propertySymbol, cancellationToken),
            SubgroupDefinition = PropertySubgroupSpecCollector.Collect(compilation, propertySymbol, cancellationToken),
        };
    }

    public static GodotPropertySpec? Collect(Compilation compilation, IFieldSymbol fieldSymbol, CancellationToken cancellationToken = default)
    {
        if (!fieldSymbol.TryGetAttribute(KnownTypeNames.BindPropertyAttribute, out var attribute))
        {
            // Fields must have the attribute to be registered.
            return null;
        }

        return CollectCore(compilation, fieldSymbol.Name, fieldSymbol.Type, attribute, cancellationToken) with
        {
            GroupDefinition = PropertyGroupSpecCollector.Collect(compilation, fieldSymbol, cancellationToken),
            SubgroupDefinition = PropertySubgroupSpecCollector.Collect(compilation, fieldSymbol, cancellationToken),
        };
    }

    public static GodotPropertySpec Collect(Compilation compilation, IParameterSymbol parameterSymbol, CancellationToken cancellationToken = default)
    {
        parameterSymbol.TryGetAttribute(KnownTypeNames.BindPropertyAttribute, out var attribute);

        return CollectCore(compilation, parameterSymbol.Name, parameterSymbol.Type, attribute, cancellationToken) with
        {
            HasExplicitDefaultValue = parameterSymbol.HasExplicitDefaultValue,
            ExplicitDefaultValue = GetExplicitDefaultValueExpression(parameterSymbol, cancellationToken),
        };

        static string? GetExplicitDefaultValueExpression(IParameterSymbol parameterSymbol, CancellationToken cancellationToken = default)
        {
            if (!parameterSymbol.HasExplicitDefaultValue)
            {
                return null;
            }

            if (parameterSymbol.ExplicitDefaultValue is null)
            {
                return "default";
            }

            foreach (var syntaxReference in parameterSymbol.DeclaringSyntaxReferences)
            {
                var parameterSyntax = (ParameterSyntax)syntaxReference.GetSyntax(cancellationToken);
                if (parameterSyntax.Default is not null)
                {
                    var sb = new StringBuilder();
                    if (parameterSymbol.Type.TypeKind == TypeKind.Enum)
                    {
                        return $"(long)({parameterSyntax.Default.Value})";
                    }
                    return parameterSyntax.Default.Value.ToString();
                }
            }

            throw new InvalidOperationException($"Parameter '{parameterSymbol}' has a default value but the syntax node could not be found.");
        }
    }

    public static GodotPropertySpec Collect(Compilation compilation, ITypeSymbol returnTypeSymbol, CancellationToken cancellationToken = default)
    {
        returnTypeSymbol.TryGetAttribute(KnownTypeNames.BindPropertyAttribute, out var attribute);

        // Return parameters don't have names.
        return CollectCore(compilation, "", returnTypeSymbol, attribute, cancellationToken);
    }

    private static GodotPropertySpec CollectCore(Compilation compilation, string symbolName, ITypeSymbol typeSymbol, AttributeData? attribute, CancellationToken cancellationToken = default)
    {
        string? nameOverride = null;

        if (attribute is not null)
        {
            foreach (var (key, constant) in attribute.NamedArguments)
            {
                switch (key)
                {
                    case "Name":
                        nameOverride = constant.Value as string;
                        break;
                }
            }
        }

        var marshalInfo = Marshalling.GetMarshallingInformation(compilation, typeSymbol);

        return new GodotPropertySpec()
        {
            SymbolName = symbolName,
            FullyQualifiedTypeName = typeSymbol.FullNameWithGlobal(),
            MarshalInfo = marshalInfo,
            NameOverride = nameOverride,
        };
    }
}

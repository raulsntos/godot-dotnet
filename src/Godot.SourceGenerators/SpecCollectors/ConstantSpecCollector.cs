using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class ConstantSpecCollector
{
    public static GodotConstantSpec? Collect(Compilation compilation, IFieldSymbol fieldSymbol, CancellationToken cancellationToken = default)
    {
        if (!fieldSymbol.IsConst)
        {
            // Field must be constant.
            return null;
        }

        if (!fieldSymbol.TryGetAttribute(KnownTypeNames.BindConstantAttribute, out var attribute))
        {
            // Constants must have the attribute to be registered.
            return null;
        }

        // Constant is not contained in an enum.
        return CollectCore(compilation, fieldSymbol.Name, enumSymbolName: "", enumNameOverride: "", isFlagsEnum: false, attribute, cancellationToken);
    }

    public static IEnumerable<GodotConstantSpec> Collect(Compilation compilation, ITypeSymbol enumTypeSymbol, CancellationToken cancellationToken = default)
    {
        if (enumTypeSymbol.TypeKind != TypeKind.Enum)
        {
            // Type symbol must be an enum.
            yield break;
        }

        if (!enumTypeSymbol.TryGetAttribute(KnownTypeNames.BindEnumAttribute, out var enumAttribute))
        {
            // Enums must have the attribute to be registered.
            yield break;
        }

        string? enumNameOverride = null;

        foreach (var (key, constant) in enumAttribute.NamedArguments)
        {
            switch (key)
            {
                case "Name":
                    enumNameOverride = constant.Value as string;
                    break;
            }
        }

        bool isFlagsEnum = enumTypeSymbol.TryGetAttribute(KnownTypeNames.SystemFlagsAttribute, out _);

        foreach (var fieldSymbol in enumTypeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            fieldSymbol.TryGetAttribute(KnownTypeNames.BindConstantAttribute, out var constantAttribute);

            yield return CollectCore(compilation, fieldSymbol.Name, enumTypeSymbol.Name, enumNameOverride, isFlagsEnum, constantAttribute, cancellationToken);
        }
    }

    private static GodotConstantSpec CollectCore(Compilation compilation, string symbolName, string? enumSymbolName, string? enumNameOverride, bool isFlagsEnum, AttributeData? attribute, CancellationToken cancellationToken = default)
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

        return new GodotConstantSpec()
        {
            SymbolName = symbolName,
            EnumSymbolName = enumSymbolName,
            EnumNameOverride = enumNameOverride,
            IsFlagsEnum = isFlagsEnum,
            NameOverride = nameOverride,
        };
    }
}

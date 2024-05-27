using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class RegistrationSpecCollector
{
    public static GodotRegistrationSpec? Collect(Compilation compilation, INamedTypeSymbol typeSymbol, CancellationToken cancellationToken = default)
    {
        if (!typeSymbol.TryGetAttribute(KnownTypeNames.GodotClassAttribute, out var attribute))
        {
            // Classes must have the attribute to be registered.
            return null;
        }

        bool? isTool = null;

        foreach (var (key, constant) in attribute.NamedArguments)
        {
            switch (key)
            {
                case "Tool":
                    isTool = constant.Value as bool?;
                    break;
            }
        }

        var registrationKind = GodotRegistrationSpec.Kind.RuntimeClass;
        if (isTool ?? false)
        {
            registrationKind = GodotRegistrationSpec.Kind.Class;
        }

        return new GodotRegistrationSpec()
        {
            SymbolName = typeSymbol.Name,
            FullyQualifiedSymbolName = typeSymbol.FullNameWithGlobal(),
            RegistrationKind = registrationKind,
        };
    }
}

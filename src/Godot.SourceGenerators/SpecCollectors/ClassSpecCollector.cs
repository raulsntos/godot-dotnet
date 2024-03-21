using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class ClassSpecCollector
{
    public static GodotClassSpec Collect(Compilation compilation, INamedTypeSymbol typeSymbol, CancellationToken cancellationToken = default)
    {
        var members = typeSymbol.GetMembers();

        List<ContainingSymbol> containingTypeSymbols = [];
        GodotConstructorSpec? constructor = null;
        List<GodotPropertySpec> properties = [];
        List<GodotMethodSpec> methods = [];
        List<GodotSignalSpec> signals = [];

        // Initialize constructor spec if the class is instantiable.
        if (!typeSymbol.IsAbstract)
        {
            constructor = new GodotConstructorSpec();
        }

        // Collect containing symbols.
        var containingType = typeSymbol.ContainingType;
        while (containingType is not null)
        {
            var containingSymbol = new ContainingSymbol()
            {
                SymbolKind = containingType.TypeKind switch
                {
                    TypeKind.Interface => ContainingSymbol.Kind.Interface,
                    TypeKind.Class when !containingType.IsRecord => ContainingSymbol.Kind.Class,
                    TypeKind.Class when containingType.IsRecord => ContainingSymbol.Kind.RecordClass,
                    TypeKind.Struct when !containingType.IsRecord => ContainingSymbol.Kind.Struct,
                    TypeKind.Struct when containingType.IsRecord => ContainingSymbol.Kind.RecordStruct,
                    _ => ContainingSymbol.Kind.Unknown,
                },
                SymbolName = containingType.Name,
            };
            containingTypeSymbols.Add(containingSymbol);

            if (containingSymbol.SymbolKind == ContainingSymbol.Kind.Unknown)
            {
                throw new InvalidOperationException($"Could not determine kind of symbol for '{containingType}'.");
            }

            containingType = containingType.ContainingType;
        }

        // Collect method and constructor specs.
        foreach (var methodSymbol in members.OfType<IMethodSymbol>())
        {
            GodotMethodSpec? methodSpec = MethodSpecCollector.Collect(compilation, methodSymbol, cancellationToken);
            if (methodSpec is not null)
            {
                methods.Add(methodSpec.Value);
            }

            GodotConstructorSpec? ctorSpec = ConstructorSpecCollector.Collect(compilation, methodSymbol, cancellationToken);
            if (ctorSpec is not null)
            {
                constructor = ctorSpec;
            }
        }

        // Collect property specs.
        foreach (var propertySymbol in members.OfType<IPropertySymbol>())
        {
            GodotPropertySpec? propertySpec = PropertySpecCollector.Collect(compilation, propertySymbol, cancellationToken);
            if (propertySpec is not null)
            {
                properties.Add(propertySpec.Value);
            }
        }

        // Collect signal specs.
        foreach (var nestedTypeSymbol in members.OfType<INamedTypeSymbol>())
        {
            GodotSignalSpec? signalSpec = SignalSpecCollector.Collect(compilation, nestedTypeSymbol, cancellationToken);
            if (signalSpec is not null)
            {
                signals.Add(signalSpec.Value);
            }
        }

        return new GodotClassSpec()
        {
            SymbolName = typeSymbol.Name,
            FullyQualifiedSymbolName = typeSymbol.FullNameWithGlobal(),
            FullyQualifiedNamespace = typeSymbol.ContainingNamespace?.FullName(),
            ContainingTypeSymbols = [.. containingTypeSymbols],
            FullyQualifiedBaseTypeName = typeSymbol.BaseType?.FullNameWithGlobal() ?? KnownTypeNames.GodotObject,
            Constructor = constructor,
            Properties = [.. properties],
            Methods = [.. methods],
            Signals = [.. signals],
        };
    }
}

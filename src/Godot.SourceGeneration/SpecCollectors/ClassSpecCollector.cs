using System;
using System.Collections.Generic;
using System.Threading;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGeneration;

internal static class ClassSpecCollector
{
    public static GodotClassSpec? Collect(Compilation compilation, INamedTypeSymbol typeSymbol, CancellationToken cancellationToken = default)
    {
        if (!typeSymbol.TryGetAttribute(KnownTypeNames.GodotClassAttribute, out var attribute))
        {
            // Classes must have the attribute to be registered.
            return null;
        }

        if (!typeSymbol.DerivesFrom(KnownTypeNames.GodotObject))
        {
            // Classes must derive from GodotObject to be registered.
            return null;
        }

        if (typeSymbol.IsGenericType)
        {
            // Generic types can't be registered.
            return null;
        }

        var members = typeSymbol.GetMembers();

        List<ContainingSymbol> containingTypeSymbols = [];
        GodotConstructorSpec? constructor = null;
        List<GodotConstantSpec> constants = [];
        List<GodotPropertySpec> properties = [];
        List<GodotMethodSpec> methods = [];
        List<GodotSignalSpec> signals = [];
        List<GodotRpcMethodSpec> rpcMethods = [];
        string? icon = null;

        // Initialize constructor spec if the class is instantiable
        // and has a suitable parameterless constructor.
        if (!typeSymbol.IsAbstract)
        {
            foreach (var ctor in typeSymbol.InstanceConstructors)
            {
                if (ctor.Parameters.Length == 0)
                {
                    constructor = GodotConstructorSpec.CreateForConstructor(typeSymbol);
                    break;
                }
            }
        }

        // Collect custom constructor spec and override the default one if found.
        var customConstructor = ConstructorSpecCollector.Collect(compilation, typeSymbol, cancellationToken);
        if (customConstructor is not null)
        {
            constructor = customConstructor;
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

        // Collect method specs.
        foreach (var methodSymbol in members.OfType<IMethodSymbol>())
        {
            GodotMethodSpec? methodSpec = MethodSpecCollector.Collect(compilation, methodSymbol, cancellationToken);
            if (methodSpec is not null)
            {
                methods.Add(methodSpec.Value);
            }
        }

        // Collect constant specs.
        foreach (var fieldSymbol in members.OfType<IFieldSymbol>())
        {
            GodotConstantSpec? constantSpec = ConstantSpecCollector.Collect(compilation, fieldSymbol, cancellationToken);
            if (constantSpec is not null)
            {
                constants.Add(constantSpec.Value);
            }
        }
        foreach (var nestedTypeSymbol in members.OfType<INamedTypeSymbol>())
        {
            var constantSpecs = ConstantSpecCollector.Collect(compilation, nestedTypeSymbol, cancellationToken);
            foreach (var constantSpec in constantSpecs)
            {
                constants.Add(constantSpec);
            }
        }

        // Collect shared enum constants from [BindSharedEnum] attributes.
        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass?.FullQualifiedNameOmitGlobal() != KnownTypeNames.BindSharedEnumAttribute)
            {
                continue;
            }

            if (attributeData.ConstructorArguments.Length < 1)
            {
                continue;
            }

            if (attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol enumTypeSymbol)
            {
                continue;
            }

            string? enumNameOverride = null;
            foreach (var (key, constant) in attributeData.NamedArguments)
            {
                if (key == "Name")
                {
                    enumNameOverride = constant.Value as string;
                }
            }

            var sharedConstantSpecs = ConstantSpecCollector.CollectShared(compilation, enumTypeSymbol, enumNameOverride, cancellationToken);
            foreach (var constantSpec in sharedConstantSpecs)
            {
                constants.Add(constantSpec);
            }
        }

        // Collect property specs.
        foreach (var symbol in members)
        {
            GodotPropertySpec? propertySpec = symbol switch
            {
                IPropertySymbol propertySymbol =>
                    PropertySpecCollector.Collect(compilation, propertySymbol, cancellationToken),

                IFieldSymbol fieldSymbol =>
                    PropertySpecCollector.Collect(compilation, fieldSymbol, cancellationToken),

                _ => null,
            };
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

        // Collect RPC method specs.
        foreach (var methodSymbol in members.OfType<IMethodSymbol>())
        {
            GodotRpcMethodSpec? rpcMethodSpec = RpcMethodSpecCollector.Collect(compilation, methodSymbol, cancellationToken);
            if (rpcMethodSpec is not null)
            {
                rpcMethods.Add(rpcMethodSpec.Value);
            }
        }

        // Collect icon.
        foreach (var (key, constant) in attribute.NamedArguments)
        {
            switch (key)
            {
                case "Icon":
                    icon = constant.Value as string;
                    break;
            }
        }

        return new GodotClassSpec()
        {
            SymbolName = typeSymbol.Name,
            FullyQualifiedSymbolName = typeSymbol.FullQualifiedNameWithGlobal(),
            FullyQualifiedNamespace = typeSymbol.ContainingNamespace?.FullQualifiedNameOmitGlobal(),
            ContainingTypeSymbols = [.. containingTypeSymbols],
            FullyQualifiedBaseTypeName = typeSymbol.BaseType?.FullQualifiedNameWithGlobal() ?? KnownTypeNames.GodotObject,
            Constructor = constructor,
            Constants = [.. constants],
            Properties = [.. properties],
            Methods = [.. methods],
            Signals = [.. signals],
            RpcMethods = [.. rpcMethods],
            IconPath = icon,
        };
    }
}

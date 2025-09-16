using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BoundMembersMustHaveUniqueNamesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0002_BoundMembersMustHaveUniqueNames,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        if (typeSymbol.TypeKind is not TypeKind.Class)
        {
            return;
        }

        if (!typeSymbol.HasAttribute(KnownTypeNames.GodotClassAttribute))
        {
            // If the class does not have the [GodotClass] attribute, the other attributes don't have any effect
            // so we don't need to check for duplicates.
            return;
        }

        // Collect all registered members in this class and its base types,
        // and group them by the registered name.
        var allMembers = GetRegisteredMembers(typeSymbol);
        var symbolsGroupedByName = new Dictionary<string, List<ISymbol>>();
        foreach (var (memberSymbol, registeredName) in allMembers)
        {
            if (!symbolsGroupedByName.TryGetValue(registeredName, out var symbols))
            {
                symbols = [];
                symbolsGroupedByName[registeredName] = symbols;
            }

            symbols.Add(memberSymbol);
        }

        // For each duplicate, report diagnostic on all but the first.
        foreach (var (name, symbols) in symbolsGroupedByName)
        {
            if (symbols.Count > 1)
            {
                foreach (var symbol in symbols.Skip(1))
                {
                    // Only report on symbols declared in this class.
                    if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, typeSymbol))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.GODOT0002_BoundMembersMustHaveUniqueNames,
                            symbol.Locations[0],
                            // Message Format parameters.
                            symbol.ContainingType.Name,
                            name
                        ));
                    }
                }
            }
        }
    }

    private static IEnumerable<(ISymbol Symbol, string Name)> GetRegisteredMembers(INamedTypeSymbol typeSymbol)
    {
        // Collect all types in the inheritance chain so we can iterate
        // from the most base type to the most derived type.
        Stack<INamedTypeSymbol> typeSymbolStack = [];
        {
            var currentTypeSymbol = typeSymbol;
            while (currentTypeSymbol is not null)
            {
                typeSymbolStack.Push(currentTypeSymbol);
                currentTypeSymbol = currentTypeSymbol.BaseType;
            }
        }

        while (typeSymbolStack.Count > 0)
        {
            var currentTypeSymbol = typeSymbolStack.Pop();
            foreach (var memberSymbol in currentTypeSymbol.GetMembers())
            {
                string? attributeFullName = memberSymbol switch
                {
                    IFieldSymbol =>
                        KnownTypeNames.BindPropertyAttribute,

                    IPropertySymbol =>
                        KnownTypeNames.BindPropertyAttribute,

                    IMethodSymbol =>
                        KnownTypeNames.BindMethodAttribute,

                    INamedTypeSymbol delegateSymbol when delegateSymbol.TypeKind == TypeKind.Delegate =>
                        KnownTypeNames.SignalAttribute,

                    _ =>
                        null,
                };

                if (string.IsNullOrEmpty(attributeFullName))
                {
                    continue;
                }

                if (memberSymbol.TryGetAttribute(attributeFullName!, out var attribute))
                {
                    yield return (memberSymbol, GetRegisteredName(memberSymbol, attribute));
                }
            }
        }
    }

    private static string GetRegisteredName(ISymbol symbol, AttributeData attribute)
    {
        foreach (var (key, constant) in attribute.NamedArguments)
        {
            if (key == "Name" && constant.Value is string name && !string.IsNullOrEmpty(name))
            {
                return name;
            }
        }

        if (attribute.AttributeClass?.FullQualifiedNameOmitGlobal() == KnownTypeNames.SignalAttribute)
        {
            // For signals, the registered name removes the 'EventHandler' suffix.
            return symbol.Name.Substring(0, symbol.Name.Length - "EventHandler".Length);
        }

        return symbol.Name;
    }
}

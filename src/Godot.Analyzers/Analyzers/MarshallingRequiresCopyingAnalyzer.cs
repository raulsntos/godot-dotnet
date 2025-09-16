using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class MarshallingRequiresCopyingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0003_MarhsallingRequiresCopying,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        switch (context.Symbol)
        {
            case IFieldSymbol fieldSymbol:
            {
                if (!context.Symbol.HasAttribute(KnownTypeNames.BindPropertyAttribute))
                {
                    return;
                }

                if (TypeRequiresCopying(context.Compilation, fieldSymbol.Type))
                {
                    var location = fieldSymbol.GetTypeSyntaxLocation();
                    if (location is null)
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0003_MarhsallingRequiresCopying,
                        location,
                        // Message Format parameters.
                        fieldSymbol.Type.ToDisplayString()
                    ));
                }

                break;
            }

            case IPropertySymbol propertySymbol:
            {
                if (!context.Symbol.HasAttribute(KnownTypeNames.BindPropertyAttribute))
                {
                    return;
                }

                if (TypeRequiresCopying(context.Compilation, propertySymbol.Type))
                {
                    var location = propertySymbol.GetTypeSyntaxLocation();
                    if (location is null)
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0003_MarhsallingRequiresCopying,
                        location,
                        // Message Format parameters.
                        propertySymbol.Type.ToDisplayString()
                    ));
                }

                break;
            }

            case IMethodSymbol methodSymbol:
            {
                if (!context.Symbol.HasAttribute(KnownTypeNames.BindMethodAttribute))
                {
                    return;
                }

                foreach (var parameterSymbol in methodSymbol.Parameters)
                {
                    if (TypeRequiresCopying(context.Compilation, parameterSymbol.Type))
                    {
                        var location = parameterSymbol.GetTypeSyntaxLocation();
                        if (location is null)
                        {
                            continue;
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.GODOT0003_MarhsallingRequiresCopying,
                            location,
                            // Message Format parameters.
                            parameterSymbol.Type.ToDisplayString()
                        ));
                    }
                }

                if (!methodSymbol.ReturnsVoid)
                {
                    if (TypeRequiresCopying(context.Compilation, methodSymbol.ReturnType))
                    {
                        var location = methodSymbol.GetTypeSyntaxLocation();
                        if (location is null)
                        {
                            return;
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.GODOT0003_MarhsallingRequiresCopying,
                            location,
                            // Message Format parameters.
                            methodSymbol.ReturnType.ToDisplayString()
                        ));
                    }
                }

                break;
            }

            case INamedTypeSymbol delegateSymbol when delegateSymbol.TypeKind == TypeKind.Delegate:
            {
                if (!context.Symbol.HasAttribute(KnownTypeNames.SignalAttribute))
                {
                    return;
                }

                var invokeMethod = delegateSymbol.DelegateInvokeMethod;
                if (invokeMethod is null)
                {
                    return;
                }

                foreach (var parameterSymbol in invokeMethod.Parameters)
                {
                    if (TypeRequiresCopying(context.Compilation, parameterSymbol.Type))
                    {
                        var location = parameterSymbol.GetTypeSyntaxLocation();
                        if (location is null)
                        {
                            continue;
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.GODOT0003_MarhsallingRequiresCopying,
                            location,
                            // Message Format parameters.
                            parameterSymbol.Type.ToDisplayString()
                        ));
                    }
                }

                break;
            }
        }
    }

    private static bool TypeRequiresCopying(Compilation compilation, ITypeSymbol type)
    {
        if (Marshalling.TryGetMarshallingInformation(compilation, type, out var marshalInfo))
        {
            // Specially-recognized types are types that can't be marshalled directly
            // but we can marshall them by copying them into a native interop Godot type.
            // For example, List<int> can be copied into a PackedInt32Array or GodotArray<int>.
            // This allows us to support very common types that users want to use,
            // but it has a performance cost due to the copying that is greater the more elements
            // there are in the collection.
            return marshalInfo.TypeIsSpeciallyRecognized;
        }

        return false;
    }
}

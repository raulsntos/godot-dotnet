using System.Collections.Immutable;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BindPropertyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create([
        Descriptors.GODOT0501_PropertyTypeIsNotSupported,
        Descriptors.GODOT0502_PropertyMustNotBeStatic,
        Descriptors.GODOT0503_PropertyMustNotBeConst,
        Descriptors.GODOT0504_PropertyMustNotBeReadOnly,
        Descriptors.GODOT0505_PropertyMustHaveGetter,
        Descriptors.GODOT0505_PropertyMustHaveSetter,
        Descriptors.GODOT0506_PropertyMustNotBeIndexer,
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (!context.Symbol.HasAttribute(KnownTypeNames.BindPropertyAttribute))
        {
            return;
        }

        switch (context.Symbol)
        {
            case IPropertySymbol propertySymbol:
            {
                if (!IsSupportedType(context.Compilation, propertySymbol.Type))
                {
                    var location = propertySymbol.GetTypeSyntaxLocation();
                    if (location is null)
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0501_PropertyTypeIsNotSupported,
                        location,
                        // Message Format parameters.
                        propertySymbol.Type.ToDisplayString()
                    ));
                }

                if (propertySymbol.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0502_PropertyMustNotBeStatic,
                        propertySymbol.Locations[0],
                        // Message Format parameters.
                        propertySymbol.Name
                    ));
                }

                if (propertySymbol.IsIndexer)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0506_PropertyMustNotBeIndexer,
                        propertySymbol.Locations[0]
                    ));
                }

                if (propertySymbol.GetMethod is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0505_PropertyMustHaveGetter,
                        propertySymbol.Locations[0],
                        // Message Format parameters.
                        propertySymbol.Name
                    ));
                }
                if (propertySymbol.SetMethod is null
                 || propertySymbol.SetMethod is { IsInitOnly: true })
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0505_PropertyMustHaveSetter,
                        propertySymbol.Locations[0],
                        // Message Format parameters.
                        propertySymbol.Name
                    ));
                }

                break;
            }

            case IFieldSymbol fieldSymbol:
            {
                if (!IsSupportedType(context.Compilation, fieldSymbol.Type))
                {
                    var location = fieldSymbol.GetTypeSyntaxLocation();
                    if (location is null)
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0501_PropertyTypeIsNotSupported,
                        location,
                        // Message Format parameters.
                        fieldSymbol.Type.ToDisplayString(),
                        fieldSymbol.Name
                    ));
                }

                // NOTE: 'const' fields are implicitly static and we don't want to report multiple diagnostics.
                if (fieldSymbol.IsStatic && !fieldSymbol.IsConst)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0502_PropertyMustNotBeStatic,
                        fieldSymbol.Locations[0],
                        // Message Format parameters.
                        fieldSymbol.Name
                    ));
                }

                if (fieldSymbol.IsConst)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0503_PropertyMustNotBeConst,
                        fieldSymbol.Locations[0],
                        // Message Format parameters.
                        fieldSymbol.Name
                    ));
                }

                if (fieldSymbol.IsReadOnly)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.GODOT0504_PropertyMustNotBeReadOnly,
                        fieldSymbol.Locations[0],
                        // Message Format parameters.
                        fieldSymbol.Name
                    ));
                }

                break;
            }
        }
    }

    private static bool IsSupportedType(Compilation compilation, ITypeSymbol type)
    {
        // Every property must be Variant compatible.
        return Marshalling.TryGetMarshallingInformation(compilation, type, out _);
    }
}

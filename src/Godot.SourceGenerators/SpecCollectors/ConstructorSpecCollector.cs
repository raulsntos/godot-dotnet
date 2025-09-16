using System.Threading;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

internal static class ConstructorSpecCollector
{
    public static GodotConstructorSpec? Collect(Compilation compilation, IMethodSymbol methodSymbol, CancellationToken cancellationToken = default)
    {
        if (!methodSymbol.HasAttribute(KnownTypeNames.BindConstructorAttribute))
        {
            // Method must have the attribute to be registered as a constructor.
            return null;
        }

        return new GodotConstructorSpec()
        {
            MethodSymbolName = methodSymbol.Name,
        };
    }
}

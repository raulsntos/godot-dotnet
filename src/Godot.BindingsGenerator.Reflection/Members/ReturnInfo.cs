using System;
using System.Diagnostics.CodeAnalysis;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Helper methods to create <see cref="ParameterInfo"/> to be used as return parameters.
/// </summary>
public static class ReturnInfo
{
    /// <summary>
    /// Constructs a <see cref="ParameterInfo"/> to be used as the return parameter
    /// of a <see cref="MethodInfo"/> or <see langword="null"/> if <paramref name="type"/>
    /// is <see langword="null"/>.
    /// </summary>
    /// <param name="type">The return type.</param>
    /// <param name="refKind">The return ref kind.</param>
    /// <returns>The constructed <see cref="ParameterInfo"/> or <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(type))]
    public static ParameterInfo? FromTypeOrNull(TypeInfo? type, RefKind refKind = RefKind.None)
    {
        if (type is null)
        {
            return null;
        }

        return new ParameterInfo("return", type)
        {
            RefKind = refKind,
        };
    }

    /// <summary>
    /// Constructs a <see cref="ParameterInfo"/> to be used as the return parameter
    /// of a <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="type">The return type.</param>
    /// <param name="refKind">The return ref kind.</param>
    /// <returns>The constructed <see cref="ParameterInfo"/>.</returns>
    public static ParameterInfo FromType(TypeInfo type, RefKind refKind = RefKind.None)
    {
        ArgumentNullException.ThrowIfNull(type);

        return new ParameterInfo("return", type)
        {
            RefKind = refKind,
        };
    }
}

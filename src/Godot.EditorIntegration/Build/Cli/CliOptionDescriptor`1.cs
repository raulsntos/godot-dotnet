using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Godot.EditorIntegration.Build.Cli;

/// <summary>
/// A typed CLI option descriptor, it defines the option and how it will be added
/// to the arguments of a <see cref="Process"/>.
/// </summary>
internal sealed class CliOptionDescriptor<T>
{
    private readonly Action<T, IList<string>> _appendArguments;

    /// <summary>
    /// Construct a <see cref="CliOptionDescriptor{T}"/> with the append behavior
    /// implemented by <paramref name="appendArguments"/>.
    /// </summary>
    /// <param name="appendArguments">
    /// Implements the behavior that defines how to add the CLI option value to the
    /// arguments of a <see cref="Process"/>.
    /// </param>
    public CliOptionDescriptor(Action<T, IList<string>> appendArguments)
    {
        _appendArguments = appendArguments;
    }

    /// <summary>
    /// Construct a <see cref="CliOption{T}"/> with the append behavior
    /// of this CLI option and the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value that will be used with this option.</param>
    /// <returns>A CLI option mapped to a value.</returns>
    public CliOption<T> WithValue(T value)
    {
        return new CliOption<T>(value, _appendArguments);
    }
}

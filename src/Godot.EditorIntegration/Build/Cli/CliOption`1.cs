using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Godot.EditorIntegration.Build.Cli;

/// <summary>
/// A typed CLI option, it defines how to add its value to the arguments of a <see cref="Process"/>.
/// </summary>
internal sealed class CliOption<T> : CliOption
{
    private readonly T _value;

    private readonly Action<T, IList<string>> _appendArguments;

    /// <summary>
    /// Construct a <see cref="CliOption{T}"/> with the given <paramref name="value"/>
    /// and the append behavior implemented by <paramref name="appendArguments"/>.
    /// </summary>
    /// <param name="value">The value mapped to this CLI option.</param>
    /// <param name="appendArguments">
    /// Implements the behavior that defines how to add the CLI option value to the
    /// arguments of a <see cref="Process"/>.
    /// </param>
    public CliOption(T value, Action<T, IList<string>> appendArguments)
    {
        _value = value;
        _appendArguments = appendArguments;
    }

    /// <inheritdoc/>
    public override void AppendArguments(IList<string> arguments)
    {
        _appendArguments(_value, arguments);
    }
}

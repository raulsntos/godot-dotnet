using System.Collections.Generic;
using System.Diagnostics;

namespace Godot.EditorIntegration.Build.Cli;

/// <summary>
/// A CLI option, it defines how to add its value to the arguments of a <see cref="Process"/>.
/// </summary>
internal abstract class CliOption
{
    /// <summary>
    /// Create a <see cref="CliOption"/> for an argument that will be
    /// always added unconditionally.
    /// </summary>
    /// <param name="argument">The argument that will be added.</param>
    public static implicit operator CliOption(string argument) =>
        new ImmutableCliOption(argument);

    /// <summary>
    /// Get an empty <see cref="CliOption"/> that adds no arguments.
    /// </summary>
    public static CliOption Empty { get; } = new EmptyCliOption();

    /// <summary>
    /// Convert this CLI option to arguments and add them to the list of arguments
    /// of a <see cref="Process"/> instance.
    /// </summary>
    /// <param name="arguments">Existing list of arguments.</param>
    public abstract void AppendArguments(IList<string> arguments);

    private sealed class ImmutableCliOption : CliOption
    {
        private readonly string _value;

        public ImmutableCliOption(string value)
        {
            _value = value;
        }

        public override void AppendArguments(IList<string> arguments)
        {
            arguments.Add(_value);
        }
    }

    private sealed class EmptyCliOption : CliOption
    {
        public override void AppendArguments(IList<string> arguments)
        {
            // This option doesn't add anything to the arguments.
        }
    }
}

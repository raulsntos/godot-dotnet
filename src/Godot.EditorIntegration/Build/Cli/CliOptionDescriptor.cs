using System;
using System.Diagnostics;

namespace Godot.EditorIntegration.Build.Cli;

/// <summary>
/// A CLI option descriptor, it defines the option and how it will be added
/// to the arguments of a <see cref="Process"/>.
/// </summary>
internal static class CliOptionDescriptor
{
    /// <summary>
    /// Create a <see cref="CliOptionDescriptor"/> for simple boolean values where
    /// <paramref name="argument"/> is added if the value is <see langword="true"/>.
    /// </summary>
    /// <param name="argument">
    /// The argument that will be added to the process if the value is <see langword="true"/>.
    /// </param>
    public static CliOptionDescriptor<bool> FromBoolean(string argument) =>
        new((value, args) =>
        {
            if (value)
            {
                args.Add(argument);
            }
        });

    /// <summary>
    /// Create a <see cref="CliOptionDescriptor"/> for simple string values where
    /// <paramref name="argument"/> and the value are added as 2 separate arguments
    /// if the value is not <see langword="null"/>.
    /// </summary>
    /// <param name="argument">
    /// The argument that will be added before the value if the value is not <see langword="null"/>.
    /// </param>
    public static CliOptionDescriptor<string?> FromString(string argument) =>
        new((value, args) =>
        {
            if (!string.IsNullOrEmpty(value))
            {
                args.Add(argument);
                args.Add(value);
            }
        });

    /// <summary>
    /// Construct a <see cref="CliOption{T}"/> with the append behavior of
    /// <paramref name="cliOption"/> and the value determined by <paramref name="getValue"/>
    /// if <paramref name="enabled"/> is <see langword="true"/>; otherwise, an empty
    /// <see cref="CliOption"/>.
    /// </summary>
    /// <param name="cliOption">The option that determines the append behavior.</param>
    /// <param name="enabled">Whether to construct an option with a value or return an empty option.</param>
    /// <param name="getValue">Retrieves the value that will be mapped to the option.</param>
    /// <returns>An option mapped to a value or an empty option.</returns>
    public static CliOption WithValueIfEnabled<T>(this CliOptionDescriptor<T> cliOption, bool enabled, Func<T> getValue)
    {
        if (enabled)
        {
            return cliOption.WithValue(getValue());
        }

        return CliOption.Empty;
    }
}

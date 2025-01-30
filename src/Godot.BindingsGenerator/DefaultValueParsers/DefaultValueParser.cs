using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal abstract partial class DefaultValueParser
{
    [GeneratedRegex("^(?<type_name>[A-z0-9]+)\\((?<args>.*)\\)$")]
    private static partial Regex ConstructorExpressionRegex { get; }

    private TypeDB? _typeDB;

    /// <summary>
    /// Parse a default value expression from the API JSON dump
    /// and return the equivalent C# expression as a string.
    /// </summary>
    /// <param name="engineDefaultValueExpression">Default value expression.</param>
    /// <returns>Default value expression for C#.</returns>
    public string Parse(string engineDefaultValueExpression, TypeDB typeDB)
    {
        try
        {
            _typeDB = typeDB;
            return ParseCore(engineDefaultValueExpression);
        }
        finally
        {
            _typeDB = null;
        }
    }

    protected abstract string ParseCore(string engineDefaultValueExpression);

    public bool TryGetDefaultValueExpression(TypeInfo type, string engineDefaultValueExpression, [NotNullWhen(true)] out string? defaultValueExpression)
    {
        if (_typeDB is null)
        {
            throw new InvalidOperationException($"Can't retrieve a default value expression. This method must be called inside the {nameof(ParseCore)} implementation.");
        }

        return _typeDB.TryGetDefaultValueExpression(type, engineDefaultValueExpression, out defaultValueExpression);
    }

    public static bool TryParseDefaultExpressionAsConstructor(string engineDefaultValueExpression, [NotNullWhen(true)] out string? engineTypeName, [NotNullWhen(true)] out string[]? constructorArguments)
    {
        var match = ConstructorExpressionRegex.Match(engineDefaultValueExpression);
        if (match.Success)
        {
            engineTypeName = match.Groups["type_name"].Value;
            var argsSpan = match.Groups["args"].ValueSpan;

            List<string> ctorArgs = [];
            {
                var remaining = argsSpan;
                while (!remaining.IsEmpty)
                {
                    int idx = remaining.IndexOf(',');
                    if ((uint)idx < (uint)remaining.Length)
                    {
                        // A delimiter character was found.
                        ctorArgs.Add(remaining[..idx].Trim().ToString());
                        remaining = remaining[(idx + 1)..];
                    }
                    else
                    {
                        // We've reached EOF, but we still need to add the remaining content as the last argument.
                        ctorArgs.Add(remaining.Trim().ToString());
                        remaining = default;
                    }
                }
            }
            constructorArguments = ctorArgs.ToArray();

            return true;
        }

        engineTypeName = null;
        constructorArguments = null;
        return false;
    }
}

using System;
using System.Diagnostics.CodeAnalysis;

namespace Godot;

/// <summary>
/// Exception that is thrown when there is an attempt to access a method in
/// Godot API that could not be found in the engine.
/// Likely because the native method has been removed or changed to an
/// incompatible version.
/// </summary>
public sealed class MissingGodotMethodException : MissingGodotMemberException
{
    internal MissingGodotMethodException() { }

    internal MissingGodotMethodException(string message) : base(message) { }

    internal MissingGodotMethodException(string message, Exception innerException) : base(message, innerException) { }

    internal static unsafe void ThrowIfNull(void* methodBind, string message)
    {
        if (methodBind is null)
        {
            Throw(message);
        }
    }

    [DoesNotReturn]
    internal static void Throw(string message)
    {
        throw new MissingGodotMethodException(message);
    }
}

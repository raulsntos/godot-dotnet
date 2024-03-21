using System;

namespace Godot;

/// <summary>
/// Exception that is thrown when there is an attempt to access a member in
/// Godot API that could not be found in the engine.
/// Likely because the native member has been removed or changed to an
/// incompatible version.
/// </summary>
public class MissingGodotMemberException : Exception
{
    internal MissingGodotMemberException() { }

    internal MissingGodotMemberException(string message) : base(message) { }

    internal MissingGodotMemberException(string message, Exception innerException) : base(message, innerException) { }
}

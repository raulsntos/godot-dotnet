using System;

namespace Godot.EditorIntegration.Build;

/// <summary>
/// OS name part of the .NET runtime identifier (RID).
/// See https://docs.microsoft.com/en-us/dotnet/core/rid-catalog.
/// </summary>
internal readonly struct RuntimeIdentifierOS : IEquatable<RuntimeIdentifierOS>
{
    public static RuntimeIdentifierOS Win => new("win");
    public static RuntimeIdentifierOS OSX => new("osx");
    public static RuntimeIdentifierOS Linux => new("linux");
    public static RuntimeIdentifierOS Android => new("android");
    public static RuntimeIdentifierOS IOS => new("ios");
    public static RuntimeIdentifierOS IOSSimulator => new("iossimulator");
    public static RuntimeIdentifierOS Browser => new("browser");

    private readonly string? _value;

    public string Value => _value ?? "";

    public static implicit operator string(RuntimeIdentifierOS os) => os.Value;

    public RuntimeIdentifierOS(string value)
    {
        _value = value;
    }

    public bool Equals(RuntimeIdentifierOS other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RuntimeIdentifierOS os && Equals(os);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}

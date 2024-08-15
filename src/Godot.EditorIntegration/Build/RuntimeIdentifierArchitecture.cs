using System;

namespace Godot.EditorIntegration.Build;

/// <summary>
/// Architecture part of the .NET runtime identifier (RID).
/// See https://docs.microsoft.com/en-us/dotnet/core/rid-catalog.
/// </summary>
internal readonly struct RuntimeIdentifierArchitecture : IEquatable<RuntimeIdentifierArchitecture>
{
    public static RuntimeIdentifierArchitecture X86 => new("x86");
    public static RuntimeIdentifierArchitecture X64 => new("x64");
    public static RuntimeIdentifierArchitecture Arm => new("arm");
    public static RuntimeIdentifierArchitecture Arm64 => new("arm64");
    public static RuntimeIdentifierArchitecture RiscV64 => new("riscv64");
    public static RuntimeIdentifierArchitecture Ppc64le => new("ppc64le");
    public static RuntimeIdentifierArchitecture Wasm => new("wasm");

    private readonly string? _value;

    public string Value => _value ?? "";

    public static implicit operator string(RuntimeIdentifierArchitecture architecture) => architecture.Value;

    public RuntimeIdentifierArchitecture(string value)
    {
        _value = value;
    }

    public bool Equals(RuntimeIdentifierArchitecture other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RuntimeIdentifierArchitecture architecture && Equals(architecture);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Get the equivalent Godot architecture name for the given runtime identifier architecture.
    /// </summary>
    /// <param name="architecture">The architecture part of a .NET runtime identifier.</param>
    /// <returns>The Godot name for the architecture.</returns>
    internal static string ToGodot(RuntimeIdentifierArchitecture architecture)
    {
        return architecture switch
        {
            _ when architecture == X86 => "x86_32",
            _ when architecture == X64 => "x86_64",
            _ when architecture == Arm => "arm32",
            _ when architecture == Arm64 => "arm64",
            _ when architecture == RiscV64 => "rv64",
            _ when architecture == Ppc64le => "ppc64",
            _ when architecture == Wasm => "wasm32",
            _ => throw new ArgumentException(SR.FormatArgument_ArchitectureNotRecognizedFromRuntimeIdentifier(architecture)),
        };
    }
}

using System;
using System.Runtime.InteropServices;
using Godot.NativeInterop;

namespace Godot.Bridge;

internal sealed class GodotVersion
{
    /// <summary>
    /// Major part of the version (e.g.: 3).
    /// </summary>
    public required int Major { get; internal init; }

    /// <summary>
    /// Minor part of the version (e.g.: 1).
    /// </summary>
    public required int Minor { get; internal init; }

    /// <summary>
    /// Patch part of the version (e.g.: 4).
    /// </summary>
    public required int Patch { get; internal init; }

    /// <summary>
    /// Status part of the version (e.g. "stable", "beta", "rc1", "rc2").
    /// </summary>
    public required string? Status { get; internal init; }

    /// <summary>
    /// Build name (e.g. "custom_build").
    /// </summary>
    public required string? Build { get; internal init; }

    /// <summary>
    /// Full git commit hash.
    /// </summary>
    public required string? Hash { get; internal init; }

    /// <summary>
    /// Unix timestamp in seconds for the git commit, or 0 if unavailable.
    /// </summary>
    public required long TimeStamp { get; internal init; }

    /// <summary>
    /// Version formatted as a string suitable to display to users (e.g.: "Godot v3.1.4.stable.official.mono").
    /// </summary>
    public required string? DisplayString { get; internal init; }

    public override string? ToString() => DisplayString;

    internal static unsafe GodotVersion Create(GDExtensionGodotVersion2 value)
    {
        return new GodotVersion()
        {
            Major = checked((int)value.major),
            Minor = checked((int)value.minor),
            Patch = checked((int)value.patch),

            Status = Marshal.PtrToStringUTF8((nint)value.status),
            Build = Marshal.PtrToStringUTF8((nint)value.build),
            Hash = Marshal.PtrToStringUTF8((nint)value.hash),

            TimeStamp = checked((long)value.timestamp),

            DisplayString = Marshal.PtrToStringUTF8((nint)value.@string),
        };
    }

    /// <summary>
    /// Get the version of the Godot .NET packages that corresponds to this version of Godot.
    /// </summary>
    /// <returns>Godot .NET version for this version of Godot.</returns>
    internal string GetGodotDotNetVersion()
    {
        if (string.IsNullOrEmpty(Status) || Status == "stable")
        {
            return $"{Major}.{Minor}.{Patch}";
        }

        int statusNumberStartIndex = Status.AsSpan().IndexOfAnyInRange('0', '9');
        if (statusNumberStartIndex == -1)
        {
            // Godot .NET versioning uses 'dev' for all development releases.
            return $"{Major}.{Minor}.{Patch}-dev";
        }

        var statusLabel = Status.AsSpan(0, statusNumberStartIndex);
        var statusNumber = Status.AsSpan(statusNumberStartIndex);

        // Godot .NET versioning uses 'alpha' instead of 'dev' for official numbered releases.
        if (statusLabel.Equals("dev", StringComparison.Ordinal))
        {
            statusLabel = "alpha";
        }

        return $"{Major}.{Minor}.{Patch}-{statusLabel}.{statusNumber}";
    }
}

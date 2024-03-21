using System;
using Godot.BindingsGenerator.ApiDump;

namespace Godot.BindingsGenerator;

internal sealed class BindingsGeneratorOptions
{
    public enum ArchBits { Bits32, Bits64 };

    public ArchBits Bits { get; init; } = ArchBits.Bits64;

    public enum FloatTypePrecision { SinglePrecision, DoublePrecision };

    public FloatTypePrecision FloatPrecision { get; init; } = FloatTypePrecision.SinglePrecision;

    public GodotBuildConfiguration BuildConfiguration =>
        (FloatPrecision, Bits) switch
        {
            (FloatTypePrecision.SinglePrecision, ArchBits.Bits32) => GodotBuildConfiguration.Float32,
            (FloatTypePrecision.SinglePrecision, ArchBits.Bits64) => GodotBuildConfiguration.Float64,
            (FloatTypePrecision.DoublePrecision, ArchBits.Bits32) => GodotBuildConfiguration.Double32,
            (FloatTypePrecision.DoublePrecision, ArchBits.Bits64) => GodotBuildConfiguration.Double64,
            _ => throw new InvalidOperationException($"Unrecognized build configuration {FloatPrecision}_{Bits}."),
        };
}

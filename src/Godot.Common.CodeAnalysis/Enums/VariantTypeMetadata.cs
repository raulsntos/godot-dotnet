using System;

namespace Godot.Common.CodeAnalysis;

// IMPORTANT: Must match the type defined in GodotBindings/Bridge/VariantTypeMetadata.cs

internal enum VariantTypeMetadata : uint
{
    None = 0,
    SByte = 1,
    Int16 = 2,
    Int32 = 3,
    Int64 = 4,
    Byte = 5,
    UInt16 = 6,
    UInt32 = 7,
    UInt64 = 8,
    Single = 9,
    Double = 10,
    Char16 = 11,
    Char32 = 12,
}

internal static class VariantTypeMetadataExtensions
{
    public static string FullNameWithGlobal(this VariantTypeMetadata variantTypeMetadata)
    {
        if (!Enum.IsDefined(typeof(VariantTypeMetadata), variantTypeMetadata))
        {
            throw new ArgumentOutOfRangeException(nameof(variantTypeMetadata), $"Unrecognized VariantTypeMetadata value '{variantTypeMetadata}'.");
        }

        return $"global::Godot.Bridge.VariantTypeMetadata.{Enum.GetName(typeof(VariantTypeMetadata), variantTypeMetadata)}";
    }
}

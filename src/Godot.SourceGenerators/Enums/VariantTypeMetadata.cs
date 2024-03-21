using System;

namespace Godot.SourceGenerators;

// IMPORTANT: Must match the type defined in GodotBindings/Bridge/ClassDB/VariantTypeMetadata.cs

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

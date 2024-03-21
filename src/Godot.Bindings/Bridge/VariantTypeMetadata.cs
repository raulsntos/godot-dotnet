using System.Diagnostics.CodeAnalysis;
using Godot.NativeInterop;

namespace Godot.Bridge;

/// <summary>
/// Describes the real type that a Variant represents.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Enum field names match the C# type names intentionally.")]
public enum VariantTypeMetadata : uint
{
    /// <summary>
    /// Variant type has no metadata.
    /// </summary>
    None = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_NONE,

    /// <summary>
    /// Variant integer represents a <see langword="sbyte"/>.
    /// </summary>
    SByte = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_INT8,

    /// <summary>
    /// Variant integer represents a <see langword="short"/>.
    /// </summary>
    Int16 = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_INT16,

    /// <summary>
    /// Variant integer represents an <see langword="int"/>.
    /// </summary>
    Int32 = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_INT32,

    /// <summary>
    /// Variant integer represents a <see langword="long"/>.
    /// </summary>
    Int64 = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_INT64,

    /// <summary>
    /// Variant integer represents a <see langword="byte"/>.
    /// </summary>
    Byte = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_UINT8,

    /// <summary>
    /// Variant integer represents an <see langword="ushort"/>.
    /// </summary>
    UInt16 = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_UINT16,

    /// <summary>
    /// Variant integer represents an <see langword="uint"/>.
    /// </summary>
    UInt32 = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_UINT32,

    /// <summary>
    /// Variant integer represents a <see langword="ulong"/>.
    /// </summary>
    UInt64 = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_INT_IS_UINT64,

    /// <summary>
    /// Variant floating point represents a <see langword="float"/>.
    /// </summary>
    Single = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_REAL_IS_FLOAT,

    /// <summary>
    /// Variant floating point represents a <see langword="double"/>.
    /// </summary>
    Double = GDExtensionClassMethodArgumentMetadata.GDEXTENSION_METHOD_ARGUMENT_METADATA_REAL_IS_DOUBLE,
}
